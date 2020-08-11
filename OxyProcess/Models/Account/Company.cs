using OxyProcess.Models.UserAddress;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.Account
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public DateTime CreatedDate { get; set; }     

        public Int64 NoOfEmployees { get; set; }

        public int IndustryId { get; set; }



    }
}
