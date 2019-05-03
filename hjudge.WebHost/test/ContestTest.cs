using hjudge.WebHost.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;

namespace hjudge.WebHost.Test
{
    [TestClass]
    public class ContestTest
    {
        private readonly IContestService contestService = TestService.Provider.GetService(typeof(IContestService)) as IContestService;
        private readonly IProblemService problemService = TestService.Provider.GetService(typeof(IProblemService)) as IProblemService;

        [TestMethod]
        public async Task ConfigAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var contest = new Data.Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };

            var cid = await contestService.CreateContestAsync(contest);
            Assert.AreNotEqual(0, cid);

            var problem = new Data.Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };

            var pid = await problemService.CreateProblemAsync(problem);
            Assert.AreNotEqual(0, pid);

            await contestService.UpdateContestProblemAsync(cid, new[] { pid, pid });
            var result = await problemService.QueryProblemAsync(stuId, cid);
            Assert.IsTrue(result.Cacheable().Count(i => i.Id == pid) == 1);

            await contestService.UpdateContestProblemAsync(cid, new int[0]);
            result = await problemService.QueryProblemAsync(stuId, cid);
            Assert.IsFalse(result.Cacheable().Any());
        }

        [TestMethod]
        public async Task ModifyAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var contest = new Data.Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };
            var id = await contestService.CreateContestAsync(contest);
            Assert.AreNotEqual(0, id);

            var studentResult = await contestService.QueryContestAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == contest.Name));

            var newName = Guid.NewGuid().ToString();
            contest.Name = newName;
            await contestService.UpdateContestAsync(contest);

            studentResult = await contestService.QueryContestAsync(stuId);
            Assert.IsTrue(studentResult.Cacheable().Any(i => i.Id == id && i.Name == contest.Name));

            await contestService.RemoveContestAsync(id);

            studentResult = await contestService.QueryContestAsync(stuId);
            Assert.IsFalse(studentResult.Cacheable().Any(i => i.Id == id));
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await contestService.CreateContestAsync(new Data.Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            var priId = await contestService.CreateContestAsync(new Data.Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                Hidden = true
            });

            var adminResult = await contestService.QueryContestAsync(adminId);
            var strdentResult = await contestService.QueryContestAsync(stuId);

            Assert.IsTrue(adminResult.Cacheable().Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Cacheable().Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Cacheable().Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Cacheable().Any(i => i.Id == priId));
        }
    }
}
