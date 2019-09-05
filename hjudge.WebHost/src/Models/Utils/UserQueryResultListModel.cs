using System.Collections.Generic;

namespace hjudge.WebHost.Models.Utils
{
    public class UserQueryResultListModel
    {
        public List<UserQueryResultModel> Users { get; set; } = new List<UserQueryResultModel>();
        public int TotalCount { get; set; }
    }
}
