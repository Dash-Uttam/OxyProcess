using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.FormTag
{
    public class TagFormDataEntryViewModel
    {

       
        public int Id { get; set; }

        public int TemplateuniqueId { get; set; }
        public int OrignalTemplateId { get; set; }

        public int TagId { get; set; }

        public string TagNumber { get; set; }
        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public int LastModifyBy { get; set; }

        public bool WritePermission { get; set; }
        public string FormJsonData {get; set;}
        public string Title { get; set; }

        public string FormName { get; set; }
        public string CreatedDate { get; set; }

        /// </summary>


    }


}
