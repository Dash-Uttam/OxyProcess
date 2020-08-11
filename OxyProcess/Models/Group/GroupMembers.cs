using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.Group
{
    public class GroupMembers
    {
        [Key]
        public long GroupMemberId { get; set; }

        public long GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Groups Group { get; set; }

        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
