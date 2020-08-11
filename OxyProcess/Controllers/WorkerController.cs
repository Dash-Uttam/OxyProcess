

namespace OxyProcess.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using OxyProcess.Constant;
    using OxyProcess.Data;
    using OxyProcess.Extensions;
    using OxyProcess.Helpers;
    using OxyProcess.Interface;
    using OxyProcess.Models;
    using OxyProcess.Models.AccountViewModels;
    using OxyProcess.Models.Worker;

    [Authorize]
    public class WorkerController : Controller
    {
        #region Property
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IEmailSender _emailSender;
        public readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Constructor
        public WorkerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IHttpContextAccessor  httpContextAcce)
        {
            _userManager = userManager;
            //    _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _httpContextAccessor = httpContextAcce;
        }
        #endregion

        #region Action
        public IActionResult Index()
        {
            return View();
        }



        [HttpGet]
        public string GetAllWorker()
        {
            try
            {
                
                Int64 userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                //List<GroupsListViewModel> groups = new List<GroupsListViewModel>();
                ApplicationUser currentUser = _context.Users.Where(x => x.Id == userid).First();
                //var currentUserRole = _context.UserRoles.Where(x => x.UserId == userid).FirstOrDefault();
                //var list = _context.Users.Where(u=>u.CompanyId == currentUser.CompanyId).Join(_context.UserRoles, p => p.Id, m => m.UserId, (p, m) => new { u = p, r = m }).Where(e =>  e.r.RoleId == 3).Select(s=>s.u).OrderByDescending(e=>e.Id).ToList();
                //var list = _context.Users.Join(_context.UserRoles, p => p.Id , m => m.UserId).Where(e => e.CompanyId == currentUser.CompanyId && e.AddressId == currentUser.AddressId)



                var list = (from user in _context.Users
                            join userrole in _context.UserRoles on user.Id equals userrole.UserId
                            join wkpass in _context.WorkerPasswords on user.Id equals wkpass.WorkerId
                            where userrole.RoleId == 3 && user.CompanyId == currentUser.CompanyId
                            select new
                            {

                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                Email = user.Email,
                                Suspended = user.Suspended,
                                Id = user.Id,
                              
                            }).OrderByDescending(e => e.Id).ToList();



                var json = JsonConvert.SerializeObject(list);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [Route("Worker/EditWorkerProfile/{uid}")]
        [HttpGet]

        public IActionResult EditWorkerProfile(string uid)
        {


            UserProfileViewModel userProfile = new UserProfileViewModel();
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    ApplicationUser currentUser = _context.Users.Where(x => x.Id == userid).First();
                    //var currentUserRole = _context.UserRoles.Where(x => x.UserId == userid).FirstOrDefault();
                    var list = (_context.Users.Where(u => u.CompanyId == currentUser.CompanyId).Join(_context.UserRoles, p => p.Id, m => m.UserId, (p, m) => new { u = p, r = m }).Where(e => e.r.RoleId == 3).Select(s => s.u).FirstOrDefault(x => x.Id == Convert.ToInt64(uid)));
                    if (list == null)
                    {
                        return RedirectToAction("InvalidAccess", "Account");
                    }


                    if (userid > 0)
                    {
                        userProfile = (from u in _context.Users
                                       join c in _context.Companies on u.CompanyId equals c.CompanyId
                                       join a in _context.Addresses on u.AddressId equals a.AddressId
                                       join p in _context.WorkerPasswords on u.Id equals p.WorkerId
                                       where u.Id == Convert.ToInt64(uid)
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
                                           ZipCode = a.ZipCode,
                                       }).FirstOrDefault();






                        //Get selected Country


                        // ------- Inserting Select Item in List -------
                        //cityList.Insert(userProfile.CityId, new City { CityId = 0, CityName = "Please select city" });

                        // ------- Assigning citylist to ViewBag.ListofCategory -------




                        ViewBag.userid = uid;
                    }
                    return View(userProfile);
                }
                return RedirectToAction("Login", "Account");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View(userProfile);
            }
        }




        public JsonResult EditUserProfile(EditUserSetting collection)
        {
            try
            {


                collection.value = collection.value.Trim();
                if (collection.name == "firstname")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == collection.pk);
                    user.FirstName = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });
                }

                if (collection.name == "lastname")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == collection.pk);
                    user.LastName = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });
                }

                if (collection.name == "Password")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == collection.pk);
                    var resetToken =  _userManager.GeneratePasswordResetTokenAsync(user);
                   var passwordChangeResult =  _userManager.ResetPasswordAsync(user, resetToken.Result, collection.value);
                    if (passwordChangeResult.Result.Succeeded)
                    {
                        
                            //update worker password on worker password tbl 
                            var d = _context.WorkerPasswords.FirstOrDefault(e => e.WorkerId == user.Id);
                            d.WorkerPassword = EncryptDecryptHelper.encrypt(collection.value);
                            _context.SaveChanges();
                       

                    }
                }



                if (collection.name == "Phone")
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == collection.pk);
                    user.PhoneNumber = collection.value;
                    _context.SaveChanges();
                    return Json(new { success = true });

                }


                if (collection.name == "Email")
                {

                    try
                    {

                        var existngUser = _context.Users.Where(e => e.Email.Trim().ToLower() == collection.value.Trim().ToLower()).FirstOrDefault();
                        if (existngUser == null)
                        {
                            var user = _context.Users.FirstOrDefault(u => u.Id == collection.pk);
                            user.Email = collection.value.Trim().ToLower();
                            user.NormalizedEmail = collection.value.Trim().ToLower();
                            user.NormalizedUserName = collection.value.Trim().ToLower();
                            user.UserName = collection.value;
                            _context.SaveChanges();
                            return Json(new { success = true });

                        }
                        else
                        {
                            return Json(new { success = false, message = "Exist" });

                        }
                    }

                    catch
                    {
                        return Json(new { success = false, message = "Something wrong" });


                    }


                }


            }
            catch (Exception)
            {

                return Json(new { success = false });
            }
            return Json(new { success = false });

        }



        //[HttpGet]
        //public IActionResult EditWorker(Int32 Id)
        //{
        //    RegisterViewModel Worker = new RegisterViewModel();
        //    try
        //    {
        //        Worker = (from u in _context.Users                           
        //                       where u.Id == Id
        //                       select new RegisterViewModel
        //                       {
        //                           FirstName = u.FirstName,
        //                           Email = u.Email,
        //                           Password = u.PasswordHash,
        //                           Phone = u.PhoneNumber,
        //                           LastName = u.LastName,
        //                       }).FirstOrDefault();                //get seleted user list by ID
        //        return View("CreateEditWorker", Worker);
        //    }
        //    catch (Exception)
        //    {
        //        ModelState.AddModelError(string.Empty, Constants.ExMsg);
        //        return View("CreateEditWorker", Worker);
        //    }


        //}




        [HttpPost]
        [AllowAnonymous]
        public JsonResult ChangeWorkerSuspendedStatus(ApplicationUser user, bool change)
        {

            try
            {
                Int64 userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApplicationUser currentUser = _context.Users.Where(x => x.Id == Convert.ToInt64(userid)).First();
                ApplicationUser suspendeduser = _context.Users.Where(x => x.Id == Convert.ToInt64(user.Id) && x.CompanyId == currentUser.CompanyId).First();
                suspendeduser.Suspended = change;
                _context.SaveChanges();
                return Json("True");
            }

            catch (Exception ex)
            {
                return Json("False");
            }

        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateWorker()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> CreateWorker(RegisterViewModel model)
        {
            try
            {
                if (model.FirstName.Trim() == "" || model.LastName.Trim() == "" || model.Email.Trim() == "" || model.FirstName == null || model.LastName == null || model.Email == null)
                {
                    return Json(new { message = "This field should not be blank" });
                }
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                ApplicationUser currentUser = _context.Users.Where(x => x.Id == userid).First();
                model.UserType = "Worker";

                model.Email = model.Email.Trim().ToLower();
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    CompanyId = currentUser.CompanyId,
                    AddressId = currentUser.AddressId,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _context.WorkerPasswords.Add(new WorkerPasswords() { WorkerPassword = EncryptDecryptHelper.encrypt(model.Password), WorkerId = Convert.ToInt32(user.Id) });
                    _context.SaveChanges();


                    string host = _httpContextAccessor.HttpContext.Request.Scheme + "://"+ _httpContextAccessor.HttpContext.Request.Host.Value;
                    await _userManager.AddToRoleAsync(user, "Worker");
                    await _emailSender.SendLoginCredentialAsync(model.Email, model.Email, model.Password, host);


                    return Json(new { message = "Success" });

                }
                else
                {
                    return Json(new { message = "Error_user_exist" });

                }

            }
            catch (Exception ex)
            {
                return Json(new { message = Constant.Constants.ExMsg });
            }
        }

        #endregion

    }
}