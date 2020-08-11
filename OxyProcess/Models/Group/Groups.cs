using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.Group
{
    public class Groups
    {
        [Key]
        public long GroupId { get; set; }
        [Required]
        public string GroupName { get; set; }

        public string CreatedById { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
