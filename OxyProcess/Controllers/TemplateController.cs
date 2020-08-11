namespace OxyProcess.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using OxyProcess.Constant;
    using OxyProcess.Data;
    using OxyProcess.Models;
    using OxyProcess.Models.FormTag;
    using OxyProcess.Models.Template;
    using OxyProcess.ViewModels.FormTag;
    using OxyProcess.ViewModels.Template;
    using AutoMapper;
    using System.Collections;

    [Authorize(Roles = "Admin")]
    public class TemplateController : Controller
    {
        #region Property  
        public readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly IMapper _mapper;
        #endregion
        private readonly IMapper _mapper;
        public TemplateController(ApplicationDbContext context, IMapper mapper, SignInManager<ApplicationUser> s/*, IMapper mapper*/)
        {
            _context = context;
            _signInManager = s;
            _mapper = mapper;
            //_mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
          
        }


       
        //Get Templatelist By User 
        public string GetTemplatelist()
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var list = _context.Template.Include(e => e.TemplateType).Where(e => e.CreatedById == userid.ToString() && e.IsDeleted == false).OrderByDescending(e => e.TemplateId).ToList();
                var json = JsonConvert.SerializeObject(list);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Change Template Activation
        [HttpPost]
        [AllowAnonymous]
        public JsonResult ChangeTemplateActivation(Template template)
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                Template TemplateUpdate = _context.Template.FirstOrDefault(e => e.TemplateId == template.TemplateId && e.CreatedById == userid.ToString());
                TemplateUpdate.IsActive = template.IsActive;
                _context.SaveChanges();
                return Json("True");
            }

            catch (Exception ex)
            {
                return Json("False");
            }
        }

        [Route("Template/CreateTemplate")]
        [HttpGet]
        public IActionResult CreateTemplate()
        {
            int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ViewData["Groups"] = _context.Groups.Where(e => e.CreatedById == userid.ToString()).ToList();
            ViewBag.TemplateType = new SelectList(_context.TemplateType, "TypeId", "TemplateTypeName");
            return View();
        }
        //Create new Template Post method 
        [HttpPost]
        [AllowAnonymous]
        public JsonResult CreateEditTemplate(TemplateViewModel model)
        {
            try
            {

                if (_signInManager.IsSignedIn(User))
                {
                    int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var user = _context.Users.FirstOrDefault(e => e.Id == userId);

                    if (model.TemplateName == "" || model.TemplateCode == "" || model.Groups == "")
                    {
                        return Json(new { message = "Please enter filds Data." });
                    }

                    if (model.TemplateId > 0)
                    {

                        Template template = _context.Template.FirstOrDefault(e => e.TemplateId == model.TemplateId);
                        if (template.TemplateName.ToLower().Trim() != model.TemplateName.ToLower().Trim())
                        {

                            var templateName = _context.Template.Count(e => e.TemplateName.ToLower() == model.TemplateName.ToLower() && e.CreatedById == userId.ToString());
                            if (templateName != 0)
                            {
                                ViewData["TemplateNameAvailable"] = "true";
                                return Json(new { message = "Template name is already exist." });
                            }
                        }

                        //if template is reportcard remove add custome titel field from db
                        if (model.TemplateTypeId == 1)
                        {
                            var dbcode = JsonConvert.DeserializeObject<List<object>>(template.TemplateCode);
                            
                            var modelcode = JsonConvert.DeserializeObject<List<object>>(model.TemplateCode);
                            //re-add db code titel to current model code 
                            modelcode.Insert(0,dbcode[0]);
                            model.TemplateCode = JsonConvert.SerializeObject(modelcode);

                        }

                        template.TemplateCode = model.TemplateCode;
                        template.TemplateName = model.TemplateName.Trim();
                        template.TemplateTypeId = model.TemplateTypeId;
                        template.Groups = model.Groups;
                        template.CreatedDate = template.CreatedDate;
                        template.CreatedById = userId.ToString();
                        template.IsActive = template.IsActive;
                        template.IsDeleted = false;
                        template.CompanyId = (int)user.CompanyId;
                        _context.SaveChanges();
                        //return Json(new { message = "Sucess" });
                        var templateuseintag = _context.TaginsideTemplates.Where(e => e.OrignalTemplateId == model.TemplateId);

                        List<TemplateFields> templateFields = new List<TemplateFields>();

                        foreach (var single in templateuseintag)
                        {
                            var templateuseintag2 = _context.TemplateFields.Where(e => e.TemplateuniqueId == single.TemplateuniqueId);

                            var templateField = JsonConvert.DeserializeObject<List<TemplateFieldsViewModel>>(model.TemplateCode);


                            foreach (var tField in templateField)
                            {

                                if (templateuseintag2.Count(x => x.name == tField.name && x.type == tField.type) == 0)
                                {

                                    var output = _mapper.Map<TemplateFields>(tField);
                                    output.TemplateId = single.OrignalTemplateId;
                                    output.TemplateuniqueId = single.TemplateuniqueId;
                                    output.TemplateId = single.OrignalTemplateId;
                                    output.TagId = single.TagId;
                                    output.IsActive = true;
                                    output.LatestValue = "";
                                    templateFields.Add(output);
                                }

                            }
                            //all template clone code change
                            single.TemplateCloneCode = model.TemplateCode;

                        }
                        _context.TemplateFields.AddRange(templateFields);
                        _context.SaveChanges();
                        return Json(new { message = "Sucess", templateid = template.TemplateId });
                    }
                    else
                    {
                        var templateName = _context.Template.Count(e => e.TemplateName.ToLower().Trim() == model.TemplateName.ToLower().Trim() && e.CreatedById == userId.ToString() && e.IsDeleted == false);
                        if (templateName == 0)
                        {
                            model.CreatedById = userId.ToString();
                            model.IsActive = true;
                            Template template = new Template();
                            template.TemplateCode = model.TemplateCode;
                            template.TemplateName = model.TemplateName.Trim();
                            template.TemplateTypeId = model.TemplateTypeId;
                            template.Groups = model.Groups;
                            template.CreatedDate = model.CreatedDate;
                            template.CreatedById = model.CreatedById;
                            template.IsActive = model.IsActive;
                            template.IsDeleted = false;
                            template.CompanyId = (int)user.CompanyId;
                            //_context.Template.Add(_mapper.Map<Template>(model));
                            _context.Template.Add(template);
                            _context.SaveChanges();
                            return Json(new { message = "Sucess", templateid = template.TemplateId });
                        }
                        else
                        {
                            ViewData["TemplateNameAvailable"] = "true";
                            return Json(new { message = "Template name is already exist." });
                        }
                    }



                }
                else
                {
                    ViewData["TemplateNameAvailable"] = "true";
                    return Json(new { message = "Something went wrong please try again." });

                }
            }

            catch (Exception ex)
            {
                ViewData["TemplateNameAvailable"] = "true";
                return Json(new { message = "Something went wrong please try again." });
            }

        }

        //Get Edit Template by Id
        [HttpGet]
        [Route("Template/EditTemplate/{tempid}")]
        public IActionResult EditTemplate(Int32 tempid)
        {
            var Template = _context.Template.FirstOrDefault(e => e.TemplateId == tempid);
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                if (Template == null)
                {
                    return RedirectToAction("InvalidAccess", "Account");

                }

                if (Template.CreatedById != userid.ToString())
                {
                    return RedirectToAction("InvalidAccess", "Account");

                }

                ViewData["Groups"] = _context.Groups.Where(e => e.CreatedById == userid.ToString()).ToList();
                //get seleted group list ID

                List<int> groupids = Template.Groups.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();

                ViewData["SeletedGroupsList"] = groupids;
                ViewBag.TemplateType = new SelectList(_context.TemplateType, "TypeId", "TemplateTypeName", _context.Template.FirstOrDefault(e => e.TemplateId == tempid).TemplateTypeId);
              
                //if template is reportcard remove first custome titel field 
                if(Template.TemplateTypeId == 1) {
                    var obj = JsonConvert.DeserializeObject<List<object>>(Template.TemplateCode);
                    obj.RemoveAt(0);
                    Template.TemplateCode = JsonConvert.SerializeObject(obj);

                }
                


                return View("CreateTemplate", Template);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, Constants.ExMsg);
                return View("CreateTemplate", Template);
            }


        }
        ////Delete Template
        [HttpPost]
        public JsonResult DeleteTemplate(int id)
        {
            try
            {
                //_context.Template.Remove(_context.Template.FirstOrDefault(x => x.TemplateId == id));
                var templateforDelete = _context.Template.FirstOrDefault(e => e.TemplateId == id);
                templateforDelete.IsDeleted = true;
                _context.SaveChanges();
                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                return new JsonResult(Constants.ExMsg);
            }

        }



        [Route("Template/ViewTemplate/{Templateid}")]
        [HttpGet]
        public ActionResult ViewTemplate(int Templateid)
        {
            try
            {
                Template templatedata = new Template();
                templatedata.TemplateId = Templateid;
                return View(templatedata);
            }
            catch (Exception ex)
            {
                return View(ex);
            }

        }



        [HttpGet]
        public JsonResult GetViewTemplate(Template template)
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);


                Template templatedata = _context.Template.FirstOrDefault(e => e.TemplateId == template.TemplateId);

                if (templatedata == null)
                {
                    return Json(401);

                }

                if (templatedata.CreatedById != userid.ToString())
                {
                    return Json(401);

                }

                return Json(templatedata);
            }
            catch (Exception ex)
            {
                return Json(null);
            }

        }

    }

}