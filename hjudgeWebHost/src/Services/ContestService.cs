using hjudgeWebHost.Data;
using hjudgeWebHost.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IContestService
    {
        Task<IQueryable<Contest>> QueryContestAsync(string? userId, ApplicationDbContext dbContext);
        Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId, ApplicationDbContext dbContext);
    }
    public class ContestService : IContestService
    {
        private readonly UserManager<UserInfo> userManager;

        public ContestService(UserManager<UserInfo> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId, ApplicationDbContext dbContext)
        {
            var user = await userManager.FindByIdAsync(userId);

            IQueryable<Contest> contests = dbContext.Contest;

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                contests = contests.Where(i => !i.Hidden);
            }
            return contests;
        }

        public async Task<IQueryable<Contest>> QueryContestAsync(string? userId, int groupId, ApplicationDbContext dbContext)
        {
            var user = await userManager.FindByIdAsync(userId);
            var group = await dbContext.Group.FirstOrDefaultAsync(i => i.Id == groupId);
            if (group == null) throw new InvalidOperationException("找不到小组") { HResult = (int)ErrorDescription.ResourceNotFound };

            if (!Utils.PrivilegeHelper.IsTeacher(user?.Privilege))
            {
                if (group.IsPrivate)
                {
                    if (!dbContext.GroupJoin.Any(i => i.GroupId == groupId && i.UserId == userId))
                        throw new InvalidOperationException("未参加此小组") { HResult = (int)ErrorDescription.NoEnoughPrivilege };
                }
            }

            IQueryable<Contest> contests = dbContext.GroupContestConfig.Include(i => i.Contest).Where(i => i.GroupId == groupId).Select(i => i.Contest);

            return contests;
        }
    }
}
