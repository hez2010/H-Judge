using hjudge.FileHost.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace hjudge.FileHost.Test
{
    //[TestClass]
    public class FileInterfaceTest
    {
        private readonly SeaweedFsService service = TestService.Provider.GetService<SeaweedFsService>();

        [TestMethod]
        public async Task FileTest()
        {
            await using (var m = new MemoryStream())
            {
                await m.WriteAsync(Encoding.UTF8.GetBytes("test"));
                await m.FlushAsync();
                m.Seek(0, SeekOrigin.Begin);
                await service.UploadAsync("{datadir:2}/test.jpg", m);

                var r = await service.DownloadAsync("{datadir:2}/test.jpg");
                Assert.IsNotNull(r);

                var buffer = new byte[(int)r.Length];
                await r.ReadAsync(buffer, 0, (int)r.Length);
                await r.DisposeAsync();
                Assert.AreEqual("test", Encoding.UTF8.GetString(buffer));
            }

            await using (var m = new MemoryStream())
            {
                await m.WriteAsync(Encoding.UTF8.GetBytes("test2"));
                await m.FlushAsync();
                m.Seek(0, SeekOrigin.Begin);
                await service.UploadAsync("{datadir:2}/test.jpg", m);

                var r = await service.DownloadAsync("{datadir:2}/test.jpg");
                Assert.IsNotNull(r);

                var buffer = new byte[(int)r.Length];
                await r.ReadAsync(buffer, 0, (int)r.Length);
                await r.DisposeAsync();
                Assert.AreEqual("test2", Encoding.UTF8.GetString(buffer));
            }

            var d = await service.DeleteAsync("{datadir:2}/test.jpg");
            Assert.IsTrue(d);

            Assert.IsNull(await service.DownloadAsync("{datadir:2}/test.jpg"));
        }
    }
}
