using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.Group
{
    public class GroupPermission
    {
        [Key]
        public long GroupPermisionId { get; set; }

        public long GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Groups Group { get; set; }


        public Boolean Read { get; set; }
        public Boolean Write { get; set; }
        public Boolean Edit { get; set; }
        public Boolean Delete { get; set; }
        public Boolean None { get; set; }

    }
}
