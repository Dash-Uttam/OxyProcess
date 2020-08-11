using OxyProcess.Models.GroupsViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.GroupsViewModels
{
    public class GroupChanges
    {

        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupMemberList  { get; set; }
        public GroupPermissionViewModel GroupPermission { get; set; }

    }
}
