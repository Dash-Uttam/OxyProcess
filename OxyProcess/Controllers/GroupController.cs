namespace OxyProcess.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using OxyProcess.Data;
    using OxyProcess.Models;
    using OxyProcess.Models.Group;
    using OxyProcess.Models.GroupsViewModels;
    using OxyProcess.Constant;
    using Microsoft.AspNetCore.Authorization;

    [Route("[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class GroupController : Controller
    {
        #region Property
        public readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public GroupController(ApplicationDbContext context, UserManager<ApplicationUser> userManager/* IMapper mapper*/)
        {
            _context = context;
            _userManager = userManager;
            //_mapper = mapper;
        }
        #endregion

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GroupsMembers(string term)
        {
            try
            {

                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(e => e.Id == userid);
                var list = _context.Users.Join(_context.UserRoles, p => p.Id, m => m.UserId, (p, m) => new { u = p, r = m }).Where(e => e.r.RoleId == 3 && e.u.CompanyId == user.CompanyId || e.r.RoleId == 4).Select(s => s.u).OrderByDescending(e => e.Id).ToList();
                var UserList = (from N in list
                                where N.Email.ToLower().StartsWith(term.ToLower())
                                select new { N.Email });
                return Json(UserList);
            }
            catch (Exception)
            {
                return Json(null);
            }
        }


        #region Action
        //Index of group

        public ActionResult Index()
        {
            //if (TempData["Message"] != null)
            //{
            //    ViewBag.Msg = TempData["Message"];
            //}
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddGroup(AddGroupViewModel model)
        {
            //transaction method for prevent ghost Data.
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        /// check name charector is not more then 32 words
                        if (model.GroupName.Length >= 32)
                        {
                            ModelState.AddModelError(string.Empty, "Group name is too long.");
                            return View("Index", model);
                        }

                        model.GroupName = model.GroupName.Trim();

                        int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                        var groups = _context.Groups.Where(x => x.GroupName.ToLower() == model.GroupName.ToLower() && x.CreatedById == userid.ToString()).ToList().Count();
                        if (groups == 0)
                        {
                            Groups group = new Groups
                            {
                                GroupName = model.GroupName,
                                CreatedDate = DateTime.Now,
                                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier).Value,

                            };

                            _context.Groups.Add(group);
                            _context.SaveChanges();
                            var groupid = group.GroupId;
                            GroupPermission gp = new GroupPermission()
                            {
                                GroupId = groupid,
                                None = true
                            };
                            _context.GroupPermission.Add(gp);
                            _context.SaveChanges();
                            TempData.Remove("Message");
                            TempData.Add("Message", "Group Added Successfully");
                            //return RedirectToAction("AddGroup");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Group already exists please enter different group name.");


                            return View("Index", model);
                        }

                    }

                    transaction.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
            return RedirectToAction("Index");
        }



        /// <summary>
        /// Get all group list
        /// </summary>
        [HttpGet]
        public string GetAllGroups()
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                //List<GroupsListViewModel> groups = new List<GroupsListViewModel>();
                var list = _context.Groups.Where(e => e.CreatedById == userid.ToString()).OrderByDescending(e => e.GroupId).OrderByDescending(e => e.GroupId).ToList();
                var json = JsonConvert.SerializeObject(list);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        #endregion


        /// <summary>
        /// Edit group information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        #region EditGroupMemberPermission
        //[HttpPost]
        //[HttpGet]
        public IActionResult Edit(int id)
        {
            var groupName = _context.Groups.Where(e => e.GroupId == id).FirstOrDefault();
            int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (groupName == null)
            {
                return RedirectToAction("InvalidAccess", "Account");
            }

            if (groupName.CreatedById != userid.ToString())
            {
                return RedirectToAction("InvalidAccess", "Account");
            }

            var grouppermission = _context.GroupPermission.Where(x => x.GroupId == id).FirstOrDefault();
            var model = new GroupMemberPermissionModel();
            model.addMemberToGroupViewModel = new AddMemberToGroupViewModel();
            model.groupPermissionViewModel = new GroupPermissionViewModel();
            model.addMemberToGroupViewModel.GroupId = id;
            model.groupPermissionViewModel.GroupId = id;
            model.GroupName = groupName.GroupName;



            if (grouppermission != null)
            {
                model.groupPermissionViewModel.Read = grouppermission.Read;
                model.groupPermissionViewModel.Write = grouppermission.Write;
                model.groupPermissionViewModel.Edit = grouppermission.Edit;
                model.groupPermissionViewModel.Delete = grouppermission.Delete;
                model.groupPermissionViewModel.None = grouppermission.None;
            }
            //if (TempData["Message"] != null)
            //{
            //    ViewBag.Msg = TempData["Message"];
            //    TempData["Message"] = null;
            //}
            return View(model);
        }

        /// <summary>
        /// Add member to the particular group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AddMemberToGroup(GroupMemberPermissionModel model)
        //{
        //    try
        //    {

        //        if (ModelState.IsValid)
        //        {
        //            var user = _userManager.FindByEmailAsync(model.addMemberToGroupViewModel.Email).Result;
        //            model.GroupName = _context.Groups.Where(e => e.GroupId == model.addMemberToGroupViewModel.GroupId).Select(g => g.GroupName).FirstOrDefault();
        //            if (user == null)
        //            {
        //                //ModelState.AddModelError(string.Empty, "User not Found ");
        //                //return View("Edit", model);
        //                ViewData["NotFound"] = "true";
        //                return View("Edit", model);
        //            }
        //            else
        //            {
        //                var grpmember = _context.GroupMembers.ToList().Where(x => x.UserId == user.Id && x.GroupId == model.addMemberToGroupViewModel.GroupId).Count();
        //                if (grpmember == 0)
        //                {
        //                    GroupMembers group = new GroupMembers
        //                    {
        //                        GroupId = model.addMemberToGroupViewModel.GroupId,
        //                        UserId = user.Id,
        //                    };
        //                    _context.GroupMembers.Add(group);
        //                    _context.SaveChanges();
        //                    TempData.Add("Message", "Group Member Added Successfully");
        //                    return RedirectToAction("Edit", new { Id = model.addMemberToGroupViewModel.GroupId });
        //                }
        //                else
        //                {
        //                    //ModelState.AddModelError(string.Empty, "User Already Available");
        //                    //return View("Edit", model);
        //                    ViewData["UserAvailable"] = "true";
        //                    return View("Edit", model);
        //                }

        //            }
        //        }

        //        var error = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        //        ViewData["EmailRequired"] = "true";
        //        model.GroupName = _context.Groups.Where(e => e.GroupId == model.addMemberToGroupViewModel.GroupId).Select(g => g.GroupName).FirstOrDefault();
        //        return View("Edit",model);
        //    }
        //    catch (Exception)
        //    {
        //        ViewData["error"] = "true";
        //        ViewData["Message"] = Constants.ExMsg;
        //        model.GroupName = _context.Groups.Where(e => e.GroupId == model.addMemberToGroupViewModel.GroupId).Select(g => g.GroupName).FirstOrDefault();
        //        return View("Edit", model);
        //    }           
        //}

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [AllowAnonymous]
        public JsonResult AddMemberToGroup(GroupMemberPermissionModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var userdata = _context.Users.FirstOrDefault(e => e.Id == userid);



                    var user = _userManager.FindByEmailAsync(model.addMemberToGroupViewModel.Email).Result;

                    model.GroupName = _context.Groups.Where(e => e.GroupId == model.addMemberToGroupViewModel.GroupId).Select(g => g.GroupName).FirstOrDefault();
                    if (user == null)
                    {
                        return Json(new { notFound = true, success = true, message = Constants.userNotFound, HardSucess = false });
                    }
                    else
                    {

                        var rolename = _userManager.GetRolesAsync(user).Result[0];

                        if (user.CompanyId == userdata.CompanyId || rolename == "Customer")
                        {


                            var grpmember = _context.GroupMembers.ToList().Where(x => x.UserId == user.Id && x.GroupId == model.addMemberToGroupViewModel.GroupId).Count();
                            if (grpmember == 0)
                            {


                                //GroupMembers group = new GroupMembers
                                //{
                                //    GroupId = model.addMemberToGroupViewModel.GroupId,
                                //    UserId = user.Id,
                                //};
                                //_context.GroupMembers.Add(group);
                                //_context.SaveChanges();

                                return Json(new { inserrtSuccess = true, success = true, Email = model.addMemberToGroupViewModel.Email, Verify = true, message = Constants.memberAddSuccess, HardSucess = false });
                            }
                            else
                            {
                                return Json(new { userAvailable = true, success = true, message = Constants.memberAvailable, HardSucess = false });
                            }
                        }

                        else
                        {

                            return Json(new { notFound = true, success = true, message = Constants.userNotFound, HardSucess = false });
                        }
                    }
                }
                var error = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                model.GroupName = _context.Groups.Where(e => e.GroupId == model.addMemberToGroupViewModel.GroupId).Select(g => g.GroupName).FirstOrDefault();
                return Json(new { emailRequired = true, success = true, message = Constants.emailRequired, HardSucess = false });
            }
            catch (Exception ex)
            {
                model.GroupName = _context.Groups.Where(e => e.GroupId == model.addMemberToGroupViewModel.GroupId).Select(g => g.GroupName).FirstOrDefault();
                return Json(new { success = false, message = Constants.ExMsg, HardSucess = false });
            }

        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [AllowAnonymous]
        public JsonResult SaveGroupChanges(GroupChanges groupChanges)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {

                try
                {

                    //User re-Varify
                    var groupName = _context.Groups.Where(e => e.GroupId == groupChanges.GroupId).FirstOrDefault();
                    int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                    if (groupName.CreatedById != userid.ToString())
                    {
                        return Json(new { message = "InvalidAccess !", success = false });

                    }








                    //Group Name Update
                    var groupfound = _context.Groups.Where(x => x.GroupName.ToLower() == groupChanges.GroupName.ToLower() && x.CreatedById == userid.ToString()).ToList().Count();


                    if (groupfound == 0)
                    {

                        Groups groups = _context.Groups.Where(e => e.GroupId == groupChanges.GroupId).FirstOrDefault();
                        groups.GroupName = groupChanges.GroupName;


                    }

                    else
                    {

                        Groups groups = _context.Groups.Where(e => e.GroupId == groupChanges.GroupId).FirstOrDefault();
                        if (groups.GroupName.ToLower() == groupChanges.GroupName.ToLower())
                        {
                            //return Json(new { Value = true, message = "No changes made." });

                        }
                        else
                        {
                            return Json(new { Value = false, message = "Group already exists please enter different group name." });
                        }
                    }



                    //group permission update
                    var grouppermission = _context.GroupPermission.Where(x => x.GroupId == groupChanges.GroupId).FirstOrDefault();

                    if (grouppermission != null)
                    {

                        //if (grouppermission.Read == groupChanges.GroupPermission.Read && grouppermission.Write == groupChanges.GroupPermission.Write && groupChanges.GroupPermission.Edit && grouppermission.Delete == groupChanges.GroupPermission.Delete && grouppermission.None == groupChanges.GroupPermission.None)
                        //{
                        //    return Json(new { permissionEditSuccess = false, success = false, message = "No changes were made" });

                        //}

                        //grouppermission.GroupId = groupChanges.GroupId;
                        grouppermission.Read = groupChanges.GroupPermission.Read;
                        grouppermission.Write = groupChanges.GroupPermission.Write;
                        grouppermission.Edit = groupChanges.GroupPermission.Edit;
                        grouppermission.Delete = groupChanges.GroupPermission.Delete;
                        grouppermission.None = groupChanges.GroupPermission.None;

                        _context.GroupPermission.Update(grouppermission);

                        //TempData.Add("Message", "Group Permission Update Successfully");
                    }
                    else
                    {

                        return Json(new { message = "Something Wrong !", success = false });

                    }



                    //Member Re-Varify
                    if (groupChanges.GroupMemberList == null || groupChanges.GroupMemberList.Trim() == string.Empty)
                    {
                        var GroupMembersList = _context.GroupMembers.Where(e => e.GroupId == groupChanges.GroupId).ToList();
                        _context.GroupMembers.RemoveRange(GroupMembersList);
                    }
                    else
                    {

                        var memberemail = groupChanges.GroupMemberList.Split(",");

                        //old email ingroup delete
                        var GroupMembersList = new List<GroupMembers>();
                        _context.GroupMembers.RemoveRange(_context.GroupMembers.Where(e => e.GroupId == groupChanges.GroupId).ToList());
                        _context.SaveChanges();

                        foreach (var listemail in memberemail)
                        {
                            Int64 UserId = _context.Users.FirstOrDefault(e => e.Email == listemail).Id;
                            GroupMembersList.Add(new GroupMembers() { GroupId = groupChanges.GroupId, UserId = UserId });

                        }
                        _context.GroupMembers.AddRange(GroupMembersList);
                    }


                    _context.SaveChanges();
                    transaction.Commit();
                    return Json(new { message = "Group successfully updated.", success = true });
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { message = "Something Wrong !", success = false });

                }
            }
        }
        /// <summary>
        /// get all user exists in particular group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public string getAllUserGroupWise(int GroupId)
        {
            try
            {
                var list = _context.GroupMembers.Include(x => x.User).Where(x => x.GroupId == GroupId).ToList();
                var json = JsonConvert.SerializeObject(list);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteGRoupUsermember(int GroupMemberId)
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                _context.GroupMembers.Remove(_context.GroupMembers.Where(x => x.GroupMemberId == GroupMemberId).SingleOrDefault());

                _context.SaveChanges();
                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                return new JsonResult("something went wrong");
            }
        }
        //Delete Group 
        [HttpPost]
        public JsonResult DeleteGroup(int id)
        {
            try
            {
                _context.Groups.Remove(_context.Groups.FirstOrDefault(x => x.GroupId == id));
                _context.GroupMembers.RemoveRange(_context.GroupMembers.Where(x => x.GroupId == id));
                _context.GroupPermission.RemoveRange(_context.GroupPermission.Where(x => x.GroupId == id));
                _context.SaveChanges();
                return new JsonResult(new { value = true });
            }
            catch (Exception)
            {
                return new JsonResult(new { value = Constants.ExMsg });
            }

        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GroupPermission(GroupMemberPermissionModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var grouppermission = _context.GroupPermission.Where(x => x.GroupId == model.groupPermissionViewModel.GroupId).FirstOrDefault();

                    if (grouppermission != null)
                    {

                        if (grouppermission.Read == model.groupPermissionViewModel.Read && grouppermission.Write == model.groupPermissionViewModel.Write && grouppermission.Edit == model.groupPermissionViewModel.Edit && grouppermission.Delete == model.groupPermissionViewModel.Delete && grouppermission.None == model.groupPermissionViewModel.None)
                        {
                            return Json(new { permissionEditSuccess = false, success = false, message = "No changes were made" });

                        }


                        //grouppermission.GroupId = model.groupPermissionViewModel.GroupId;
                        grouppermission.Read = model.groupPermissionViewModel.Read;
                        grouppermission.Write = model.groupPermissionViewModel.Write;
                        grouppermission.Edit = model.groupPermissionViewModel.Edit;
                        grouppermission.Delete = model.groupPermissionViewModel.Delete;
                        grouppermission.None = model.groupPermissionViewModel.None;

                        _context.GroupPermission.Update(grouppermission);
                        _context.SaveChanges();
                        //TempData.Add("Message", "Group Permission Update Successfully");
                        return Json(new { permissionEditSuccess = true, success = true, message = "Group Permission Update Successfully" });
                        //return RedirectToAction("Edit", new { Id = model.groupPermissionViewModel.GroupId });
                    }
                    //else
                    //{
                    //    GroupPermission groupPermission = new GroupPermission
                    //    {
                    //        GroupId = model.groupPermissionViewModel.GroupId,
                    //        Read = model.groupPermissionViewModel.Read,
                    //        Write = model.groupPermissionViewModel.Write,
                    //        Edit = model.groupPermissionViewModel.Edit,
                    //        Delete = model.groupPermissionViewModel.Delete,
                    //        None = model.groupPermissionViewModel.None
                    //    };
                    //    _context.GroupPermission.Add(groupPermission);
                    //    _context.SaveChanges();
                    //    //TempData.Add("Message", "Group Permission Added Successfully");

                    //    return Json(new { permissionAddSuccess = true, success = true, message = "Group Permission Added Successfully" });

                    //    //return RedirectToAction("Edit", new { Id = model.groupPermissionViewModel.GroupId });
                    //}
                }
                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = Constants.ExMsg });
            }

        }




    }


    #endregion
}

