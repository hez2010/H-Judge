using hjudgeWebHost.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace hjudgeWebHost.Services
{
    public interface IContestService
    {
        (ApplicationDbContext DbContext, IQueryable<Contest> Contests) QueryContest(ApplicationDbContext? dbContext = null);
        (ApplicationDbContext DbContext, IQueryable<Contest> Contests) QueryContest(int groupId, ApplicationDbContext? dbContext = null);
    }
    public class ContestService : IContestService
    {
        private readonly DbContextOptions<ApplicationDbContext> DbOptions;
        public ContestService(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            DbOptions = dbOptions;
        }

        public (ApplicationDbContext DbContext, IQueryable<Contest> Contests) QueryContest(ApplicationDbContext? dbContext = null)
        {
            var db = dbContext ?? new ApplicationDbContext(DbOptions);
            return (db, db.Contest);
        }

        public (ApplicationDbContext DbContext, IQueryable<Contest> Contests) QueryContest(int groupId, ApplicationDbContext? dbContext = null)
        {
            var db = dbContext ?? new ApplicationDbContext(DbOptions);
            return (db, db.Contest
                    .Where(i => db.GroupContestConfig
                        .Any(j => j.GroupId == groupId &&
                            j.ContestId == i.Id)));
        }
    }
}
