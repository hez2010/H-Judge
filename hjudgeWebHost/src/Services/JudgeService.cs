using hjudgeWebHost.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IJudgeService
    {
        Task<IQueryable<Judge>> QueryJudgesAsync(int? groupId, int? contestId, int? problemId);
        Task<IQueryable<Judge>> QueryJudgesAsync(string userId, int? groupId, int? contestId, int? problemId);
        Task<Judge> GetJudgeAsync(int judgeId);
        Task QueueJudgeAsync(Judge judge);
    }
    public class JudgeService : IJudgeService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ICacheService cacheService;

        public JudgeService(ApplicationDbContext dbContext, ICacheService cacheService)
        {
            this.dbContext = dbContext;
            this.cacheService = cacheService;
        }

        public Task<Judge> GetJudgeAsync(int judgeId)
        {
            return cacheService.GetObjectAndSetAsync($"judge_{judgeId}", () => dbContext.Judge.FirstOrDefaultAsync(i => i.Id == judgeId));
        }

        public Task<IQueryable<Judge>> QueryJudgesAsync(int? groupId, int? contestId, int? problemId)
        {
            return Task.FromResult(problemId switch
            {
                null => dbContext.Judge.Where(i => i.GroupId == null && i.ContestId == null),
                _ => dbContext.Judge.Where(i => i.GroupId == null && i.ContestId == null && i.ProblemId == problemId)
            });
        }

        public Task<IQueryable<Judge>> QueryJudgesAsync(string userId, int? groupId, int? contestId, int? problemId)
        {
            return Task.FromResult(problemId switch
            {
                null => dbContext.Judge.Where(i => i.UserId == userId && i.GroupId == groupId && i.ContestId == contestId),
                _ => dbContext.Judge.Where(i => i.UserId == userId && i.GroupId == groupId && i.ContestId == contestId && i.ProblemId == problemId)
            });
        }

        public Task QueueJudgeAsync(Judge judge)
        {
            throw new System.NotImplementedException();
        }
    }
}
