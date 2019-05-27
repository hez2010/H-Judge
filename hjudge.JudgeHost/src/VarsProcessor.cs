using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hjudge.JudgeHost
{
    public class VarsProcessor
    {
        public static string ProcessString(string str, IDictionary<string, string> varsTable)
        {
            foreach(var i in varsTable)
            {
                str = Regex.Replace(str, i.Key, i.Value);
            }
            return str;
        }
        
        public static async Task<IEnumerable<string>> FillinVarsAndFetchFiles(object target, IDictionary<string, string> varsTable)
        {
            var type = target.GetType();
            var properties = type.GetProperties();
            var fileList = new List<string>();

            if (properties.Length != 0)
            {
                foreach(var p in properties)
                {
                    if (p.PropertyType == typeof(string))
                    {
                        if (p.GetValue(target) is string str)
                        {
                            var newStr = ProcessString(str, varsTable);
                            if (p.CanRead && p.CanWrite) p.SetValue(target, ProcessString(str, varsTable));
                            if (newStr.Contains("${datadir"))
                            {
                                fileList.Add(newStr);
                            }
                        }
                    }
                    else
                    {
                        fileList.AddRange(await FillinVarsAndFetchFiles(p.GetValue(target), varsTable));
                    }
                }
            }
            return fileList;
        }
    }
}
