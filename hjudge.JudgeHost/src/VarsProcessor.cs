using System;
using System.Collections.Generic;
using System.Text;

namespace hjudge.JudgeHost
{
    public class VarsProcessor
    {
        public static string ProcessString(string str, IDictionary<string, string> varsTable)
        {
            foreach(var i in varsTable)
            {
                str = str.Replace(i.Key, i.Value);
            }
            return str;
        }

        public static void FillinVars(object target, IDictionary<string, string> varsTable)
        {
            var type = target.GetType();
            var properties = type.GetProperties();

            if (properties.Length != 0)
            {
                foreach(var p in properties)
                {
                    if (p.PropertyType == typeof(string))
                    {
                        if (p.CanRead && p.CanWrite && p.GetValue(target) is string str)
                        {
                            p.SetValue(target, ProcessString(str, varsTable));
                        }
                    }
                    else
                    {
                        FillinVars(p.GetValue(target), varsTable);
                    }
                }
            }
        }
    }
}
