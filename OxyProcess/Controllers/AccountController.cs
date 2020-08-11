namespace OxyProcess.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using OxyProcess.Extensions;
    using OxyProcess.Models;
    using OxyProcess.Models.AccountViewModels;
    using OxyProcess.Data;
    using OxyProcess.Models.Account;
    using OxyProcess.Models.UserAddress;
    using OxyProcess.Constant;
    using OxyProcess.Interface;
    using OxyProcess.Models.Industry;
    using System.Text.RegularExpressions;
    using OxyProcess.Models.Worker;
    using OxyProcess.Helpers;

    [Authorize]
    [Route("[controller]/[action]")]
    ///<summary>
    /// Account related methods 
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        public readonly ApplicationDbContext _context;
        public AccountController(
            UserManager<ApplicationUser> userManager,
             RoleManager<ApplicationRole> userRoleManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _context = context;

        }

        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Get login 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns>Login view</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
           
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Post login
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>Redirect user to particular dashboard</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/Home/Index")
        {
            try
            {
     
                ViewData["ReturnUrl"] = returnUrl;
                if (ModelState.IsValid)
                {
                    var user = _userManager.FindByNameAsync(model.Email).Result;
                    var isok = await _userManager.CheckPasswordAsync(user, model.Password);

                    if (isok == true)
                    {
                        if (user.Suspended == true)
                        {
                            ModelState.AddModelError(string.Empty, "your account has been suspended please contact administrator.");
                            return View(model);
                        }

                        if (!_userManager.IsEmailConfirmedAsync(user).Result)
                        {
                            ModelState.AddModelError("",
                            "Email is not confirmed please confirm your email first.");
                            return View("reconfirmEmail", model);
                            //return RedirectToAction("reconfirmEmail", "Account",model);
                        }
                    }


                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);


                   
                    if (result.Succeeded)
                    {
                    
                        _logger.LogInformation("User logged in.");
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
           
        }

        
             [HttpGet]
        [AllowAnonymous]
        public IActionResult reconfirmEmail()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> reconfirmEmail(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email.Trim().ToLower(), model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                model.Email= model.Email.Trim().ToLower();
                var user = _context.Users.FirstOrDefault(e => e.Email.ToLower() == model.Email.ToLower());
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
                return Json(new { Success = true, Message = "" });
            }
            else
            {
               
                return Json(new { Success = false, Message = "Account not found.Please contect admin." });
            }
           
        }

        /// <summary>
        /// Get register
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            return View();
        }


        /// <summary>
        /// Post register
        /// </summary>
        /// <param name="model"></param>
        /// <returns>success register</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //transaction method for prevent ghost Data.
            using (var transaction = _context.Database.BeginTransaction())
            {
             
                try
                {
                    if (ModelState.IsValid)
                    {
                        model.Email = model.Email.Trim().ToLower();
                        var existngUser = await _userManager.FindByEmailAsync(model.Email);
                        model.Phone = Regex.Replace(model.Phone, @"(\s+|@|&|'|-|\(|\)|<|>|#)", "");

                        if (existngUser == null)
                        {
                           

                            //Add Address
                            Address address = new Address();
                            address.AddressLine1 = model.AddressLine1;
                            address.AddressLine2 = model.AddressLine2;
                            address.ZipCode = model.ZipCode.Trim();
                            address.City = model.City;
                            address.StateId = model.StateId;
                            address.CountryId = model.CountryId;
                            _context.Addresses.Add(address);
                            _context.SaveChanges();
                            var addressID = address.AddressId;
                            Company company = new Company();
                            if (addressID != 0)
                            {
                                //Add Company
                                company.CompanyName = model.Company.Trim();
                                company.CreatedDate = DateTime.Now;
                                company.NoOfEmployees = Convert.ToInt64(model.NumberOfEmployes);
                                company.IndustryId = model.IndustryId;
                                _context.Companies.Add(company);

                            }
                            var companyId = company.CompanyId;

                      
                            var user = new ApplicationUser
                            {
                                UserName = model.Email,
                                Email = model.Email,
                                PhoneNumber = model.Phone,
                                FirstName = model.FirstName.Trim(),
                                LastName = model.LastName.Trim(),
                                CompanyId = companyId,
                                AddressId = addressID
                            };

                            var result = await _userManager.CreateAsync(user, model.Password);
                            if (result.Succeeded)
                            {
                            //.SaveChanges();
                                

                                if (model.UserType == "Admin" || model.UserType == "admin")
                                {
                                    await _userManager.AddToRoleAsync(user, "Admin");
                                }

                                if (model.UserType == "Customer" || model.UserType == "customer")
                                {
                                    await _userManager.AddToRoleAsync(user, "Customer");
                                }
                                //transaction.Commit();
                                _logger.LogInformation("User created a new account with password.");

                                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
                                await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
                                ViewData["Error"] = "false";
                                transaction.Commit();
                                return View("RegistrationSuccess", "Account");
                            }

                            // If we got this far, something failed, redisplay form
                            AddErrors(result);

                            transaction.Rollback();
                            return View(model);


                        }

                        else
                        {

                            ViewData["Error"] = "true";
                            ViewBag.errmsg = model.Email + " " + "is already taken";
                            transaction.Rollback();
                            return View(model);

                        }


                    }

                    ViewData["Error"] = "true";
                    transaction.Rollback();
                    return View(model);
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewData["Error"] = "true";
                    ModelState.AddModelError(string.Empty, Constants.ExMsg);
                    return View(model);
                }

            }
        }

    

        /// <summary>
        /// Logout user from 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _context.Dispose();

            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

       

        /// <summary>
        /// Email confirmation process for confirming user account
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <param name="NewEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string NewEmail = "")
        {
            try
            {
                
                IdentityResult result = null;
                if (userId == null || code == null)
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException($"Unable to load user with ID '{userId}'.");

                }
                if (user.EmailConfirmed == false)
                {
                    result = await _userManager.ConfirmEmailAsync(user, code);

                    if (result.Succeeded)
                    {
                        await _signInManager.SignOutAsync();
                        _logger.LogInformation("User logged out.");
                        ViewData["message"] = "";
                        await _emailSender.SendWelcomeMessage(user.Email,"");
                        
                        return View("ConfirmedEmail");
                    }

                    else
                    {
                        ViewData["message"] = "SomeThing wrong to change email please try again";
                        return View("ConfirmedEmail");
                    }

                }
                if (user.EmailConfirmed == true)
                {
                    NewEmail= NewEmail.Trim();
                    user.Email = NewEmail;
                    user.UserName = NewEmail;
                    result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignOutAsync();
                        _logger.LogInformation("User logged out.");
                        ViewData["message"] = "";
                        return View("ConfirmedEmail");
                    }
                    else
                    {
                        ViewData["message"] = Constants.ExMsg;
                        return View("ConfirmedEmail");

                    }
                }

                return View(result.Succeeded ? "ConfirmedEmail" : "Error");
            }
            catch (Exception)
            {
                ViewData["message"] = Constants.ExMsg;
                return View("ConfirmedEmail");
            }

         

        }

        /// <summary>
        /// Get email confirmation
        /// </summary>
        /// <returns>email confirmation view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmedEmail()
        {
            return View();
        }

        /// <summary>
        /// Forgotpassword
        /// </summary>
        /// <returns>Forgotpassword view</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
           
           @ViewData["ReturnUrl"] = "Account/ForgotPassword";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation(ForgotPasswordViewModel model)
        {
            return View(model);
        }

        /// <summary>
        /// Get uesr to reset password and send reset password link via email
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ForgotPasswordConfirmation view</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {

            try
            {
                if (ModelState.IsValid)
                {

                    model.Email=model.Email.Trim().ToLower();
                    var user = _context.Users.FirstOrDefault(e => e.Email.ToLower() == model.Email.ToLower());
                    if (user != null)
                    {
                        bool isconfom = await _userManager.IsEmailConfirmedAsync(user);
                        if (user == null || !isconfom || user.Suspended == true)
                        {
                            ModelState.AddModelError(string.Empty, "user email not found.");
                            return View();
                        }


                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
                        await _emailSender.resetpasswordConfirmationAsync(user.FirstName,user.LastName, model.Email, callbackUrl);
                       

                        return View(nameof(ForgotPasswordConfirmation), model.Email);
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "User email not found.");
                        return View();

                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View(model);
            }
         
        }




        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> ResendForgotPassword(ForgotPasswordViewModel model)
        {

            try
            {
                if (ModelState.IsValid)
                {

                    model.Email = model.Email.Trim().ToLower();
                    var user = _context.Users.FirstOrDefault(e => e.Email.ToLower() == model.Email.ToLower());
                    if (user != null)
                    {
                        bool isconfom = await _userManager.IsEmailConfirmedAsync(user);
                        if (user == null || !isconfom || user.Suspended == true)
                        {
                          
                            return Json(new { message = "user email not found.", success = false });
                        }


                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);
                        await _emailSender.resetpasswordConfirmationAsync(user.FirstName, user.LastName, model.Email, callbackUrl);


                        return Json(new { message = "Sucesss", success = true });
                    }
                    else
                    {

                        return Json(new { message = "user email not found.", success = false });
                      

                    }
                }

                // If we got this far, something failed, redisplay form
                return Json(new { message = "user email not found.", success = false });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return Json(new { message = "Something Wrong.", success = false });
            }

        }



        /// <summary>
        /// Get reset password page by clicking link on email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns>Reset password view</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code = null)
        {
            try
            {                
                ApplicationUser userinfo = await _userManager.FindByIdAsync(userId);
                Task<bool> isok = _userManager.VerifyUserTokenAsync(userinfo, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", code);
                if (isok.Result == false)
                {
                    ViewData["message"] = "linkexp";
                    return View("ConfirmedEmail");
                }

                if (code == null)
                {
                    throw new ApplicationException("A code must be supplied for password reset.");
                }
                var model = new ResetPasswordViewModel { Code = code, Email = userinfo.Email };
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View();
            }

         
        }


        /// <summary>
        /// post reset password  
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                model.Email = model.Email.Trim().ToLower();
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    //if user worker then update password in workerpass table
                   var UserIsWorker  = _userManager.IsInRoleAsync(user, "Worker");

                    if (UserIsWorker.Result == true)
                    {
                        //update worker password on worker password tbl 
                        var d = _context.WorkerPasswords.FirstOrDefault(e => e.WorkerId == user.Id);
                        d.WorkerPassword = EncryptDecryptHelper.encrypt(model.Password);
                        _context.SaveChanges();
                    }

                    return Redirect(nameof(ResetPasswordConfirmation));
                    //return View("ResetPasswordConfirmation", "Account");
                }
                AddErrors(result);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View(model);
            }
       
        }

        /// <summary>
        /// Get change email 
        /// </summary>
        /// <returns>ChangeEmailAddress view</returns>
        [HttpGet]
        
        [Authorize(Roles ="Admin")]
        public IActionResult ChangeEmailAddress()
        {
            ViewData["ReturnUrl"] = "Account/ChangeEmailAddress";
            return View();
        }

        /// <summary>
        /// Post ChangeEmailAddress
        /// </summary>
        /// <param name="models"></param>
        /// <returns>redirect to emailchangeconfirmation</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ChangeEmailAddress(ChangeEmailViewModel models)
        {
            try
            {
               
                if (!ModelState.IsValid)
                {
                    return View(models);
                }
                models.Email = models.Email.Trim().ToLower();
                var existngUser = await _userManager.FindByEmailAsync(models.Email);
                if (existngUser == null)
                {
                    int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    if (userid > 0 && user != null && code != null && code != "")
                    {
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code, NewEmail = models.Email }, protocol: Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(models.Email, callbackUrl);
                    }
                    else
                    {
                        ViewData["message"] = "Something is Wrong ! please try again";
                    }
                    return View("EmailChangeConfirmation");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, models.Email + " " + "is already taken");
                    return View(models);
                }
              
            }
            catch (Exception ex)
            {
                ViewData["message"] = Constants.ExMsg;
                return View("EmailChangeConfirmation");
            }
        
        }

        /// <summary>
        /// Get reset password page from user settings 
        /// </summary>
        /// <returns>reset password form</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetProfilePassword()
        {
            ResetProfilePasswordViewModel model = new ResetProfilePasswordViewModel();
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var email = _context.Users.Where(u => u.Id == userid).Select(p => p.Email).FirstOrDefault();
                model.Email = email;
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View();
            }
     
           
        }


        /// <summary>
        /// Update users passwors in database
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> ResetProfilePassword(ResetProfilePasswordViewModel models)
        {

            try
            {
               
                if (!ModelState.IsValid)
                {
                    return View(models);
                }

                if(models.OldPassword == null)
                {

                    ModelState.AddModelError("OldPassword", "The Old Password field is required.");
                    return View(models);

                }

                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var userinfo = await _userManager.FindByIdAsync(userid.ToString());
                var result = await _userManager.CheckPasswordAsync(userinfo, models.OldPassword);

                if (userid > 0 && userinfo != null)
                {
                    if (result == true)
                    {

                        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(userinfo);
                        IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(userinfo, resetToken, models.Password);
                        if (passwordChangeResult.Succeeded)
                        {
                            if (User.IsInRole("Worker"))
                            {
                                //update worker password on worker password tbl 
                                var d = _context.WorkerPasswords.FirstOrDefault(e => e.WorkerId == userid);
                                d.WorkerPassword = EncryptDecryptHelper.encrypt(models.Password);
                                _context.SaveChanges();
                            }

                            await _signInManager.SignOutAsync();
                            _logger.LogInformation("User logged out.");
                            return RedirectToAction(nameof(HomeController.Index), "Home");
                        }
                        else
                        {
                            models.ErrorMessage = "Something is Wrong !";
                            return RedirectToAction("Index", "UserSetting", models);
                        }
                    }
                    else
                    {
                        models.ErrorMessage = "Old password is Wrong !";

                        ModelState.AddModelError(string.Empty,"Old password is Wrong !");
                        return View(models);
                    }
                }
                else
                {

                    ModelState.AddModelError(string.Empty, Constants.ExMsg);
                    return View(models);
                }              
            }
            catch (Exception)
            {

                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View(models);
            }

        }

       
       /// <summary>
       /// Get reset password confirmation
       /// </summary>
       /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GroupsMembers(string term)
        {
            try
            {
                var ObjList = _context.Users.ToList();
                //    List<ApplicationUser> ObjList = new List<ApplicationUser>()
                //    {
                //        new ApplicationUser {Id=1,Email="jigneshparmar531@gmail.com" },
                //        new ApplicationUser {Id=2,Email="jigneshparmar531@gmail.com" },
                //        new ApplicationUser {Id=3,Email="jigneshparmar531@gmail.com" },
                //        new ApplicationUser {Id=4,Email="Yasheshbhavsar@gmail.com" },
                //        new ApplicationUser {Id=5,Email="divyeshp@gmail.com," },
                //        new ApplicationUser {Id=6,Email="vardhikprajapati@gmail.com" },
                //        new ApplicationUser {Id=7,Email="divyeshp@gmail.com" }
                //};
                var UserList = (from N in ObjList
                                where N.Email.ToLower().StartsWith(term.ToLower())
                                select new { N.Email });
                return Json(UserList);
            }
            catch (Exception)
            {
                return Json(null);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> EmailCheck(string email)
        {

            var existngUser = await _userManager.FindByEmailAsync(email);
            if (existngUser == null)
            {
                return Json("true");

            }
            else
            {
                return Json("Email account already exists");

            }
        }


        [HttpGet]
        [AllowAnonymous]
        public List<industries> GetindustryList()
        {
            try
            {
                List<industries> IndustryList = new List<industries>();
                try
                { 
                    IndustryList = _context.industries.OrderBy(e=>e.industrieType).ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return IndustryList;

            }
            catch
            {
                return null;

            }
            
        }


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        /// <summary>
        /// Get registration success message
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult InvalidAccess()
        {
            return View();
        }

        

        #endregion
    }
}
