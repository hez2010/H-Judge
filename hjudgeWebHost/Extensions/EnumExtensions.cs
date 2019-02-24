#nullable enable
using System;
using System.ComponentModel;
using System.Linq;

namespace hjudgeWebHost.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum val)
        {
            var type = val.GetType();
            var memberInfo = type.GetMember(val.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes == null)
            {
                return val.ToString();
            }
            return (attributes.Single() as DescriptionAttribute)?.Description ?? val.ToString();
        }

    }
}
