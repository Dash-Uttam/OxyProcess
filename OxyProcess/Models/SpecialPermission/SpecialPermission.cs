using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.SpecialPermission
{
    public class SpecialPermission
    {
        public int Id { get; set; }

        public int Requested_User { get; set; }

        public string TagId { get; set; }
        public int Unique_Template_Id { get; set; }

        public int Company_Id { get; set; }

        public bool AccessGranted { get; set; }

        public bool Rejected { get; set; }
    }
}
