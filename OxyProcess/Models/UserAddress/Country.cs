using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.UserAddress
{
    public class Country
    {
        [Key]
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        //public ICollection<Address> Addresses { get; set; }
        public ICollection<State> States { get; set; }
     
    }
}
