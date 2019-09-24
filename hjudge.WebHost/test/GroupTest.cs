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
    public class GroupTest
    {
        private readonly IGroupService groupService = TestService.Provider.GetService<IGroupService>();
        private readonly IContestService contestService = TestService.Provider.GetService<IContestService>();

        [TestMethod]
        public async Task ConfigAsync()
        {

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var group = new Group
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };

            var gid = await groupService.CreateGroupAsync(group);
            Assert.AreNotEqual(0, gid);

            var contest = new Contest
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };

            var cid = await contestService.CreateContestAsync(contest);
            Assert.AreNotEqual(0, cid);

            await groupService.UpdateGroupContestAsync(gid, new[] { cid, cid });
            var result = await contestService.QueryContestAsync(stuId, gid);
            Assert.IsTrue(result.Count(i => i.Id == cid) == 1);

            await groupService.UpdateGroupContestAsync(gid, new int[0]);
            result = await contestService.QueryContestAsync(stuId, gid);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task ModifyAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var group = new Group
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };
            var id = await groupService.CreateGroupAsync(group);
            Assert.AreNotEqual(0, id);

            var studentResult = await groupService.QueryGroupAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == group.Name));

            var newName = Guid.NewGuid().ToString();
            group.Name = newName;
            await groupService.UpdateGroupAsync(group);

            studentResult = await groupService.QueryGroupAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == group.Name));

            await groupService.RemoveGroupAsync(id);

            studentResult = await groupService.QueryGroupAsync(stuId);
            Assert.IsFalse(studentResult.Any(i => i.Id == id));
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;
            var pubId = await groupService.CreateGroupAsync(new Group
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            var priId = await groupService.CreateGroupAsync(new Group
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                IsPrivate = true
            });

            var adminResult = await groupService.QueryGroupAsync(adminId);
            var strdentResult = await groupService.QueryGroupAsync(stuId);

            Assert.IsTrue(adminResult.Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));

            await groupService.OptInGroupAsync(stuId, priId);
            strdentResult = await groupService.QueryGroupAsync(stuId);
            Assert.IsTrue(strdentResult.Any(i => i.Id == priId));

            await groupService.OptOutGroupAsync(stuId, priId);
            strdentResult = await groupService.QueryGroupAsync(stuId);
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));
        }
    }
}
