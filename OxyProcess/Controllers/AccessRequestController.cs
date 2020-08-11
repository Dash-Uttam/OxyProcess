using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OxyProcess.Data;
using OxyProcess.Models.SpecialPermission;

namespace OxyProcess.Controllers
{
    public class AccessRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccessRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AccessRequest
        public ActionResult Index()
        {
            return View();
        }

        //GET: Pending Request
        public ActionResult PendingRequest()
        {
            return View();
        }


        //GET: Pending Request List
        [HttpGet]
        public ActionResult PendingRequestList()
        {
            try {


                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(e => e.Id == userid);
                var AccessReq = _context.SpecialPermission.Where(e => e.Requested_User == user.Id).ToList();

                IQueryable data = (from u in _context.Users
                                join accreq in AccessReq on u.Id equals accreq.Requested_User
                                join tegintemp in _context.TaginsideTemplates on accreq.Unique_Template_Id equals tegintemp.TemplateuniqueId
                                join tag in _context.Tag on tegintemp.TagId equals tag.Id
                                join temp in _context.Template on tegintemp.OrignalTemplateId equals temp.TemplateId
                                select new RequestPersonViewModel
                                {
                                    Id = accreq.Id,
                                    AccessPermission = accreq.AccessGranted,
                                    Tag = tag.TagNumber,
                                    Reject = accreq.Rejected,
                                    TemplateName = temp.TemplateName

                                }).Distinct();


                return Json(new { Sucesss = false, Data = data });
            }
            catch
            {
                return Json(new { Sucesss = false, Data ="" }) ;

            }
        }

        // Post: AccessRequest/RejectRequest/5
        [HttpPost]
        public ActionResult RejectRequest(int Id)
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (userid != 0)
                {
                    var request = _context.SpecialPermission.FirstOrDefault(e => e.Id == Id);
                    request.Rejected = true;
                    _context.SaveChanges();
                    return Json(new { Success = true});
                }
                return Json(new { Success = false });
            }
            catch
            {
                return Json(new { Success = false });
            }
        }

        public JsonResult GetAllRequest()
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(e => e.Id == userid);
                var AccessReq = _context.SpecialPermission.Where(e => e.Company_Id == user.CompanyId && e.Rejected == false).ToList();

                IQueryable data = (from u in _context.Users
                                join accreq in AccessReq on u.Id equals accreq.Requested_User
                                join tegintemp in _context.TaginsideTemplates on accreq.Unique_Template_Id equals tegintemp.TemplateuniqueId
                                join tag in _context.Tag on tegintemp.TagId equals tag.Id
                                join temp in _context.Template on tegintemp.OrignalTemplateId equals temp.TemplateId
                                select new RequestPersonViewModel
                                {
                                    Id = accreq.Id,
                                    FullName = u.LastName + " " + u.LastName,
                                    AccessPermission = accreq.AccessGranted,
                                    UserName = u.Email,
                                    Tag = tag.TagNumber,
                                    TemplateName = temp.TemplateName

                                }).Distinct();

                return Json(data);
            }
            catch
            {
                return Json("False");


            }
        }


        [HttpPost]
        public JsonResult ChangePermission(int Id, bool change)
        {

            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var user = _context.Users.FirstOrDefault(e => e.Id == userid);
                var AccessReq = _context.SpecialPermission.FirstOrDefault(e => e.Company_Id == user.CompanyId && e.Id == Id);

                if (AccessReq != null)
                {
                    AccessReq.AccessGranted = change;
                    _context.SaveChanges();
                    return Json("True");

                }
                return Json("False");

            }

            catch (Exception ex)
            {
                return Json("False");
            }

        }

    }
}