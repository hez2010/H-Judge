using System;
using System.Collections;
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
            foreach (var i in varsTable)
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
                foreach (var p in properties)
                {
                    var value = p.GetValue(target);
                    if (value == null) continue;
                    if (value is string str)
                    {
                        var newStr = ProcessString(str, varsTable);
                        if (p.CanRead && p.CanWrite) p.SetValue(target, newStr);
                        if (newStr.Contains("${datadir")) fileList.Add(newStr);
                    }
                    else if (value is Array arr)
                    {
                        for (var cnt = 0; cnt < arr.Length; cnt++)
                        {
                            var obj = arr.GetValue(cnt);
                            if (obj is string strItem)
                            {
                                if (!arr.IsReadOnly)
                                {
                                    var newStr = ProcessString(strItem, varsTable);
                                    arr.SetValue(newStr, cnt);
                                    if (newStr.Contains("${datadir")) fileList.Add(newStr);
                                }
                            }
                            else
                            {
                                fileList.AddRange(await FillinVarsAndFetchFiles(obj, varsTable));
                            }
                        }
                    }
                    else if (value is IList list)
                    {
                        for (var cnt = 0; cnt < list.Count; cnt++)
                        {
                            var obj = list[cnt];
                            if (obj is string strItem)
                            {
                                if (!list.IsReadOnly)
                                {
                                    var newStr = ProcessString(strItem, varsTable);
                                    list[cnt] = newStr;
                                    if (newStr.Contains("${datadir")) fileList.Add(newStr);
                                }
                            }
                            else
                            {
                                fileList.AddRange(await FillinVarsAndFetchFiles(obj, varsTable));
                            }
                        }
                    }
                    else fileList.AddRange(await FillinVarsAndFetchFiles(p.GetValue(target), varsTable));
                }
            }
            return fileList;
        }
    }
}
