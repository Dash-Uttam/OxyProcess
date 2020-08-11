using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.SpecialPermission
{
    public class RequestPersonViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public bool AccessPermission { get; set; }
        public string TemplateName { get; set; }
        public string Tag { get; set; }
        public bool Reject { get; set; }

    }
}
