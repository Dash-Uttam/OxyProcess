using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.ViewModels.FormTag
{
    public class TagViewModel
    {

        public int Id { get; set; }
        public string TagNumber { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
