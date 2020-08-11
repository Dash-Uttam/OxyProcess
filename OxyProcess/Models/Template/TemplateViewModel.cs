using OxyProcess.Models.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.ViewModels.Template
{
    public class TemplateViewModel
    {
        public int TemplateId { get; set; }
        [Required(ErrorMessage = "Template Name is required")]
        public string TemplateName { get; set; }
        [Required(ErrorMessage = "Select field for template")]
        public string TemplateCode { get; set; }
        [Required(ErrorMessage = "Select group perrmission")]
        public string Groups { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public int TemplateTypeId { get; set; }

        public virtual TemplateType TemplateType { get; set; }
    }
}
