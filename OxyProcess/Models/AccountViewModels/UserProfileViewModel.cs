using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.AccountViewModels
{
    public class UserProfileViewModel
    {
        public string Email { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Password { get; set; }

        public string CompanyName { get; set; }
   
        public string Phone { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public int CompanyId { get; set; }

        

        public int StateId { get; set; }

        public int CountryId { get; set; }

        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }
}
