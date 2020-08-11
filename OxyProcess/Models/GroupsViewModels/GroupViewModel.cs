using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.GroupsViewModels
{
    public class GroupViewModel
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
