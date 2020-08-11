using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.GroupsViewModels
{
    public class GroupPermissionViewModel
    {
        public int GroupId { get; set; }
        public bool Read { get; set; }
        public bool Write { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        public bool None { get; set; }
    }
}
