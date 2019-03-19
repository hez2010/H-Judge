using hjudgeWebHost.Data;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IJudgeService
    {
        Task<IQueryable<Judge>> QueryJudgesAsync(int? groupId, int? contestId, int? problemId, ApplicationDbContext dbContext);
        Task<IQueryable<Judge>> QueryJudgesAsync(string userId, int? groupId, int? contestId, int? problemId, ApplicationDbContext dbContext);
    }
    public class JudgeService : IJudgeService
    {
        public Task<IQueryable<Judge>> QueryJudgesAsync(int? groupId, int? contestId, int? problemId, ApplicationDbContext dbContext)
        {
            return Task.FromResult(dbContext.Judge.Where(i => i.GroupId == groupId && i.ContestId == contestId && i.ProblemId == problemId));
        }

        public Task<IQueryable<Judge>> QueryJudgesAsync(string userId, int? groupId, int? contestId, int? problemId, ApplicationDbContext dbContext)
        {
            return Task.FromResult(dbContext.Judge.Where(i => i.UserId == userId && i.GroupId == groupId && i.ContestId == contestId && i.ProblemId == problemId));
        }
    }
}
