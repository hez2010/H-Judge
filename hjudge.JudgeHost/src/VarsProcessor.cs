using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hjudge.JudgeHost
{
    public class VarsProcessor
    {
        public static async Task<IEnumerable<string>> FillinWorkingDirAndGetRequiredFiles(object? target, string workingDir)
        {
            if (target == null) return new string[0];
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
                        var newStr = str.Replace("${workingdir}", workingDir);
                        if (newStr.StartsWith("R:"))
                        {
                            newStr = newStr[2..];
                            fileList.Add(newStr);
                        }
                        if (p.CanRead && p.CanWrite) p.SetValue(target, newStr);
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
                                    var newStr = strItem.Replace("${workingdir}", workingDir);
                                    if (newStr.StartsWith("R:"))
                                    {
                                        newStr = newStr[2..];
                                        fileList.Add(newStr);
                                    }
                                    arr.SetValue(newStr, cnt);
                                    if (newStr.Contains("${datadir")) fileList.Add(newStr);
                                }
                            }
                            else
                            {
                                fileList.AddRange(await FillinWorkingDirAndGetRequiredFiles(obj, workingDir));
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
                                    var newStr = strItem.Replace("${workingdir}", workingDir);
                                    if (newStr.StartsWith("R:"))
                                    {
                                        newStr = newStr[2..];
                                        fileList.Add(newStr);
                                    }
                                    list[cnt] = newStr;
                                    if (newStr.Contains("${datadir")) fileList.Add(newStr);
                                }
                            }
                            else
                            {
                                fileList.AddRange(await FillinWorkingDirAndGetRequiredFiles(obj, workingDir));
                            }
                        }
                    }
                    else fileList.AddRange(await FillinWorkingDirAndGetRequiredFiles(p.GetValue(target), workingDir));
                }
            }
            return fileList;
        }
    }
}
