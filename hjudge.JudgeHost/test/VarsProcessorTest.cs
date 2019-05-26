using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
        }
        class TestStructure
        {
            public string A { get; set; } = "testabc123456";
            public string B { get; } = "testabc";
            public InnerStructure C { get; } = new InnerStructure();
            public InnerStructure D { get; set; } = new InnerStructure();
            public string F { get; set; } = "hhhh";
            public int G { get; set; } = 123;
            public string H = "testabc";
        }

        [TestMethod]
        public void VarsProcess()
        {
            var dict = new Dictionary<string, string>
            {
                ["test"] = "abc",
                ["123456"] = "def"
            };

            var obj = new TestStructure();

            VarsProcessor.FillinVars(obj, dict);

            Assert.AreEqual("abcabcdef", obj.A);
            Assert.AreEqual("testabc", obj.B);
            Assert.AreEqual("testabc", obj.H);
            Assert.AreEqual("abcabc", obj.C.A);
            Assert.AreEqual("testabc", obj.C.B);
            Assert.AreEqual("testabc", obj.C.H);
            Assert.AreEqual("abcabc", obj.D.A);
            Assert.AreEqual("testabc", obj.D.B);
            Assert.AreEqual("testabc", obj.D.H);
        }
    }
}
