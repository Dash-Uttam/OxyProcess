using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.FormTag
{
    
    public class TemplateFields
    {
        [Key]
        public int Id { get; set; }

        public string type { get; set; }

        public string label { get; set; }

        public string name { get; set; }

        public string subtype { get; set; }

        public string LatestValue { get; set; }//main value by letest entry 

        public int TemplateuniqueId { get; set; }

        public int TemplateId { get; set; }

        public int TagId { get; set; }

        public bool IsActive { get; set; }

        public int LastModifyBy { get; set; }

        //public bool selected { get; set; }
        //public string className { get; set; }

        //public string description { get; set; }

        //public string placeholder { get; set; }
        //public string style { get; set; }

        //public string maxlength { get; set; }

        //public string rows { get; set; }

        public string value { get; set; }

        //public string min { get; set; }


        //public string max { get; set; }

        //public string step { get; set; }
        //public bool inline { get; set; }

        //public bool required { get; set; }

        //public bool multiple { get; set; }
        /// <for_fields_management>




        /// </summary>


    }


}
