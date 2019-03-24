using hjudgeWebHost.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
{
    public interface IGroupService
    {
        Task<IQueryable<Group>> QueryGroupAsync();
        Task<IQueryable<Group>> QueryGroupAsync(string userId);
        Task<Group> GetGroupAsync(int groupId);
        Task<int> CreateGroupAsync(Group group);
        Task UpdateGroupAsync(Group group);
        Task RemoveGroupAsync(int groupId);
    }
    public class GroupService : IGroupService
    {
        public Task<int> CreateGroupAsync(Group group)
        {
            throw new NotImplementedException();
        }

        public Task<Group> GetGroupAsync(int groupId)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<Group>> QueryGroupAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<Group>> QueryGroupAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveGroupAsync(int groupId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateGroupAsync(Group group)
        {
            throw new NotImplementedException();
        }
    }
}
