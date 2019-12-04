using System;
using System.Threading.Tasks;
using hjudge.WebHost.Data;
using hjudge.WebHost.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace hjudge.WebHost.Test
{
    [TestClass]
    public class VoteTest
    {
        [TestMethod]
        public async Task UpvoteProblemTest()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();
            var voteService = scope.ServiceProvider.GetService<IVoteService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await problemService.CreateProblemAsync(new Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            Assert.AreNotEqual(0, pubId);

            Assert.IsTrue(await voteService.UpvoteProblemAsync(stuId, pubId));
            var result = await problemService.GetProblemAsync(pubId);

            Assert.AreEqual(1, result?.Upvote);

            Assert.IsFalse(await voteService.UpvoteProblemAsync(stuId, pubId));
            Assert.IsFalse(await voteService.DownvoteProblemAsync(stuId, pubId));
        }

        [TestMethod]
        public async Task UpvoteContestTest()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();
            var voteService = scope.ServiceProvider.GetService<IVoteService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await contestService.CreateContestAsync(new Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            Assert.AreNotEqual(0, pubId);

            Assert.IsTrue(await voteService.UpvoteContestAsync(stuId, pubId));
            var result = await contestService.GetContestAsync(pubId);

            Assert.AreEqual(1, result?.Upvote);

            Assert.IsFalse(await voteService.UpvoteContestAsync(stuId, pubId));
            Assert.IsFalse(await voteService.DownvoteContestAsync(stuId, pubId));
        }
        [TestMethod]
        public async Task DownvoteProblemTest()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();
            var voteService = scope.ServiceProvider.GetService<IVoteService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await problemService.CreateProblemAsync(new Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            Assert.AreNotEqual(0, pubId);

            Assert.IsTrue(await voteService.DownvoteProblemAsync(stuId, pubId));
            var result = await problemService.GetProblemAsync(pubId);

            Assert.AreEqual(1, result?.Downvote);

            Assert.IsFalse(await voteService.UpvoteProblemAsync(stuId, pubId));
            Assert.IsFalse(await voteService.DownvoteProblemAsync(stuId, pubId));
        }

        [TestMethod]
        public async Task DownvoteContestTest()
        {
            using var scope = TestService.Scope;
            var contestService = scope.ServiceProvider.GetService<IContestService>();
            var problemService = scope.ServiceProvider.GetService<IProblemService>();
            var voteService = scope.ServiceProvider.GetService<IVoteService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await contestService.CreateContestAsync(new Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            Assert.AreNotEqual(0, pubId);

            Assert.IsTrue(await voteService.DownvoteContestAsync(stuId, pubId));
            var result = await contestService.GetContestAsync(pubId);

            Assert.AreEqual(1, result?.Downvote);

            Assert.IsFalse(await voteService.UpvoteContestAsync(stuId, pubId));
            Assert.IsFalse(await voteService.DownvoteContestAsync(stuId, pubId));
        }
    }
}
