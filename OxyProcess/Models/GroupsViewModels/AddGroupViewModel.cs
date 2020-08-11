using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.GroupsViewModels
{
    public class AddGroupViewModel
    {
        [Required(ErrorMessage = "Please enter GroupName")]
        public string GroupName { get; set; }
    }
}
