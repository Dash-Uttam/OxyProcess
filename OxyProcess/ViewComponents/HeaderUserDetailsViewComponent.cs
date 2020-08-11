using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OxyProcess.Data;
using OxyProcess.Models;
using OxyProcess.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace OxyProcess.ViewComponents
{
    public class HeaderUserDetailsViewComponent : ViewComponent
    {
        public readonly ApplicationDbContext _context;
  
        public readonly SignInManager<ApplicationUser> SignInManager;
      
        public HeaderUserDetailsViewComponent(ApplicationDbContext app, SignInManager<ApplicationUser> sig)
        {

            _context = app;
            SignInManager = sig;
          

        }

        public  IViewComponentResult Invoke(int numberOfItems)
        {
            try
            {
                int userid = Convert.ToInt32(UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(u => u.Id == userid);


               ApplicationUser userProfile = new ApplicationUser();

                
                if (userid > 0)
                {
                   var  userProfiles =   (from u in _context.Users
                                   join c in _context.Companies on u.CompanyId equals c.CompanyId
                                   where u.Id == userid
                                   select  new  ApplicationUser
                                   {
                                       FirstName = u.FirstName,
                                       LastName = u.LastName,
                                       CompanyName = c.CompanyName,

                                   }).FirstOrDefault();
                }
                //return  View("HeaderUserDetails", userProfile);
                return View("HeaderUserDetails", userProfile);
            }
            catch (Exception ex)
            {

                throw;
            }

          
          

        }
    }
}
