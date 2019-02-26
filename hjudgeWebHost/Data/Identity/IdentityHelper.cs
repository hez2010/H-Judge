using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hjudgeWebHost.Data.Identity
{
    public class IdentityHelper
    {
        public class OtherInfoList
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private static PropertyInfo[] properties = typeof(OtherUserInfo).GetProperties()
                                                        .Where(i => i.IsDefined(typeof(ItemNameAttribute), false))
                                                        .ToArray();

        public static List<OtherInfoList> GetOtherUserInfo(string rawInfo)
        {
            var otherInfo = JsonConvert.DeserializeObject<OtherUserInfo>(rawInfo ?? "{}");
            if (otherInfo == null) otherInfo = new OtherUserInfo();
            var otherInfoList = new List<OtherInfoList>();
            
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute is ItemNameAttribute)
                    {
                        otherInfoList.Add(new OtherInfoList
                        {
                            Key = property.Name,
                            Name = attribute.GetType().GetProperty("ItemName").GetValue(attribute)?.ToString(),
                            Value = property.GetValue(otherInfo)?.ToString()
                        });
                        break;
                    }
                }
            }
            return otherInfoList;
        }
    }
}
