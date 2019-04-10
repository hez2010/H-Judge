using hjudgeWebHost.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHostTest
{
    [TestClass]
    public class ProblemTest
    {
        private readonly IProblemService service = TestService.Provider.GetService(typeof(IProblemService)) as IProblemService;

        [TestMethod]
        public async Task CreateAsync()
        {
            var id = await service.CreateProblemAsync(new hjudgeWebHost.Data.Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = UserUtils.GetAdmin().Id
            });

            Assert.AreNotEqual(0, id);
        }
        
        public async Task QueryAsync()
        {
            var pubId = await service.CreateProblemAsync(new hjudgeWebHost.Data.Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = UserUtils.GetAdmin().Id
            });

            var priId = await service.CreateProblemAsync(new hjudgeWebHost.Data.Problem
            {
                Name = Guid.NewGuid().ToString(),
                UserId = UserUtils.GetAdmin().Id,
                Hidden = true
            });

            var adminResult = await service.QueryProblemAsync(UserUtils.GetAdmin().Id);
            var strdentResult = await service.QueryProblemAsync(UserUtils.GetStudent().Id);

            Assert.IsTrue(adminResult.Any(i => i.Id == priId));
            Assert.IsTrue(adminResult.Any(i => i.Id == pubId));
            Assert.IsTrue(strdentResult.Any(i => i.Id == pubId));
            Assert.IsFalse(strdentResult.Any(i => i.Id == priId));
        }
    }
}
