using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Hubs;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Message;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace hjudgeWeb.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class MessageController : Controller
    {
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly UserManager<UserInfo> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly IHubContext<ChatHub> _chatHub;

        public MessageController(SignInManager<UserInfo> signInManager,
            UserManager<UserInfo> userManager,
            DbContextOptions<ApplicationDbContext> dbContextOptions,
            IHubContext<ChatHub> chatHub)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContextOptions = dbContextOptions;
            _chatHub = chatHub;
        }

        /// <summary>
        /// Get current signed in user and its privilege
        /// </summary>
        /// <returns></returns>
        private async Task<(UserInfo, int)> GetUserPrivilegeAsync()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return (null, 0);
            }
            var user = await _userManager.GetUserAsync(User);
            return (user, user?.Privilege ?? 0);
        }

        private bool HasAdminPrivilege(int privilege)
        {
            return privilege >= 1 && privilege <= 3;
        }

        [HttpGet]
        public async Task<List<ChatMessageModel>> GetChats(int startId = int.MaxValue, int count = 10)
        {
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                return await db.Discussion.Include(i => i.UserInfo).OrderByDescending(i => i.Id).Where(i => i.Id < startId && i.ProblemId == null && i.ContestId == null && i.GroupId == null)
                    .Select(i => new ChatMessageModel
                    {
                        Id = i.Id,
                        UserId = i.UserId,
                        Content = i.Content,
                        RawSendTime = i.SubmitTime,
                        UserName = i.UserInfo.UserName,
                        ReplyId = i.ReplyId
                    }).Take(count).OrderBy(i => i.RawSendTime).ToListAsync();
            }
        }

        public class SendChatModel
        {
            public string Content { get; set; }
            public int ReplyId { get; set; }
        }

        [HttpPost]
        public async Task<ResultModel> SendChat([FromBody]SendChatModel model)
        {
            var ret = new ResultModel { IsSucceeded = true };
            var (user, privilege) = await GetUserPrivilegeAsync();
            if (_signInManager.IsSignedIn(User) && user != null)
            {
                if (!user.EmailConfirmed)
                {
                    ret.ErrorMessage = "没有验证邮箱";
                    ret.IsSucceeded = false;
                    return ret;
                }
                if (user.Coins < 10 && !HasAdminPrivilege(privilege))
                {
                    ret.ErrorMessage = "金币余额不足";
                    ret.IsSucceeded = false;
                    return ret;
                }
                if (string.IsNullOrWhiteSpace(model.Content))
                {
                    ret.ErrorMessage = "请输入消息内容";
                    ret.IsSucceeded = false;
                    return ret;
                }
                if (model.Content.Length > 65536)
                {
                    ret.ErrorMessage = "消息内容过长";
                    ret.IsSucceeded = false;
                    return ret;
                }
                var sendTime = DateTime.Now;
                var content = HttpUtility.HtmlEncode(model.Content);
                using (var db = new ApplicationDbContext(_dbContextOptions))
                {
                    var lastSubmit = await db.Discussion.OrderByDescending(i => i.SubmitTime).FirstOrDefaultAsync(i => i.UserId == user.Id);
                    if (lastSubmit != null && (DateTime.Now - lastSubmit.SubmitTime) < TimeSpan.FromSeconds(10))
                    {
                        ret.ErrorMessage = "消息发送过于频繁，请等待 10 秒后再试";
                        ret.IsSucceeded = false;
                        return ret;
                    }
                    var diss = new Discussion
                    {
                        UserId = user.Id,
                        Content = content,
                        SubmitTime = sendTime,
                        ReplyId = model.ReplyId
                    };
                    db.Discussion.Add(diss);
                    await db.SaveChangesAsync();
                    await _chatHub.Clients.All.SendAsync("ChatMessage", diss.Id, user.Id, user.UserName, $"{sendTime.ToShortDateString()} {sendTime.ToLongTimeString()}", content, model.ReplyId);
                }
                user.Experience += 5;
                if (!HasAdminPrivilege(privilege))
                {
                    user.Coins -= 10;
                }
                await _userManager.UpdateAsync(user);
            }
            else
            {
                ret.ErrorMessage = "没有登录";
                ret.IsSucceeded = false;
            }
            return ret;
        }
    }
}