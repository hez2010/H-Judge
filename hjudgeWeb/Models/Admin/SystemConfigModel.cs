using hjudgeWeb.Configurations;
using System.Collections.Generic;

namespace hjudgeWeb.Models.Admin
{
    public class SystemConfigModel : ResultModel
    {
        public SystemConfigModel()
        {
            Languages = new List<LanguageConfiguration>();
        }

        public string Environments { get; set; }
        public string System { get; set; }
        public List<LanguageConfiguration> Languages { get; set; }
    }
}
