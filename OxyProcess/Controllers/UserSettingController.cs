namespace OxyProcess.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using OxyProcess.Constant;
    using OxyProcess.Data;
    using OxyProcess.Models;
    using OxyProcess.Models.AccountViewModels;
    using OxyProcess.Models.UserAddress;


    //[Authorize]
    [Route("[controller]/[action]")]
    public class UserSettingController : Controller
    {

        #region Property
        public readonly ApplicationDbContext _context;
        #endregion

        #region Constructor
        public UserSettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion

        #region Action
        /// <summary>
        /// Get user profile detail for edit user information
        /// </summary>
        /// <returns>UserProfileViewModel</returns>
        [HttpGet]
        public IActionResult Index()
        {
            UserProfileViewModel userProfile = new UserProfileViewModel();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    if (userid > 0)
                    {
                        userProfile = (from u in _context.Users
                                       join c in _context.Companies on u.CompanyId equals c.CompanyId
                                       join a in _context.Addresses on u.AddressId equals a.AddressId
                                       where u.Id == userid
                                       select new UserProfileViewModel
                                       {
                                           FirstName = u.FirstName,
                                           Email = u.Email,
                                           
                                           CompanyName = c.CompanyName,
                                           Phone = u.PhoneNumber,
                                           LastName = u.LastName,
                                           AddressLine1 = a.AddressLine1,
                                           AddressLine2 = a.AddressLine2,
                                           CountryId = a.CountryId,
                                           StateId = a.StateId,
                                           City = a.City,
                                           CountryName = a.Countries.CountryName,
                                           StateName = a.State.StateName,
                                           //CityName = a.City.CityName,
                                           ZipCode = a.ZipCode,
                                       }).FirstOrDefault();

                        ViewData.Clear();

                        //Get selected Country
                        List<Country> countryList = new List<Country>();
                        // ------- Getting Data from Database Using EntityFrameworkCore -------
                        countryList = (from country in _context.Countries
                                       select country).ToList();

                        // ------- Inserting Select Item in List -------
                        //countryList.Insert(userProfile.CountryId, new Country { CountryId = 0, CountryName = "Please select Country" });

                        // ------- Assigning countrylist to ViewBag.ListofCategory -------
                        ViewBag.ListofCountry = countryList;

                        //Get selected state
                        List<State> stateList = new List<State>();
                        // ------- Getting Data from Database Using EntityFrameworkCore -------
                        stateList = (from state in _context.States
                                     where state.CountryId == userProfile.CountryId
                                     select state).ToList();

                        // ------- Inserting Select Item in List -------
                        //stateList.Insert(userProfile.StateId, new State { StateId = 0, StateName = "Please select state" });

                        // ------- Assigning colist to ViewBag.ListofCategory -------
                        ViewBag.ListofState = stateList;


                        //Get selected city
                        //List<City> cityList = new List<City>();
                        // ------- Getting Data from Database Using EntityFrameworkCore -------
                        //cityList = (from city
                        //               in _context.Cites
                        //            where city.StateId == userProfile.StateId
                        //            select city).ToList();

                        // ------- Inserting Select Item in List -------
                        //cityList.Insert(userProfile.CityId, new City { CityId = 0, CityName = "Please select city" });

                        // ------- Assigning citylist to ViewBag.ListofCategory -------
                        //ViewBag.ListofCity = cityList;




                    }
                    return View("UserSettings", userProfile);
                }
                return RedirectToAction("Login", "Account");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View("UserSettings", userProfile);
            }


        }


        /// <summary>
        /// Save user profile detail in to the database
        /// </summary>
        /// <param name="collection"></param>
        /// <returns>Json Sucess true or false</returns>
        public JsonResult EditUserProfile(EditUserSetting collection)
        {
            try
            {

                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                collection.value = collection.value.Trim();
                if (collection.name == "firstname")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    user.FirstName = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });
                }

                if (collection.name == "lastname")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    user.LastName = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });
                }

                //if (collection.name == "companyname")
                //{
                //    var userdata = _context.Users.FirstOrDefault(u => u.Id == userid);
                //    var company = _context.Companies.FirstOrDefault(u => u.CompanyId == userid);
                //    company.CompanyName = collection.value;
                //    _context.SaveChanges();
                //    return Json(new { success = true });

                //}

                if (collection.name == "Phone")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    user.PhoneNumber = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });

                }


                if (collection.name == "companyName")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    var company = _context.Users.Include(e => e.Company).FirstOrDefault(e => e.Id == user.Id);
                    company.Company.CompanyName = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });

                }
                if (collection.name == "addressLine1")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    var address = _context.Users.Include(e => e.Address).FirstOrDefault(e => e.Id == user.Id);
                    address.Address.AddressLine1 = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });

                }
                if (collection.name == "addressLine2")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    var address = _context.Users.Include(e => e.Address).FirstOrDefault(e => e.Id == user.Id);
                    address.Address.AddressLine2 = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });
                }
                if (collection.name == "zipCode")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    var address = _context.Users.Include(e => e.Address).FirstOrDefault(e => e.Id == user.Id);
                    address.Address.ZipCode = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });
                }


            }
            catch (Exception)
            {

                return Json(new { success = false });
            }
            return Json(new { success = false });

        }



        [HttpPost]
        [AllowAnonymous]
        public JsonResult updateCST(UserProfileViewModel model)
        {
            try
            {
                if (model.CountryId > 0 && model.StateId > 0 && model.City  != string.Empty &&  model.City != null)
                {
                    int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    ApplicationUser currentUser = _context.Users.Where(x => x.Id == userid).First();
                    var address = _context.Users.Include(e => e.Address).FirstOrDefault(e => e.Id == userid);
                    address.Address.CountryId = model.CountryId;
                    address.Address.StateId = model.StateId;
                    address.Address.City = model.City;
                    _context.SaveChanges();
                    return Json(new { success = true, cscUpdate = true, message = "Address updated successfully" });
                }
                else
                {
                    return Json(new { success = true, sscRequired = true,  message = "Country state city must be required" });
                }


            }
            catch (Exception ex)
            {

                return Json(new { success = true, message = Constants.ExMsg });
            }
        }

        #endregion



    }
}