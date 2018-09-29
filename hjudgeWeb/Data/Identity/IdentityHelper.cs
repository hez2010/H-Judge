using Newtonsoft.Json;
using System.Collections.Generic;

namespace hjudgeWeb.Data.Identity
{
    public class IdentityHelper
    {
        public class OtherInfoList
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public static List<OtherInfoList> GetOtherUserInfo(string rawInfo)
        {
            var otherInfo = JsonConvert.DeserializeObject<OtherUserInfo>(rawInfo ?? string.Empty);
            if (otherInfo == null) otherInfo = new OtherUserInfo();
            var properties = typeof(OtherUserInfo).GetProperties();
            var otherInfoList = new List<OtherInfoList>();
            foreach (var property in properties)
            {
                if (!property.IsDefined(typeof(ItemNameAttribute), false))
                {
                    continue;
                }

                var attributes = property.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute.GetType().Name == "ItemNameAttribute")
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
