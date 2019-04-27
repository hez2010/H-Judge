using hjudgeWebHost.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHostTest
{
    [TestClass]
    public class GroupTest
    {
        private readonly IGroupService service = TestService.Provider.GetService(typeof(IGroupService)) as IGroupService;

        [TestMethod]
        public async Task ModifyAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var group = new hjudgeWebHost.Data.Group
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };
            var id = await service.CreateGroupAsync(group);
            Assert.AreNotEqual(0, id);

            var studentResult = await service.QueryGroupAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == group.Name));

            var newName = Guid.NewGuid().ToString();
            group.Name = newName;
            await service.UpdateGroupAsync(group);

            studentResult = await service.QueryGroupAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == group.Name));

            await service.RemoveGroupAsync(id);

            studentResult = await service.QueryGroupAsync(stuId);
            Assert.IsFalse(studentResult.Any(i => i.Id == id));
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;
            var pubId = await service.CreateGroupAsync(new hjudgeWebHost.Data.Group
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            var priId = await service.CreateGroupAsync(new hjudgeWebHost.Data.Group
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                IsPrivate = true
            });

            var adminResult = await service.QueryGroupAsync(adminId);
            var strdentResult = await service.QueryGroupAsync(stuId);

            Assert.IsTrue(adminResult.Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));

            await service.OptInGroup(stuId, priId);
            strdentResult = await service.QueryGroupAsync(stuId);
            Assert.IsTrue(strdentResult.Any(i => i.Id == priId));

            await service.OptOutGroup(stuId, priId);
            strdentResult = await service.QueryGroupAsync(stuId);
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));
        }
    }
}
