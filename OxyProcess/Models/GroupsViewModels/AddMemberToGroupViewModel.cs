using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.GroupsViewModels
{
    public class AddMemberToGroupViewModel
    {
        [Required]
        public int GroupId { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
