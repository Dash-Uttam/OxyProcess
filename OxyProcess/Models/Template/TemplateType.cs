using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.Template
{
    public class TemplateType
    {
        [Key]
        public int TypeId { get; set; }
        public string TemplateTypeName { get; set; }


    }
}
