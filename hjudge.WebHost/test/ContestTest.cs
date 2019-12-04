using System;
using System.Linq;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace hjudge.WebHost.Test
{
    [TestClass]
    public class ContestTest
    {
        [TestMethod]
        public async Task JoinQuitAsync()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var contest = new Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                SpecifyCompetitors = true
            };

            var cid = await contestService.CreateContestAsync(contest);
            Assert.AreNotEqual(0, cid);

            await contestService.JoinContestAsync(cid, new[] { stuId });
            Assert.IsTrue(await contestService.HasJoinedContestAsync(cid, stuId));

            await contestService.QuitContestAsync(cid, new[] { stuId });
            Assert.IsFalse(await contestService.HasJoinedContestAsync(cid, stuId));
        }

        [TestMethod]
        public async Task ConfigAsync()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var contest = new Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };

            var cid = await contestService.CreateContestAsync(contest);
            Assert.AreNotEqual(0, cid);

            var problem = new Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };

            var pid = await problemService.CreateProblemAsync(problem);
            Assert.AreNotEqual(0, pid);

            await contestService.UpdateContestProblemAsync(cid, new[] { pid, pid });
            var result = await problemService.QueryProblemAsync(stuId, cid);
            Assert.IsTrue(result.Count(i => i.Id == pid) == 1);

            await contestService.UpdateContestProblemAsync(cid, Array.Empty<int>());
            result = await problemService.QueryProblemAsync(stuId, cid);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task ModifyAsync()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var contest = new Contest
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
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == contest.Name));

            await contestService.RemoveContestAsync(id);

            studentResult = await contestService.QueryContestAsync(stuId);
            Assert.IsFalse(studentResult.Any(i => i.Id == id));
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await contestService.CreateContestAsync(new Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            var priId = await contestService.CreateContestAsync(new Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                Hidden = true
            });

            var adminResult = await contestService.QueryContestAsync(adminId);
            var strdentResult = await contestService.QueryContestAsync(stuId);

            Assert.IsTrue(adminResult.Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));
        }
    }
}
