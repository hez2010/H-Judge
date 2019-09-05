using System;
using System.ComponentModel;
using System.Linq;

namespace hjudge.WebHost.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum val)
        {
            var type = val.GetType();
            var memberInfo = type.GetMember(val.ToString());
            if (memberInfo.Length == 0)
            {
                return val.ToString();
            }
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Single() as DescriptionAttribute)?.Description ?? val.ToString();
        }

    }
}
