using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.FormTag
{
    public class TemplateDataFields
    {
        [Key]
        public int Id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
}
