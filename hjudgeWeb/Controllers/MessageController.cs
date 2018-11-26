using hjudgeWeb.Configurations;
using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Hubs;
using hjudgeWeb.Models;
using hjudgeWeb.Models.Message;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        public async Task<List<ChatMessageModel>> GetChats(int startId = int.MaxValue, int count = 10, int problemId = 0, int contestId = 0, int groupId = 0)
        {
            using (var db = new ApplicationDbContext(_dbContextOptions))
            {
                var pid = problemId == 0 ? null : (int?)problemId;
                var cid = contestId == 0 ? null : (int?)contestId;
                var gid = groupId == 0 ? null : (int?)groupId;

                if (cid != null)
                {
                    var contest = await db.Contest.Select(i => new { i.Id, i.Config }).FirstOrDefaultAsync(i => i.Id == cid);
                    if (contest != null)
                    {
                        var config = JsonConvert.DeserializeObject<ContestConfiguration>(contest.Config ?? "{}");
                        if (!config.CanDisscussion)
                        {
                            return new List<ChatMessageModel>();
                        }
                    }
                }

                return await db.Discussion.Include(i => i.UserInfo)
                    .OrderByDescending(i => i.Id)
                    .Where(i => i.Id < startId
                        && i.ProblemId == pid
                        && i.ContestId == cid
                        && i.GroupId == gid)
                    .Select(i => new ChatMessageModel
                    {
                        Id = i.Id,
                        UserId = i.UserId,
                        Content = i.Content,
                        RawSendTime = i.SubmitTime,
                        UserName = i.UserInfo.UserName,
                        ReplyId = i.ReplyId
                    }).Take(count)
                    .OrderBy(i => i.RawSendTime).ToListAsync();
            }
        }

        public class SendChatModel
        {
            public string Content { get; set; }
            public int ReplyId { get; set; }
            public int? ProblemId { get; set; }
            public int? ContestId { get; set; }
            public int? GroupId { get; set; }
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


                var pid = model.ProblemId == 0 ? null : model.ProblemId;
                var cid = model.ContestId == 0 ? null : model.ContestId;
                var gid = model.GroupId == 0 ? null : model.GroupId;

                using (var db = new ApplicationDbContext(_dbContextOptions))
                {
                    if (cid != null)
                    {
                        var contest = await db.Contest.Select(i => new { i.Id, i.Config }).FirstOrDefaultAsync(i => i.Id == cid);
                        if (contest != null)
                        {
                            var config = JsonConvert.DeserializeObject<ContestConfiguration>(contest.Config ?? "{}");
                            if (!config.CanDisscussion)
                            {
                                ret.ErrorMessage = "此比赛不允许参与讨论";
                                ret.IsSucceeded = false;
                                return ret;
                            }
                        }
                    }

                    var lastSubmit = await db.Discussion.OrderByDescending(i => i.SubmitTime).FirstOrDefaultAsync(i => i.UserId == user.Id);
                    if (lastSubmit != null && (DateTime.Now - lastSubmit.SubmitTime) < TimeSpan.FromSeconds(10))
                    {
                        ret.ErrorMessage = "消息发送过于频繁，请等待 10 秒后再试";
                        ret.IsSucceeded = false;
                        return ret;
                    }
                    if (model.ReplyId != 0)
                    {
                        var previousDis = await db.Discussion
                                .Include(i => i.UserInfo)
                                .Include(i => i.Problem)
                                .Include(i => i.Contest)
                                .Include(i => i.Group)
                                .Select(i => new
                                {
                                    i.Id,
                                    i.UserId,
                                    i.ProblemId,
                                    ProblemName = i.Problem.Name,
                                    i.ContestId,
                                    ContestName = i.Contest.Name,
                                    i.GroupId,
                                    GroupName = i.Group.Name,
                                    i.Content,
                                    i.SubmitTime
                                }).FirstOrDefaultAsync(i => i.Id == model.ReplyId);

                        if (previousDis != null)
                        {
                            var link = string.Empty;
                            var position = string.Empty;
                            if (cid == null)
                            {
                                if (pid == null)
                                {
                                    link = "/";
                                    position = "主页";
                                }
                                else
                                {
                                    link = $"/ProblemDetails/{pid}";
                                    position = $"题目 {pid} - {previousDis.ProblemName}";
                                }
                            }
                            else
                            {
                                if (pid == null)
                                {
                                    link = $"/ContestDetails/{cid}";
                                    position = $"比赛 {cid} - {previousDis.ContestName}";
                                }
                                else
                                {
                                    link = $"/ProblemDetails/{cid}/{pid}";
                                    position = $"比赛 {cid} - {previousDis.ContestName}，题目 {pid} - {previousDis.ProblemName}";
                                }
                            }
                            var msgContent = new MessageContent
                            {
                                Content = $"<h3>回复了您的帖子 #{previousDis.Id}：</h3><br />" +
                                "<div style=\"width: 90 %; overflow: auto; max-height: 100px; \">" +
                                $"<pre style=\"white-space: pre-wrap; word-wrap: break-word;\">{new string(content.Take(128).ToArray()) + (content.Length > 128 ? "..." : string.Empty)}</pre>" +
                                "<h3>原帖内容：</h3><br />" +
                                $"<pre style=\"white-space: pre-wrap; word-wrap: break-word;\">{new string(previousDis.Content.Take(128).ToArray()) + (previousDis.Content.Length > 128 ? "..." : string.Empty)}</pre>" +
                                $"<hr /><p>位置：{position}，<a href=\"{link}\">点此前往查看</a></p>"
                            };
                            db.MessageContent.Add(msgContent);
                            await db.SaveChangesAsync();
                            db.Message.Add(new Message
                            {
                                FromUserId = user.Id,
                                ToUserId = previousDis.UserId,
                                ContentId = msgContent.Id,
                                ReplyId = 0,
                                SendTime = DateTime.Now,
                                Status = 1,
                                Title = $"您的帖子 #{previousDis.Id} 有新的回复",
                                Type = 1
                            });
                        }
                    }

                    var dis = new Discussion
                    {
                        UserId = user.Id,
                        Content = content,
                        SubmitTime = sendTime,
                        ReplyId = model.ReplyId,
                        ProblemId = pid,
                        ContestId = cid,
                        GroupId = gid
                    };
                    db.Discussion.Add(dis);

                    await db.SaveChangesAsync();
                    await _chatHub.Clients.All.SendAsync("ChatMessage", dis.Id, user.Id, user.UserName, $"{sendTime.ToShortDateString()} {sendTime.ToLongTimeString()}", content, model.ReplyId);
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