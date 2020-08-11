using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.GroupsViewModels
{
    public class GroupMemberPermissionModel
    {
        public GroupPermissionViewModel groupPermissionViewModel { get; set; }
        public AddMemberToGroupViewModel addMemberToGroupViewModel { get; set; }
        public string GroupName { get; set; }
    }
}
