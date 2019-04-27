using hjudgeWebHost.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHostTest
{
    [TestClass]
    public class ContestTest
    {
        private readonly IContestService service = TestService.Provider.GetService(typeof(IContestService)) as IContestService;

        [TestMethod]
        public async Task ModifyAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var contest = new hjudgeWebHost.Data.Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };
            var id = await service.CreateContestAsync(contest);
            Assert.AreNotEqual(0, id);

            var studentResult = await service.QueryContestAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == contest.Name));

            var newName = Guid.NewGuid().ToString();
            contest.Name = newName;
            await service.UpdateContestAsync(contest);

            studentResult = await service.QueryContestAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == contest.Name));

            await service.RemoveContestAsync(id);

            studentResult = await service.QueryContestAsync(stuId);
            Assert.IsFalse(studentResult.Any(i => i.Id == id));
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await service.CreateContestAsync(new hjudgeWebHost.Data.Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            var priId = await service.CreateContestAsync(new hjudgeWebHost.Data.Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                Hidden = true
            });

            var adminResult = await service.QueryContestAsync(adminId);
            var strdentResult = await service.QueryContestAsync(stuId);

            Assert.IsTrue(adminResult.Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));
        }
    }
}
