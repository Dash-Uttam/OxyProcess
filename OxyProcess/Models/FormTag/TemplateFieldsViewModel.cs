using OxyProcess.Models.FormTag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.FormTag
{
    public class TemplateFieldsViewModel
    {
        
        public TemplateFieldsViewModel(List<TemplateFieldsChildsViewModel> templateFieldsChildsViewModels , List<FilesManager> filesManager)
        {
            this.values = templateFieldsChildsViewModels;
            this.Files = filesManager;
        }

        public int Id { get; set; }

        public string type { get; set; }
        public string label { get; set; }
        public string name { get; set; }
        public string subtype { get; set; }
        public bool selected { get; set; }
        public string className { get; set; }

        public string description { get; set; }

        public string placeholder { get; set; }
        public string style { get; set; }

        public string maxlength { get; set; }

        public string rows { get; set; }

        public string value { get; set; }

        public string min { get; set; }


        public string max { get; set; }

        public string step { get; set; }
        public bool inline { get; set; }

        public bool required { get; set; }

        /// <for_fields_management>

        public string LatestValue { get; set; }//main value by letest entry 

        public int TemplateuniqueId { get; set; }

        public string TemplateSecondName { get; set; }
        public int TemplateId { get; set; }

        public string FormName { get; set; }

        public int TagId { get; set; }

        public bool IsActive { get; set; }

        public int LastModifyBy { get; set; }

        public int TemplateType { get; set; }

        public string ChildJson { get; set; }

        public bool WritePermission { get; set; }

        public bool multiple { get; set; }

        public List<TemplateFieldsChildsViewModel> values { get; set; }

        public List<FilesManager> Files { get; set; }
        /// </summary>
        

    }



    public class TemplateFieldsChildsViewModel
    {
        public string label { get; set; }
        public string value { get; set; }

        public bool selected { get; set; }

    }


}
