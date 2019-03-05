using hjudgeWebHost.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace hjudgeWebHost.Services
{
    public interface IProblemService
    {
        (ApplicationDbContext DbContext, IQueryable<Problem> Problems) QueryProblem(ApplicationDbContext? dbContext = null);
        (ApplicationDbContext DbContext, IQueryable<Problem> Problems) QueryProblem(int contestId, ApplicationDbContext? dbContext = null);
        (ApplicationDbContext DbContext, IQueryable<Problem> Problems) QueryProblem(int contestId, int groupId, ApplicationDbContext? dbContext = null);
    }
    public class ProblemService : IProblemService
    {
        private readonly DbContextOptions<ApplicationDbContext> DbOptions;
        public ProblemService(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            DbOptions = dbOptions;
        }

        public (ApplicationDbContext DbContext, IQueryable<Problem> Problems) QueryProblem(ApplicationDbContext? dbContext = null)
        {
            var db = dbContext ?? new ApplicationDbContext(DbOptions);
            return (db, db.Problem);
        }

        public (ApplicationDbContext DbContext, IQueryable<Problem> Problems) QueryProblem(int contestId, ApplicationDbContext? dbContext = null)
        {
            var db = dbContext ?? new ApplicationDbContext(DbOptions);
            return (db, db.Problem
                    .Where(i => db.ContestProblemConfig
                        .Any(j => j.ContestId == contestId &&
                            i.Id == j.ProblemId)));
        }

        public (ApplicationDbContext DbContext, IQueryable<Problem> Problems) QueryProblem(int contestId, int groupId, ApplicationDbContext? dbContext = null)
        {
            var db = dbContext ?? new ApplicationDbContext(DbOptions);
            return (db, db.Problem
                    .Where(i => db.ContestProblemConfig
                        .Any(j => j.ContestId == contestId &&
                            i.Id == j.ProblemId &&
                            db.GroupContestConfig
                                .Any(k => k.GroupId == groupId &&
                                    k.ContestId == j.ContestId))));
        }
    }
}
