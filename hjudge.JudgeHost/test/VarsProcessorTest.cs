using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudge.JudgeHost.Test
{
    [TestClass]
    public class VarsProcessorTest
    {
        class InnerStructure
        {
            public string A { get; set; } = "testabc";
            public string B { get; } = "testabc";
            public string F { get; set; } = "hhhh";
            public int G { get; set; } = 123;
            public string H = "testabc";
            public string? M = null;
        }
        class TestStructure
        {
            public string A { get; set; } = "testabc123456";
            public string B { get; } = "testabc";
            public InnerStructure C { get; } = new InnerStructure();
            public InnerStructure? D { get; set; } = new InnerStructure();
            public string F { get; set; } = "hhhh${datadir:2}";
            public int G { get; set; } = 123;
            public string H = "testabc";
            public string[] M { get; set; } = new[] { "testabc", "def${datadir:2}" };
            public List<string> N { get; set; } = new List<string> { "testabc", "def" };
        }

        [TestMethod]
        public async Task VarsProcess()
        {
            var dict = new Dictionary<string, string>
            {
                ["test"] = "abc",
                ["123456"] = "def"
            };

            var obj = new TestStructure();

            var result = (await VarsProcessor.FillinVarsAndFetchFiles(obj, dict)).ToArray();
            Assert.AreEqual(2, result.Length);

            Assert.AreEqual("abcabcdef", obj.A);
            Assert.AreEqual("abcabc", obj.M[0]);
            Assert.AreEqual("abcabc", obj.N[0]);
            Assert.AreEqual("testabc", obj.B);
            Assert.AreEqual("testabc", obj.H);
            Assert.AreEqual("abcabc", obj.C.A);
            Assert.AreEqual("testabc", obj.C.B);
            Assert.AreEqual("testabc", obj.C.H);
            Assert.AreEqual("abcabc", obj.D?.A);
            Assert.AreEqual("testabc", obj.D?.B);
            Assert.AreEqual("testabc", obj.D?.H);

            var nullobj = new TestStructure
            {
                D = null
            };
            await VarsProcessor.FillinVarsAndFetchFiles(nullobj, dict);
        }
    }
}
