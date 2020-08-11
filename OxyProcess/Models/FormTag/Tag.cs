using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.FormTag
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        public string TagNumber { get; set; }
        public string CreatedById { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedDate { get; set; }

        public string TagCode { get; set; }
    }
}
