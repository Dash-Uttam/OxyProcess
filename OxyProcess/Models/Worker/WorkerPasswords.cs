using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.Worker
{
    public class WorkerPasswords
    {
        [Key]
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public string WorkerPassword { get; set; }

    }
}
