namespace OxyProcess.Controllers
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using OxyProcess.Models;
    using System.Diagnostics;
    using System.Security.Claims;

    public class HomeController : Controller
    {
        #region Property
        private readonly UserManager<ApplicationUser> _userManager;
        #endregion

        #region Constructor
        public HomeController(UserManager<ApplicationUser> um)
        {
            _userManager = um;
        }
        #endregion

        #region Action
        public IActionResult Index()
        {
           
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult UnderConstruction()
        {
            return View();
        }
        #endregion

    }
}
