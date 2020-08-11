using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.FormTag
{
    public class TaginsideTemplates
    {
        [Key]
        public int Id { get; set; }
        public int TemplateuniqueId { get; set; } //Template identification Id for Tag only 
        public string TemplateCloneCode { get; set; } // Template Clone from Template table
        public int OrignalTemplateId { get; set; } //Orignal Template ID  
        public int TagId { get; set; } //Owner Tag ID 
        public DateTime CreatedDate { get; set; } //created date and time    
        public bool IsActive { get; set; } //template is active or not 
        public bool IsFilled { get; set; } //tag form are fill or not 
        public int GroupId { get; set; }  //Tag Group of templates
        public string GroupName { get; set; } //Group Name
        public int UpdateById { get; set; }  //update by id 
        
    }
}
