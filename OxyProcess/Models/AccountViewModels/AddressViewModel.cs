using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.AccountViewModels
{
    public class AddressViewModel
    {
        public int AddressId { get; set; }
        [Required]
        public string AddressLine1 { get; set; }
      
        public string AddressLine2 { get; set; }
        [Required]
        public string City { get; set; }
      

        public int StateId { get; set; }


        public int CountryId { get; set; }

        [Required]
        public string ZipCode { get; set; }
    }
}
