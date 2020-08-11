using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.Industry
{
    public class industries
    {
        [Key]
        public int Id { get; set; }
        public string industrieType { get; set; }

    }
}
