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
    public class ProblemTest
    {
        [TestMethod]
        public async Task ModifyAsync()
        {
            using var scope = TestService.Scope;
            var problemService = scope.ServiceProvider.GetService<IProblemService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var problem = new Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            };
            var id = await problemService.CreateProblemAsync(problem);
            Assert.AreNotEqual(0, id);

            var studentResult = await problemService.QueryProblemAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == problem.Name));

            var newName = Guid.NewGuid().ToString();
            problem.Name = newName;
            await problemService.UpdateProblemAsync(problem);

            studentResult = await problemService.QueryProblemAsync(stuId);
            Assert.IsTrue(studentResult.Any(i => i.Id == id && i.Name == problem.Name));

            await problemService.RemoveProblemAsync(id);

            studentResult = await problemService.QueryProblemAsync(stuId);
            Assert.IsFalse(studentResult.Any(i => i.Id == id));
        }

        [TestMethod]
        public async Task QueryAsync()
        {
            using var scope = TestService.Scope;
            var problemService = scope.ServiceProvider.GetService<IProblemService>();

            var adminId = (await UserUtils.GetAdmin()).Id;
            var stuId = (await UserUtils.GetStudent()).Id;

            var pubId = await problemService.CreateProblemAsync(new Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId
            });

            var priId = await problemService.CreateProblemAsync(new Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = adminId,
                Hidden = true
            });

            var adminResult = await problemService.QueryProblemAsync(adminId);
            var strdentResult = await problemService.QueryProblemAsync(stuId);

            Assert.IsTrue(adminResult.Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));
        }
    }
}
