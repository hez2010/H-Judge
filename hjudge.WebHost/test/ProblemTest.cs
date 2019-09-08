using hjudge.WebHost.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using Microsoft.Extensions.DependencyInjection;

namespace hjudge.WebHost.Test
{
    //[TestClass]
    public class ProblemTest
    {
        private readonly IProblemService service = TestService.Provider.GetService<IProblemService>();

        [TestMethod]
        public async Task ModifyAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var problem = new Data.Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };
            var id = await service.CreateProblemAsync(problem);
            Assert.AreNotEqual(0, id);

            var studentResult = await service.QueryProblemAsync(stuId);
            Assert.IsTrue(studentResult.Cacheable().Any(i => i.Id == id && i.Name == problem.Name));

            var newName = Guid.NewGuid().ToString();
            problem.Name = newName;
            await service.UpdateProblemAsync(problem);

            studentResult = await service.QueryProblemAsync(stuId);
            Assert.IsTrue(studentResult.Cacheable().Any(i => i.Id == id && i.Name == problem.Name));

            await service.RemoveProblemAsync(id);

            studentResult = await service.QueryProblemAsync(stuId);
            Assert.IsFalse(studentResult.Cacheable().Any(i => i.Id == id));
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await service.CreateProblemAsync(new Data.Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            var priId = await service.CreateProblemAsync(new Data.Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                Hidden = true
            });

            var adminResult = await service.QueryProblemAsync(adminId);
            var strdentResult = await service.QueryProblemAsync(stuId);

            Assert.IsTrue(adminResult.Cacheable().Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Cacheable().Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Cacheable().Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Cacheable().Any(i => i.Id == priId));
        }
    }
}
