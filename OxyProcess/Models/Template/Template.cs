using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using OxyProcess.Models.Group;
namespace OxyProcess.Models.Template
{
    public class Template
    {
        [Key]
        public int TemplateId { get; set; }
        [Required]        
        public string TemplateName { get; set; }
        [Required]
        public string TemplateCode { get; set; }
        [Required]
        public string Groups { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public int CompanyId { get; set; }

        public bool IsDeleted { get; set; }
        [ForeignKey("TypeId")]
        public int TemplateTypeId { get; set; }

        public virtual TemplateType TemplateType { get; set; }
    }
}
