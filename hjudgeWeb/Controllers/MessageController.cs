using hjudgeWeb.Data;
using hjudgeWeb.Data.Identity;
using hjudgeWeb.Hubs;
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
                        UserName = i.UserInfo.UserName
                    }).Take(count).OrderBy(i => i.RawSendTime).ToListAsync();
            }
        }

        public class SendChatModel
        {
            public string Content { get; set; }
        }

        [HttpPost]
        public async Task<bool> SendChat([FromBody]SendChatModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (_signInManager.IsSignedIn(User) && user != null)
            {
                try
                {
                    var sendTime = DateTime.Now;
                    var content = HttpUtility.HtmlEncode(model.Content);
                    using (var db = new ApplicationDbContext(_dbContextOptions))
                    {
                        db.Discussion.Add(new Discussion
                        {
                            UserId = user.Id,
                            Content = content,
                            SubmitTime = sendTime,
                        });
                        await db.SaveChangesAsync();
                    }
                    await _chatHub.Clients.All.SendAsync("ChatMessage", user.Id, user.UserName, $"{sendTime.ToShortDateString()} {sendTime.ToLongTimeString()}", content);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
}