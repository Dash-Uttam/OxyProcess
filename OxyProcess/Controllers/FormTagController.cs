namespace OxyProcess.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Newtonsoft.Json;
    using OxyProcess.Constant;
    using OxyProcess.Data;
    using OxyProcess.Models.FormTag;
    using OxyProcess.Models.Template;
    using OxyProcess.ViewModels.FormTag;
    using AutoMapper;
    using OxyProcess.Helpers;
    using OxyProcess.Models.SpecialPermission;
    using System.Runtime.CompilerServices;

    public class FormTagController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _env;
        public FormTagController(ApplicationDbContext _ApplicationDbContext, IMapper mapper, IHostingEnvironment env)
        {
            _env = env;
            _mapper = mapper;
            _context = _ApplicationDbContext;
        }

        [Authorize(Roles = "Admin,Worker")]
        public IActionResult Index()
        {
            return View("CreateTag");
        }

        [Authorize(Roles = "Admin,Worker")]
        public IActionResult FindTagId()
        {
            return View("FindTagId");
        }

      

        /// <summary>
        /// Tag Search Result
        /// </summary>
        /// <param name="SearchTag"></param>
        /// <returns>Tag Forms</returns>
        [HttpGet]
        public IActionResult TagSearchResult(string SearchTag)
        {
            try
            {
                var tag = _context.Tag.FirstOrDefault(e => e.TagNumber == SearchTag);

                if (tag == null)
                {

                    TempData["TagErrorMessage"] = "Tag not found";
                    return RedirectToAction("Index", "Home");

                }


                return View(tag);
            }
            catch
            {

                return View();
            }
        }

        /// <summary>
        /// Get Fill Form Data
        /// </summary>
        /// <param name="Templateid"></param>
        /// <returns>ID Of Template</returns>

        [HttpGet]
        public ActionResult FillFormData(int TemplateUniqueId, string returnSearchTag, int DataEntryID)
        {
            try
            {


                var Template = (from taginsideTemplates in _context.TaginsideTemplates
                                join
                              template in _context.Template on taginsideTemplates.OrignalTemplateId equals template.TemplateId
                                where taginsideTemplates.TemplateuniqueId == TemplateUniqueId
                                select template).FirstOrDefault();

                var TemplateType = Template.TemplateTypeId;



                if (TemplateType == 1) //if template is reguler to redirect multipal entry page
                    {
                  
                        return RedirectToAction("RegulerFormEntries", new { formUniqueId = TemplateUniqueId, returnSearchTag = returnSearchTag });

                    }
                    var TagFormDataEntry = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId);


                    if (TagFormDataEntry == null)
                    {

                    CheckUserRightsHelper checkUserRightsHelper = new CheckUserRightsHelper(_context);
                    int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                    var userRights = checkUserRightsHelper.CheckUserAcceess(Template.TemplateId, Currentuserid);
                    //if user has no permission for view 
                    if (userRights.Write == false)
                    {
                        return RedirectToAction("TagReportCardList", new { SearchTag = returnSearchTag });
                        //user has nop permission logic
                    }

                        TaginsidTemplatesViewModel TaginsideTemplatesData = new TaginsidTemplatesViewModel();
                        TaginsideTemplatesData.TemplateuniqueId = TemplateUniqueId;
                        TaginsideTemplatesData.TagNumber = returnSearchTag;
                        return View(TaginsideTemplatesData);
                    }

                    else
                    {

                        return RedirectToAction("ViewEditFormData", new { TemplateUniqueId = TemplateUniqueId, returnSearchTag = returnSearchTag, DataEntryID = TagFormDataEntry.Id });

                    }
               
            }
            catch (Exception ex)
            {
                return View(ex);
            }

        }


        [HttpGet]
        public ActionResult NewFormDataForReguler(int TemplateUniqueId, string returnSearchTag)
        {
            try
            {

                int OrignalTemplateId = _context.TaginsideTemplates.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId).OrignalTemplateId;
                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                CheckUserRightsHelper checkUserRightsHelper = new CheckUserRightsHelper(_context);

                var userRights = checkUserRightsHelper.CheckUserAcceess(OrignalTemplateId, Currentuserid);

                if(userRights.Write == true)
                {


            
                var TagFormDataEntry = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId);
                var TemplateType = (from taginsideTemplates in _context.TaginsideTemplates
                                    join
                                  template in _context.Template on taginsideTemplates.OrignalTemplateId equals template.TemplateId
                                    where taginsideTemplates.TemplateuniqueId == TemplateUniqueId
                                    select template).Select(e => e.TemplateTypeId).FirstOrDefault();


                if (TemplateType == 1) //  if reguler form then  create new form every time. 
                {
                    TaginsidTemplatesViewModel TaginsideTemplatesData = new TaginsidTemplatesViewModel();
                    TaginsideTemplatesData.TemplateuniqueId = TemplateUniqueId;
                    TaginsideTemplatesData.TagNumber = returnSearchTag;
                    return View("FillFormData", TaginsideTemplatesData);

                }

                else
                {
                    return View();

                }


                }
                else{

                    return RedirectToAction("Index","Home");
                }


            }
            catch (Exception ex)
            {
                return View(ex);
            }

        }


        [HttpGet]
        public ActionResult EditViewFormDataForReguler(int TemplateUniqueId, string returnSearchTag, int DataEntryId)
        {
            try
            {


                return RedirectToAction("ViewEditFormData", new { TemplateUniqueId = TemplateUniqueId, returnSearchTag = returnSearchTag, DataEntryId = DataEntryId });

            }
            catch (Exception ex)
            {
                return View(ex);
            }

        }

        /// <summary>
        /// Get New Form Field Data
        /// </summary>
        /// <param name="SearchTag"></param>
        /// <returns>Tag inside Templates</returns>
        [HttpGet]
        public JsonResult GetNewFormFieldData(int TemplateuniqueId)
        {
            try
            {
                var data = _context.TaginsideTemplates.FirstOrDefault(e => e.TemplateuniqueId == TemplateuniqueId);
                var Tag = _context.Tag.FirstOrDefault(e => e.Id == data.TagId);
                var TagList = TagJsonToListConvert(Tag.TagCode);
                var Templatename = TagList.FirstOrDefault(e => e.uniqid == data.TemplateuniqueId).secondname;

                var taginsidTemplatesViewModel = _mapper.Map<TaginsidTemplatesViewModel>(data);
                taginsidTemplatesViewModel.TemplateName = Templatename;
                taginsidTemplatesViewModel.TagNumber = Tag.TagNumber;
                return Json(taginsidTemplatesViewModel);

            }
            catch (Exception ex)
            {
                return Json("");

            }
        }



        /// <summary>
        /// Tag Report Card List
        /// </summary>
        /// <param name="SearchTag"></param>
        /// <returns>Tag Forms</returns>
        [HttpGet]
        [Route("FormTag/SearchTag")]
        public IActionResult TagReportCardList(string SearchTag)
        {
            try
            {
                var tag = _context.Tag.FirstOrDefault(e => e.TagNumber == SearchTag);

                if (tag == null)
                {

                    TempData["TagErrorMessage"] = "Tag not found";
                    return RedirectToAction("Index", "Home");

                }


                return View(tag);
            }
            catch
            {

                return View();
            }
        }

        [HttpGet]
        public ActionResult EditFormData(int TemplateUniqueId, string returnSearchTag)
        {
            try
            {

                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                bool UserHasPermission = false;
                var tagDetails = _context.TaginsideTemplates.FirstOrDefault(x => x.TemplateuniqueId == TemplateUniqueId);
                if (tagDetails != null)
                {
                    var TemplateDetails = _context.Template.FirstOrDefault(t => t.TemplateId == tagDetails.OrignalTemplateId);
                    if (TemplateDetails != null)
                    {

                        if (TemplateDetails.Groups == "0")
                        {

                            if (TemplateDetails.CreatedById == Currentuserid.ToString())// For Creator or Owner Access Allow
                            {

                                UserHasPermission = true;

                            }
                            else
                            {

                                var Users = (from u1 in _context.Users
                                             join u2 in _context.Users on u1.CompanyId equals u2.CompanyId
                                             where u1.Id == Convert.ToInt64(TemplateDetails.CreatedById)
                                             select u2).ToList();

                                var Workerfromsame = Users.FirstOrDefault(e => e.Id == Currentuserid);

                                if (Workerfromsame != null)
                                {
                                    UserHasPermission = true;
                                }
                                else
                                {
                                    UserHasPermission = false;
                                    //TempData["TemplateErrorType"] = "No Permission";

                                }

                            }


                        }

                        else
                        {//for private and for group 

                            if (TemplateDetails.CreatedById == Currentuserid.ToString())// For Creator or Owner Access Allow
                            {

                                UserHasPermission = true;

                            }
                            else
                            {
                                List<string> CommaSaperatedGroup = TemplateDetails.Groups.Split(',').ToList();

                                var MembersinGroup = _context.GroupMembers.Where(c => CommaSaperatedGroup.Contains(c.GroupId.ToString())).ToList();

                                foreach (var d in MembersinGroup)
                                {

                                    if (d.UserId == Currentuserid)
                                    {
                                        var permission = _context.GroupPermission.FirstOrDefault(e => e.GroupId == d.GroupId);
                                        if (permission.Read)
                                        {
                                            UserHasPermission = true;
                                        }

                                    }
                                }
                            }
                        }

                    }
                }

                if (UserHasPermission == true)
                {

                    var Templatefiels = _context.TemplateFields.Where(e => e.TemplateuniqueId == TemplateUniqueId).ToList();
                    var Tagcode = _context.Tag.FirstOrDefault(e => e.Id == tagDetails.TagId).TagCode;


                    //tag json to TagInsideTemplatesJson  type convert

                    List<TagInsideTemplatesJson> TagInsideTemplatesJson = new List<TagInsideTemplatesJson>();

                    TagInsideTemplatesJson = TagJsonToListConvert(Tagcode);


                    var templateFieldsView = _mapper.Map<List<TemplateFieldsViewModel>>(Templatefiels);

                    foreach (var d in templateFieldsView)
                    {

                        d.Files = _context.FilesManager.Where(e => e.FieldId == d.Id).ToList();

                    }

                    var templateFieldfromjson = JsonConvert.DeserializeObject<List<TemplateFieldsViewModel>>(tagDetails.TemplateCloneCode);

                    List<TemplateFieldsViewModel> ReturnFiledList = new List<TemplateFieldsViewModel>();


                    //short field list by json for manage listing or index of get fields 
                    foreach (var tField in templateFieldfromjson)
                    {
                        var listtoadd = templateFieldsView.FirstOrDefault(e => e.name == tField.name && e.type == tField.type);

                        if (listtoadd != null)
                        {
                            listtoadd.description = tField.description;
                            listtoadd.label = tField.label;
                            listtoadd.TemplateSecondName = TagInsideTemplatesJson.FirstOrDefault(e => e.uniqid == tagDetails.TemplateuniqueId).secondname;
                            ReturnFiledList.Add(listtoadd);
                        }

                    }
                    return View(ReturnFiledList);


                    //user has yep permission logic

                }


                else
                {
                    TempData["TemplateErrorMessage"] = "No Permission";
                    return RedirectToAction("TagSearchResult", new { SearchTag = returnSearchTag });
                    //user has nop permission logic

                }
            }

            catch (Exception ex)
            {


                return View();
            }


        }



        [HttpGet]
        public ActionResult ViewEditFormData(int TemplateUniqueId, string returnSearchTag, int DataEntryId)
        {
            try
            {
                CheckUserRightsHelper checkUserRightsHelper = new CheckUserRightsHelper(_context);

                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var TemplateCode = _context.TaginsideTemplates.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId);


                var userRights = checkUserRightsHelper.CheckUserAcceess(TemplateCode.OrignalTemplateId, Currentuserid);


                //if user has no permission for view 
                if (userRights.Read == false)
                {

                    TempData["TemplateErrorMessage"] = "No Permission";
                    return RedirectToAction("TagReportCardList", new { SearchTag = returnSearchTag });
                    //user has nop permission logic
                }


               
                var FormData = new TagFormDataEntry();

                //var TemplateType = (from taginsideTemplates in _context.TaginsideTemplates
                //                    join
                //                  template in _context.Template on taginsideTemplates.OrignalTemplateId equals template.TemplateId
                //                    where taginsideTemplates.TemplateuniqueId == TemplateUniqueId
                //                    select template).Select(e => e.TemplateTypeId).FirstOrDefault();


                //if (TemplateType == 1)
                //{
                //    FormData = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId && e.Id == DataEntryId);
                //}


                FormData = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId && e.Id == DataEntryId);



                ViewBag.TagNumber = returnSearchTag;
                ViewBag.FormUniqueId = TemplateUniqueId;
                ViewBag.DataEntryId = DataEntryId;
                var TemplateCloneCodeobj = JsonConvert.DeserializeObject<List<TemplateFieldsViewModel>>(TemplateCode.TemplateCloneCode);


                var FormDataObj = JsonConvert.DeserializeObject<List<TemplateDataFields>>(FormData.FormJsonData);

                foreach (var e in TemplateCloneCodeobj)
                {
                    if (e.values != null)
                    {
                        e.values.ForEach(a => a.selected = false);
                        var Formdata = FormDataObj.FirstOrDefault(x => x.name == e.name);
                        var ValueList = new List<String>();
                        if (Formdata != null)
                        {
                            ValueList = Formdata.value.Split(',').ToList();
                        }

                        foreach (var list in ValueList)
                        {
                            var val = e.values.FirstOrDefault(x => x.value == list);
                            if (val != null)
                            {
                                val.selected = true;
                                //e.value = e.value + "," + val.label;
                                //e.value = string.Join(",", e.value, val.label);
                                e.value = e.value == null ? val.label : e.value + ", " + val.label;
                            }
                        }

                    }
                    else
                    {

                        var Formdata = FormDataObj.FirstOrDefault(x => x.name == e.name);
                        if (Formdata != null)
                        {
                            e.value = Formdata.value;
                            e.Files = _context.FilesManager.Where(t => t.FieldName == Formdata.name && t.TemplateuniqueId == TemplateUniqueId && t.FormdataEntryId == FormData.Id).ToList();
                        }
                    }


                }

                var tagcode = _context.Tag.FirstOrDefault(t => t.TagNumber == returnSearchTag).TagCode;
                var TagFormData = TagJsonToListConvert(tagcode);
                var temptype = _context.Template.FirstOrDefault(e => e.TemplateId == TemplateCode.OrignalTemplateId).TemplateTypeId;
                var formname = TagFormData.FirstOrDefault(x => x.uniqid == TemplateUniqueId).secondname;
                //update file list with ids
                TemplateCloneCodeobj.ForEach(a =>
                {
                    a.FormName = formname;
                    a.TemplateType = temptype;
                    a.WritePermission = userRights.Write;
                });




                return View(TemplateCloneCodeobj);




            }

            catch (Exception ex)
            {


                return View();
            }


        }



        //get First Public ReportCard for search result view with data
        public JsonResult GetSingleInputForReportCard(ViewEditDataViewModel data)
        {




            try
            {

                int TemplateUniqueId = data.FormUniquId;
                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);


                var TemplateCode = _context.TaginsideTemplates.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId);

                var FormData = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId && e.Id == data.Id);







                //get full json from clone template

                var FormDataobj = JsonConvert.DeserializeObject<List<ViewEditDataViewModel>>(FormData.FormJsonData);
                var templateFieldobj = JsonConvert.DeserializeObject<List<TemplateFieldsViewModel>>(TemplateCode.TemplateCloneCode);

                var clonecodesingelfield = templateFieldobj.FirstOrDefault(e => e.name == data.name);
                var formdatasinglefield = FormDataobj.FirstOrDefault(e => e.name == data.name);



                if (clonecodesingelfield.values != null)
                {
                    //by auto select data
                    if (formdatasinglefield != null)
                    {
                        if (formdatasinglefield.value != "")
                        {


                            var ValueList = formdatasinglefield.value.Split(',').ToList();


                            if (ValueList != null)
                            {

                                clonecodesingelfield.values.ForEach(a => a.selected = false);

                                foreach (var v in ValueList)
                                {

                                    var listdata = clonecodesingelfield.values.FirstOrDefault(e => e.value == v);
                                    listdata.selected = true;
                                }
                            }

                        }
                    }



                }
                else
                {
                    if (formdatasinglefield != null)
                    {
                        clonecodesingelfield.value = formdatasinglefield.value;
                        clonecodesingelfield.Files = _context.FilesManager.Where(e => e.FieldName == formdatasinglefield.name && e.FormdataEntryId == FormData.Id).ToList();
                    }
                    else
                    {
                        clonecodesingelfield.value = "";
                        clonecodesingelfield.Files = new List<FilesManager>();
                    }
                }

                clonecodesingelfield.TemplateuniqueId = TemplateUniqueId;


                return Json(new { Data = clonecodesingelfield, Permission = true });




            }

            catch
            {


                return Json(new { Data = "Error", Permission = false });
            }

        }


        //get First Public ReportCard for search result view with data
        public JsonResult GetSingleInput(TemplateFields data)
        {




            try
            {

                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                bool UserHasPermission = false;
                var input = _context.TemplateFields.FirstOrDefault(e => e.Id == data.Id && e.name == data.name);

                var tagDetails = _context.TaginsideTemplates.FirstOrDefault(x => x.TemplateuniqueId == input.TemplateuniqueId);
                if (tagDetails != null)
                {
                    var TemplateDetails = _context.Template.FirstOrDefault(t => t.TemplateId == tagDetails.OrignalTemplateId);
                    if (TemplateDetails != null)
                    {

                        if (TemplateDetails.Groups == "0")
                        {

                            if (TemplateDetails.CreatedById == Currentuserid.ToString())// For Creator or Owner Access Allow
                            {

                                UserHasPermission = true;

                            }
                            else
                            {

                                var Users = (from u1 in _context.Users
                                             join u2 in _context.Users on u1.CompanyId equals u2.CompanyId
                                             where u1.Id == Convert.ToInt64(TemplateDetails.CreatedById)
                                             select u2).ToList();

                                var Workerfromsame = Users.FirstOrDefault(e => e.Id == Currentuserid);

                                if (Workerfromsame != null)
                                {
                                    UserHasPermission = true;
                                }
                                else
                                {
                                    UserHasPermission = false;
                                    //TempData["TemplateErrorType"] = "No Permission";

                                }

                            }


                        }

                        else
                        {//for private and for group 

                            if (TemplateDetails.CreatedById == Currentuserid.ToString())// For Creator or Owner Access Allow
                            {

                                UserHasPermission = true;

                            }
                            else
                            {
                                List<string> CommaSaperatedGroup = TemplateDetails.Groups.Split(',').ToList();

                                var MembersinGroup = _context.GroupMembers.Where(c => CommaSaperatedGroup.Contains(c.GroupId.ToString())).ToList();

                                foreach (var d in MembersinGroup)
                                {

                                    if (d.UserId == Currentuserid)
                                    {
                                        var permission = _context.GroupPermission.FirstOrDefault(e => e.GroupId == d.GroupId);
                                        if (permission.Read && permission.Write)
                                        {
                                            UserHasPermission = true;
                                        }

                                    }
                                }
                            }
                        }

                    }
                }

                if (UserHasPermission == true)
                {

                    var inputfilddata = _mapper.Map<TemplateFieldsViewModel>(input);

                    //get full json from clone template

                    var templateFieldobj = JsonConvert.DeserializeObject<List<TemplateFieldsViewModel>>(tagDetails.TemplateCloneCode);

                    inputfilddata.label = templateFieldobj.First(e => e.name == inputfilddata.name).label;
                    inputfilddata.values = templateFieldobj.FirstOrDefault(e => e.name == inputfilddata.name).values;
                    inputfilddata.placeholder = templateFieldobj.FirstOrDefault(e => e.name == inputfilddata.name).placeholder;
                    inputfilddata.Files = new List<FilesManager>();
                    inputfilddata.multiple = templateFieldobj.FirstOrDefault(e => e.name == inputfilddata.name).multiple;
                    inputfilddata.maxlength = templateFieldobj.FirstOrDefault(e => e.name == inputfilddata.name).maxlength;
                    inputfilddata.Files = _context.FilesManager.Where(e => e.FieldId == input.Id).ToList();



                    return Json(new { Data = inputfilddata, Permission = true });
                }


                else
                {
                    //TempData["TemplateErrorMessage"] = "No Permission";
                    //return RedirectToAction("TagSearchResult", new { SearchTag = returnSearchTag });
                    //user has nop permission logic

                    return Json(new { Data = "", Permission = false });
                }
            }

            catch
            {


                return Json(new { Data = "Error", Permission = false });
            }

        }
        public JsonResult GetSingleFormData(int TemplateUniqueId)
        {
            try
            {

                var taginsideTemplates = _context.TaginsideTemplates.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId);
                var FormData = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateUniqueId);
                var TemplateCloneCodeobj = JsonConvert.DeserializeObject<List<TemplateFieldsViewModel>>(taginsideTemplates.TemplateCloneCode);
                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                bool WritePermission = true;

                CheckUserRightsHelper checkUserRightsHelper = new CheckUserRightsHelper(_context);
                var userRights = checkUserRightsHelper.CheckUserAcceess(taginsideTemplates.OrignalTemplateId, Currentuserid);
                

               
                //if user has no permission for view 
                if (userRights.Write == false)
                {
                    WritePermission = false;
                }


                if (FormData == null && taginsideTemplates != null)
                {


                    return Json(new { Returndata = TemplateCloneCodeobj,Permission = WritePermission });

                }


                var FormDataObj = JsonConvert.DeserializeObject<List<TemplateDataFields>>(FormData.FormJsonData);


                foreach (var e in TemplateCloneCodeobj)
                {
                    if (e.values != null)
                    {
                        e.values.ForEach(a => a.selected = false);
                        var Formdata = FormDataObj.FirstOrDefault(x => x.name == e.name);
                        var ValueList = new List<String>();
                        if (Formdata != null)
                        {
                            ValueList = Formdata.value.Split(',').ToList();
                        }

                        foreach (var list in ValueList)
                        {
                            var val = e.values.FirstOrDefault(x => x.value == list);
                            if (val != null)
                            {
                                val.selected = true;
                                //e.value = e.value + "," + val.label;
                                //e.value = string.Join(",", e.value, val.label);
                                e.value = e.value == null ? val.label : e.value + ", " + val.label;
                            }
                        }

                    }
                    else
                    {
                        var Formdata = FormDataObj.FirstOrDefault(x => x.name == e.name);
                        if (Formdata != null)
                        {
                            e.value = Formdata.value;
                            e.Files = _context.FilesManager.Where(t => t.FieldName == Formdata.name && t.TemplateuniqueId == TemplateUniqueId && t.FormdataEntryId == FormData.Id).ToList();
                        }
                    }
                }

                return Json(new { Returndata = TemplateCloneCodeobj, Permission = WritePermission });

            }
            catch (Exception ex)
            {
                return Json("Fail");

            }
        }

        //get search tag data by tagid
        [HttpPost]
        public JsonResult GetSearchTagFormData(Tag model)
        {
            var tag = _context.Tag.FirstOrDefault(e => e.Id == model.Id);

            if (tag == null)
            {

                return Json("Fail");

            }

            return Json(tag);

        }



        /// <summary>
        /// Get all tag list
        /// </summary>
        [HttpGet]
        public string GetAllTags()
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(r => r.Id == userid);


                //List<GroupsListViewModel> groups = new List<GroupsListViewModel>();
                var list = _context.Tag.Where(t => t.CompanyId == user.CompanyId).OrderByDescending(e => e.Id).ToList();
                var json = JsonConvert.SerializeObject(list);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Get all report card  list
        /// </summary>
        [HttpGet]
        public string GetAllPublicReportCards()
        {
            try
            {
                List<Template> RepotcardFinalData = new List<Template>();

                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user =  _context.Users.FirstOrDefault(e => e.Id == userid);
                List<Template> ReportcardList = _context.Template.Where(Templates => Templates.CompanyId == user.CompanyId && Templates.TemplateTypeId == 2 && Templates.IsActive == true && Templates.IsDeleted == false && Templates.Groups == "0").ToList();
                foreach (var Data in ReportcardList)
                {
                    if (User.IsInRole("Admin"))
                    {
                        RepotcardFinalData.Add(Data);
                    }
                    else
                    {
                        List<int> groupids = Data.Groups.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();
                        if (groupids.Contains(0))
                        {
                            RepotcardFinalData.Add(Data);
                        }
                    }
                }

                var json = JsonConvert.SerializeObject(RepotcardFinalData);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }

        }


        /// <summary>
        /// Get all report card  list
        /// </summary>
        [HttpGet]
        public string GetAllReportCards()
        {
            try
            {
                List<Template> RepotcardFinalData = new List<Template>();

                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(e => e.Id == userid);
                List<Template> ReportcardList = _context.Template.Where(Templates => Templates.CompanyId == user.CompanyId && Templates.TemplateTypeId == 2 && Templates.IsActive == true && Templates.IsDeleted == false).ToList();

                foreach (var Data in ReportcardList)
                {


                    if (User.IsInRole("Admin"))
                    {
                        RepotcardFinalData.Add(Data);
                    }
                    else
                    {
                        List<int> groupids = Data.Groups.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();
                        if (groupids.Contains(0))
                        {
                            RepotcardFinalData.Add(Data);
                        }
                    }
                }
                var json = JsonConvert.SerializeObject(RepotcardFinalData);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }

        }



        [HttpGet]
        public string GetAllRegulerForm()
        {
            try
            {
                List<Template> RepotcardFinalData = new List<Template>();

                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(e => e.Id == userid);

                List<Template> RegularFormList = _context.Template.Where(Templates => Templates.CompanyId == user.CompanyId && Templates.TemplateTypeId == 1 && Templates.IsActive == true && Templates.IsDeleted == false).ToList();

                //foreach (var Data in RegularFormList)
                //{


                //    if (User.IsInRole("Admin"))
                //    {
                //        RepotcardFinalData.Add(Data);
                //    }
                //    else
                //    {
                //        List<int> groupids = Data.Groups.Split(',').Select(x => x.Trim()).Select(x => Int32.Parse(x)).ToList();
                //        if (groupids.Contains(0))
                //        {
                //            RepotcardFinalData.Add(Data);
                //        }
                //    }
                //}
                var json = JsonConvert.SerializeObject(RegularFormList);
                return json;
            }
            catch (Exception ex)
            {
                return null;
            }

        }



        ///<summary>
        ///Edit Form tag
        ///</summary>
        [HttpGet]
        [Route("FormTag/EditFormtag/{id}")]
        [Authorize(Roles = "Admin,Worker")]
        public IActionResult EditFormtag(int id)
        {
            try
            {

                var tagnumber = _context.Tag.FirstOrDefault(tag => tag.Id == id);
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = _context.Users.FirstOrDefault(e => e.Id == userid);
                if (tagnumber == null)
                {
                    return RedirectToAction("InvalidAccess", "Account");

                }

                if (user.CompanyId != tagnumber.CompanyId)
                {
                    return RedirectToAction("InvalidAccess", "Account");

                }
                return View("ManageTag", tagnumber);

            }
            catch (Exception)
            {
                return View();
            }
        }

        ///<summary>
        ///Add Report Card
        ///</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Worker")]
        [Route("FormTag/AddReportCard")]
        public IActionResult AddReportCard()
        {
            try
            {

                return View();

            }
            catch (Exception)
            {
                return View();
            }
        }


        /// <summary>
        /// Reguler Form Entries
        /// </summary>
        /// <param name="formUniqueId"></param>
        /// <param name="returnSearchTag"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("FormTag/RegulerFormEntries")]
        public IActionResult RegulerFormEntries(int formUniqueId, string returnSearchTag)
        {
            try
            {
                var formEntry = _context.TagFormDataEntry.Where(e => e.TemplateuniqueId == formUniqueId).ToList();
                List<TagFormDataEntryViewModel> templateDataFieldsViewModel = new List<TagFormDataEntryViewModel>();


                var FormCode = _context.Tag.FirstOrDefault(e => e.TagNumber == returnSearchTag);
                var FormsData = TagJsonToListConvert(FormCode.TagCode);
                string formname = FormsData.FirstOrDefault(x => x.uniqid == formUniqueId).secondname;

                int OrignalTemplateId = _context.TaginsideTemplates.FirstOrDefault(e => e.TemplateuniqueId == formUniqueId).OrignalTemplateId;
                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                CheckUserRightsHelper checkUserRightsHelper = new CheckUserRightsHelper(_context);

                var userRights = checkUserRightsHelper.CheckUserAcceess(OrignalTemplateId, Currentuserid, formUniqueId);

                //if user has no permission for view 
                if (userRights.Read == false)
                {

                   
                    return RedirectToAction("TagReportCardList", new { SearchTag = returnSearchTag });
                    //user has nop permission logic
                }




                if (formEntry.Count == 0)//if for first time no entry
                {
                    TagFormDataEntryViewModel templateDataFields = new TagFormDataEntryViewModel();
                    templateDataFields.TemplateuniqueId = formUniqueId;
                    templateDataFields.TagNumber = returnSearchTag;
                    templateDataFields.FormName = formname;
                    templateDataFields.WritePermission = userRights.Write;
                    templateDataFieldsViewModel.Add(templateDataFields);
                }

                foreach (var e in formEntry)
                {
                    TagFormDataEntryViewModel templateDataFields = new TagFormDataEntryViewModel();
                    templateDataFields.CreatedDate = e.CreatedDate.ToString("MM/dd/yyyy");
                    templateDataFields.CreatedBy = e.CreatedBy;
                    templateDataFields.FormJsonData = e.FormJsonData;
                    templateDataFields.Id = e.Id;
                    templateDataFields.IsActive = e.IsActive;
                    templateDataFields.LastModifyBy = e.LastModifyBy;
                    templateDataFields.OrignalTemplateId = e.OrignalTemplateId;
                    templateDataFields.TagId = e.TagId;
                    templateDataFields.TemplateuniqueId = e.TemplateuniqueId;
                    templateDataFields.FormName = formname;
                    var ViewEditDataViewModel = JsonConvert.DeserializeObject<List<TemplateDataFields>>(e.FormJsonData);
                    //templateDataFields.Title = ViewEditDataViewModel.FirstOrDefault(x => x.type == "title").value;
                    //templateDataFields.TagNumber = _context.Tag.FirstOrDefault(y => y.Id == e.TagId).TagNumber;
                    templateDataFieldsViewModel.Add(templateDataFields);
                }


                templateDataFieldsViewModel.ForEach(a =>
                {
                    a.TagNumber = _context.Tag.FirstOrDefault(y => y.Id == FormCode.Id).TagNumber;
                    a.WritePermission = userRights.Write;
                });
                //templateDataFieldsViewModel.CreatedDate = formEntry.


                return View("RegulerFormEntries", templateDataFieldsViewModel);

            }




            catch (Exception)
            {
                return View();
            }
        }



        /// <summary>
        /// Reguler Form Entries
        /// </summary>
        /// <param name="formUniqueId"></param>
        /// <param name="returnSearchTag"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("FormTag/HasPermission")]
        public JsonResult HasPermission(string TagId ,int TagUnqId)
        {
            try
            {
                bool WritePermission = true;
                int Currentuserid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var taginsideTemplates = _context.TaginsideTemplates.FirstOrDefault(e => e.TemplateuniqueId == TagUnqId);

                CheckUserRightsHelper checkUserRightsHelper = new CheckUserRightsHelper(_context);
                var userRights = checkUserRightsHelper.CheckUserAcceess(taginsideTemplates.OrignalTemplateId, Currentuserid, TagUnqId);



                //if user has no permission for view 
                if (userRights.Read == false)
                {
                    WritePermission = false;
                }


                return Json(new { WritePermission = WritePermission });


            }




            catch (Exception)
            {
                return Json(new { WritePermission = false });
            }
        }

        /// <summary>
        /// Create Form Tag
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Worker")]
        [Route("FormTag/CreateFormTag")]
        public IActionResult CreateFormTag()
        {
            try
            {

                return View("ManageTag");

            }
            catch (Exception)
            {
                return View();
            }
        }


        /// <summary>
        /// Delete Tag
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTag(int id)
        {
            try
            {
                _context.Tag.Remove(_context.Tag.FirstOrDefault(t => t.Id == id));
                _context.SaveChanges();
                return new JsonResult(new { value = true });
            }
            catch (Exception)
            {
                return new JsonResult(new { value = Constants.ExMsg });
            }

        }


        /// <summary>
        /// Create Edit TagForm
        /// </summary>
        /// <param name="Tagformdata"></param>
        /// <returns></returns>
        public JsonResult CreateEditTagForm(Tag Tagformdata)
        {
            try
            {
                int userid = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
               var user = _context.Users.FirstOrDefault(u => u.Id == userid);
                //create tag if not found
                var Tagdata = _context.Tag.FirstOrDefault(e => e.Id == Tagformdata.Id);
                if (Tagdata == null)
                {
                    //Create First Tag with Empty TagCode
                    Tagdata = new Tag();
                    Tagdata.CreatedById = userid.ToString();
                    Tagdata.CreatedDate = DateTime.Now;
                    Tagdata.TagNumber = Tagformdata.TagNumber;
                    Tagdata.CompanyId = (int)user.CompanyId;
                    _context.Tag.Add(Tagdata);
                    _context.SaveChanges();
                    Tagformdata.Id = Tagdata.Id;
                }

                //Insert Tag Code Into tag
                List<TaginsideTemplates> taginsides = new List<TaginsideTemplates>();
                Tagdata.TagCode = Tagformdata.TagCode;
                _context.SaveChanges();

                TaginsideTemplates TIT = new TaginsideTemplates();
                TIT.CreatedDate = DateTime.Now;
                TIT.IsActive = true;


                var data = JsonConvert.DeserializeObject<List<object>>(Tagformdata.TagCode);//remove extra bracket from json and convert to array 
                var MainList = JsonConvert.DeserializeObject<List<object>>(data[0].ToString());//second orignal list fatch from json


                foreach (var dt in MainList)
                {


                    var parentsNode = JsonConvert.DeserializeObject<Dictionary<string, object>>(dt.ToString()); //p
                    if (parentsNode["type"].ToString() == "Group")
                    {

                        var GroupChildNode = JsonConvert.DeserializeObject<List<object>>(parentsNode["children"].ToString());
                        var GroupChildClass = JsonConvert.DeserializeObject<List<TagInsideTemplatesJson>>(GroupChildNode[0].ToString());

                        foreach (var dtchild in GroupChildClass)
                        {
                            TaginsideTemplates taginside = new TaginsideTemplates();
                            taginside.IsActive = true;
                            taginside.TagId = Tagformdata.Id;
                            taginside.IsFilled = false;
                            taginside.TemplateuniqueId = dtchild.uniqid;
                            taginside.OrignalTemplateId = dtchild.id;
                            taginside.GroupId = Convert.ToInt32(parentsNode["uniqid"]);
                            taginside.GroupName = parentsNode["name"].ToString();
                            var temp = _context.Template.FirstOrDefault(e => e.TemplateId == taginside.OrignalTemplateId);
                            taginside.TemplateCloneCode = temp.TemplateCode;
                            taginsides.Add(taginside);
                        }
                    }

                    else
                    {

                        TaginsideTemplates taginside = new TaginsideTemplates();
                        taginside.IsActive = true;
                        taginside.TagId = Tagformdata.Id;
                        taginside.IsFilled = false;
                        taginside.TemplateuniqueId = Convert.ToInt32(parentsNode["uniqid"]);
                        taginside.OrignalTemplateId = Convert.ToInt32(parentsNode["id"]);
                        var temp = _context.Template.FirstOrDefault(e => e.TemplateId == taginside.OrignalTemplateId);
                        taginside.TemplateCloneCode = temp.TemplateCode;
                        taginsides.Add(taginside);
                    }

                }

                //get all list from Database and match for new one 

                var TagListDb = _context.TaginsideTemplates.Where(e => e.TagId == Tagformdata.Id).ToList();

                List<TaginsideTemplates> UpldateList = new List<TaginsideTemplates>();
                List<TaginsideTemplates> SubmitList = new List<TaginsideTemplates>();
                SubmitList.AddRange(taginsides);

                foreach (var datalist in taginsides)
                {
                    var isexist = TagListDb.FirstOrDefault(e => e.TemplateuniqueId == datalist.TemplateuniqueId);

                    if (isexist != null)
                    {
                        UpldateList.Add(datalist);
                        SubmitList.Remove(datalist);
                    }


                }

                _context.TaginsideTemplates.AddRange(SubmitList);
                _context.SaveChanges();

                //Add Row in template_Field_table 

                var tempalelist = _context.TaginsideTemplates.Where(e => e.TagId == Tagformdata.Id).ToList();

                List<TemplateFields> templateFields = new List<TemplateFields>();
                foreach (var e in tempalelist)
                {
                    var TemplateCode = e.TemplateCloneCode;
                    var templateField = JsonConvert.DeserializeObject<List<TemplateFieldsViewModel>>(TemplateCode);


                    foreach (var tField in templateField)
                    {

                        if (_context.TemplateFields.Count(x => x.name == tField.name && x.TagId == Tagformdata.Id && x.TemplateuniqueId == e.TemplateuniqueId) == 0)
                        {
                            var output = _mapper.Map<TemplateFields>(tField);
                            output.TemplateuniqueId = e.TemplateuniqueId;
                            output.LatestValue = "";
                            output.TemplateId = e.OrignalTemplateId;
                            output.TagId = e.TagId;
                            output.IsActive = true;
                            templateFields.Add(output);
                        }

                    }

                }
                _context.TemplateFields.AddRange(templateFields);
                _context.SaveChanges();



                return Json(new { Data = Tagdata.TagNumber, ReturnType = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { Data = "", ReturnType = "Fail" });
            }
        }
        //get template by id for view or edit.
        public JsonResult GetTagForm(Tag Tagformdata)
        {
            try
            {

                var tagdata = _context.Tag.FirstOrDefault(e => e.Id == Tagformdata.Id);

                return Json(tagdata);

            }
            catch (Exception)
            {
                return Json("Fail");

            }
        }


        /// <summary>
        /// Submit New Forms
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        [Authorize(Roles = "Admin,Worker")]
        public async Task<IActionResult> SubmitNewForms()
        {
            try
            {




                var dictData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

                List<FilesManager> filesManagers = new List<FilesManager>();
                List<TemplateDataFields> templateFieldsList = new List<TemplateDataFields>();
                TagFormDataEntry tagFormDataEntry = new TagFormDataEntry();

                foreach (var Fielddict in dictData.Where(e => e.Key != "HiddenData" && e.Key != "__RequestVerificationToken"))
                {
                    TemplateDataFields templateFields = new TemplateDataFields();
                    templateFields.name = Fielddict.Key;
                    templateFields.value = Fielddict.Value;
                    templateFields.type = Fielddict.Key.Split('-')[0];
                    templateFieldsList.Add(templateFields);
                }




                //Get Hidden Field data
                string identificationdata = dictData.FirstOrDefault(e => e.Key == "HiddenData").Value;
                var identificationdataModel = JsonConvert.DeserializeObject<TagFormDataEntry>(identificationdata);
                tagFormDataEntry.TagId = identificationdataModel.TagId;
                tagFormDataEntry.TemplateuniqueId = identificationdataModel.TemplateuniqueId;
                tagFormDataEntry.OrignalTemplateId = identificationdataModel.OrignalTemplateId;
                tagFormDataEntry.IsActive = true;
                tagFormDataEntry.CreatedBy = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                tagFormDataEntry.CreatedDate = DateTime.Now;


                //File Upload IF exist File

                string TagNumber = _context.Tag.FirstOrDefault(e => e.Id == Convert.ToInt32(identificationdataModel.TagId)).TagNumber;
                var files = HttpContext.Request.Form.Files;

                if (files.Count != 0)
                {

                    var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

                    //get dynamic data from field

                    string fileNameData = "";
                    string FieldName = "";


                    foreach (var singlefile in files)
                    {
                        if (singlefile != null && singlefile.Length > 0)
                        {
                            var file = singlefile;
                            //There is an error here
                            var uploads = Path.Combine(_env.WebRootPath, "Uploads\\FormData");
                            if (file.Length > 0)
                            {
                                var guid = Guid.NewGuid();
                                var newname = guid + "@" + file.FileName;
                                using (var fileStream = new FileStream(Path.Combine(uploads, newname), FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                    fileNameData = fileNameData + "," + newname;
                                    FieldName = singlefile.Name;

                                    //add filemanager table file data
                                    FilesManager filesdata = new FilesManager();
                                    filesdata.FieldName = FieldName;
                                    filesdata.Name = newname;
                                    filesdata.FileType = singlefile.ContentType;
                                    filesManagers.Add(filesdata);

                                }

                            }
                        }
                    }





                    foreach (var filedata in filesManagers)
                    {
                        TemplateDataFields templateFieldsfile = new TemplateDataFields();
                        templateFieldsfile.name = filedata.FieldName;
                        templateFieldsfile.value = "Files";
                        templateFieldsfile.type = filedata.Name.Split('-')[0];
                        templateFieldsList.Add(templateFieldsfile);

                    }


                    ////return back url
                    //returnurltag = _context.Tag.FirstOrDefault(r => r.Id == templateFields.TagId).TagNumber;
                    //uniqtempid = templateFields.TemplateuniqueId;


                }



                //form data to json convert
                string FormDataJson = JsonConvert.SerializeObject(templateFieldsList);
                tagFormDataEntry.FormJsonData = FormDataJson;

                _context.TagFormDataEntry.AddRange(tagFormDataEntry);
                _context.SaveChanges();


                //update file list with ids
                filesManagers.ForEach(a =>
                {
                    a.TemplateuniqueId = tagFormDataEntry.TemplateuniqueId;
                    a.TagId = tagFormDataEntry.TagId;
                    a.FormdataEntryId = tagFormDataEntry.Id;
                });


                _context.FilesManager.AddRange(filesManagers);
                _context.SaveChanges();

                ////return back url
                //returnurltag = _context.Tag.FirstOrDefault(r => r.Id == currentfield.TagId).TagNumber;
                //uniqtempid = currentfield.TemplateuniqueId;




                return RedirectToAction("ViewEditFormData", new { TemplateUniqueId = tagFormDataEntry.TemplateuniqueId, returnSearchTag = TagNumber, DataEntryId = tagFormDataEntry.Id });



            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        /// <summary>
        /// Set Single Input For Report Card Edit
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        [Authorize(Roles = "Admin,Worker")]
        public async Task<IActionResult> SetSingleInputForReportCardEdit()
        {
            try
            {

                var files = HttpContext.Request.Form.Files;

                if (files.Count != 0)
                {
                    List<FilesManager> filesManagers = new List<FilesManager>();
                    var objFormCollection = HttpContext.Request.ReadFormAsync();
                    var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                    int DataId = Convert.ToInt32(dict.FirstOrDefault(e => e.Key.Contains("HiddenDataId")).Value);
                    int TemplateuniqueId = Convert.ToInt32(dict.FirstOrDefault(e => e.Key.Contains("HiddenData")).Value);
                    var currentFormDataCode = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateuniqueId && e.Id == DataId);
                    var GetTagId = _context.Tag.FirstOrDefault(e => e.Id == currentFormDataCode.TagId).TagNumber;
                    string FieldNameByField = dict.FirstOrDefault(e => e.Key.Contains("FieldName")).Value;

                    //get dynamic data from field
                    int fieldid = Convert.ToInt32(dict.ElementAt(0).Value);
                    string fileNameData = "";
                    string FieldName = "";


                   

                    var currentLetstFormCodeFields = JsonConvert.DeserializeObject<List<TemplateDataFields>>(currentFormDataCode.FormJsonData);


                    /// if new field add after form submit and no field found in data filds json so this logic create field in json

                    if (currentLetstFormCodeFields.FirstOrDefault(e => e.name == FieldNameByField) == null)
                    {
                        TemplateDataFields templateDataFields = new TemplateDataFields();
                        templateDataFields.name = FieldNameByField.ToString();
                        templateDataFields.type = FieldNameByField.Split('-')[0];

                        currentLetstFormCodeFields.Add(templateDataFields);
                        //refind current field

                        var updatedcode = JsonConvert.SerializeObject(currentLetstFormCodeFields);

                        currentFormDataCode.FormJsonData = updatedcode;

                        _context.SaveChanges();

                    }
                    


                    foreach (var singlefile in files)
                    {
                        if (singlefile != null && singlefile.Length > 0)
                        {
                            var file = singlefile;
                            //There is an error here
                            var uploads = Path.Combine(_env.WebRootPath, "Uploads\\FormData");
                            if (file.Length > 0)
                            {
                                var guid = Guid.NewGuid();
                                var newname = guid + "@" + file.FileName;
                                using (var fileStream = new FileStream(Path.Combine(uploads, newname), FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                    fileNameData = fileNameData + "," + newname;
                                    FieldName = singlefile.Name;

                                    //add filemanager table file data
                                    FilesManager filesdata = new FilesManager();

                                    filesdata.Name = newname;
                                    filesdata.FileType = singlefile.ContentType;
                                    filesManagers.Add(filesdata);

                                }

                            }
                        }
                    }


                    filesManagers.ForEach(a =>
                    {
                        a.FieldName = FieldName;
                        a.TemplateuniqueId = TemplateuniqueId;
                        a.TagId = currentFormDataCode.TagId;
                        a.FormdataEntryId = DataId;
                    });





                    _context.FilesManager.AddRange(filesManagers);
                    _context.SaveChanges();

                    //return back url



                    return RedirectToAction("ViewEditFormData", new { TemplateUniqueId = TemplateuniqueId, returnSearchTag = GetTagId, DataEntryId = DataId });

                }

                else
                {


                    var objFormCollection = HttpContext.Request.ReadFormAsync();
                    var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());


                    int TemplateuniqueId = Convert.ToInt32(dict.FirstOrDefault(e => e.Key.Contains("HiddenDataTemplateUniqid")).Value);
                    int DataId = Convert.ToInt32(dict.FirstOrDefault(e => e.Key.Contains("HiddenDataId")).Value);
                    string FieldName = dict.FirstOrDefault(e => e.Key.Contains("FieldName")).Value;
                    var currentFormDataCode = _context.TagFormDataEntry.FirstOrDefault(e => e.TemplateuniqueId == TemplateuniqueId && e.Id == DataId);
                    var GetTagId = _context.Tag.FirstOrDefault(e => e.Id == currentFormDataCode.TagId).TagNumber;


                    var currentLetstFormCodeFields = JsonConvert.DeserializeObject<List<TemplateDataFields>>(currentFormDataCode.FormJsonData);


                    /// if new field add after form submit and no field found in data filds json so this logic create field in json

                    if (currentLetstFormCodeFields.FirstOrDefault(e=>e.name == FieldName) == null)
                    {
                        TemplateDataFields templateDataFields = new TemplateDataFields();
                        templateDataFields.name = FieldName.ToString();
                        templateDataFields.type = FieldName.Split('-')[0];

                        currentLetstFormCodeFields.Add(templateDataFields);
                        //refind current field
                      

                    }


                    var currentfields = currentLetstFormCodeFields.FirstOrDefault(e => e.name == FieldName.ToString());
                    

                    //if currentfield is null so value is empty or not selected so json update with empty value
                    if (dict.Keys.ElementAtOrDefault(0) != FieldName)
                    {
                        currentfields.value = "";

                    }
                    else
                    {
                        currentfields.value = dict.Values.ElementAtOrDefault(0);
                    }
                    var updatedcode = JsonConvert.SerializeObject(currentLetstFormCodeFields);

                    currentFormDataCode.FormJsonData = updatedcode;

                    _context.SaveChanges();

                    return RedirectToAction("ViewEditFormData", new { TemplateUniqueId = TemplateuniqueId, returnSearchTag = GetTagId, DataEntryId = DataId });

                    //currentfield.value == 


                    //if (Fieldavalibity.Key == null && Fieldavalibity.Value == null)
                    //{
                    //    currentfield.LatestValue = "";
                    //    _context.SaveChanges();
                    //}

                    //else
                    //{

                    //    ////get dynamic data from field
                    //    //string FieldName = Fieldavalibity.Key;
                    //    //string FieldData = Fieldavalibity.Value;


                    //    ////var templateFields = _context.TemplateFields.FirstOrDefault(e => e.Id == fieldid && e.name == FieldName);

                    //    //currentfield.LatestValue = FieldData;
                    //    _context.SaveChanges();
                    //}
                    ////return back url
                    //returnurltag = _context.Tag.FirstOrDefault(r => r.Id == currentfield.TagId).TagNumber;
                    //uniqtempid = currentfield.TemplateuniqueId;



                }

            }
            catch (Exception ex)
            {
                return View();
            }
        }



        /// <summary>
        /// Set Single Input
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        [Authorize(Roles = "Admin,Worker")]
        public async Task<IActionResult> SetSingleInput()
        {
            try
            {

                var returnurltag = "";
                var uniqtempid = 0;
                var files = HttpContext.Request.Form.Files;

                if (files.Count != 0)
                {
                    List<FilesManager> filesManagers = new List<FilesManager>();
                    var objFormCollection = HttpContext.Request.ReadFormAsync();
                    var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

                    //get dynamic data from field
                    int fieldid = Convert.ToInt32(dict.ElementAt(0).Value);
                    string fileNameData = "";
                    string FieldName = "";


                    foreach (var singlefile in files)
                    {
                        if (singlefile != null && singlefile.Length > 0)
                        {
                            var file = singlefile;
                            //There is an error here
                            var uploads = Path.Combine(_env.WebRootPath, "Uploads\\FormData");
                            if (file.Length > 0)
                            {
                                var guid = Guid.NewGuid();
                                var newname = guid + "@" + file.FileName;
                                using (var fileStream = new FileStream(Path.Combine(uploads, newname), FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                    fileNameData = fileNameData + "," + newname;
                                    FieldName = singlefile.Name;

                                    //add filemanager table file data
                                    FilesManager filesdata = new FilesManager();

                                    filesdata.Name = newname;
                                    filesdata.FileType = singlefile.ContentType;
                                    filesManagers.Add(filesdata);

                                }

                            }
                        }
                    }

                    var templateFields = _context.TemplateFields.FirstOrDefault(e => e.Id == fieldid && e.name == FieldName);

                    templateFields.LatestValue = fileNameData;

                    filesManagers.ForEach(a =>
                    {
                        a.FieldId = templateFields.Id;
                        a.TemplateuniqueId = templateFields.TemplateuniqueId;
                        a.TagId = templateFields.TagId;
                    });


                    _context.FilesManager.AddRange(filesManagers);
                    _context.SaveChanges();

                    //return back url
                    returnurltag = _context.Tag.FirstOrDefault(r => r.Id == templateFields.TagId).TagNumber;
                    uniqtempid = templateFields.TemplateuniqueId;

                    return RedirectToAction("EditFormData", new { TemplateUniqueId = uniqtempid, returnSearchTag = returnurltag });

                }

                else
                {



                    var objFormCollection = HttpContext.Request.ReadFormAsync();
                    var dict = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());


                    int Tagid = Convert.ToInt32(dict.FirstOrDefault(e => e.Key.Contains("HiddenData")).Value);
                    var currentfield = _context.TemplateFields.FirstOrDefault(e => e.Id == Tagid);


                    var Fieldavalibity = dict.FirstOrDefault(e => e.Key.Contains(currentfield.name));


                    if (Fieldavalibity.Key == null && Fieldavalibity.Value == null)
                    {
                        currentfield.LatestValue = "";
                        _context.SaveChanges();
                    }

                    else
                    {

                        //get dynamic data from field
                        string FieldName = Fieldavalibity.Key;
                        string FieldData = Fieldavalibity.Value;


                        //var templateFields = _context.TemplateFields.FirstOrDefault(e => e.Id == fieldid && e.name == FieldName);

                        currentfield.LatestValue = FieldData;
                        _context.SaveChanges();
                    }
                    //return back url
                    returnurltag = _context.Tag.FirstOrDefault(r => r.Id == currentfield.TagId).TagNumber;
                    uniqtempid = currentfield.TemplateuniqueId;

                    return RedirectToAction("EditFormData", new { TemplateUniqueId = uniqtempid, returnSearchTag = returnurltag });

                }

            }
            catch (Exception ex)
            {
                return View();
            }
        }


        /// <summary>
        /// download File
        /// </summary>
        /// <param name="FileId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult downloadFile(int FileId)
        {
            string wwwrootPath = _env.WebRootPath + "\\Uploads\\FormData";
            var fileinfo = _context.FilesManager.FirstOrDefault(e => e.Id == FileId);
            FileInfo file = new FileInfo(Path.Combine(wwwrootPath, fileinfo.Name));
            return downloadFileFinal(wwwrootPath, fileinfo.FileType, fileinfo.Name);
        }
        public FileResult downloadFileFinal(string filePath, string contenttype, string filename)
        {
            IFileProvider provider = new PhysicalFileProvider(filePath);
            IFileInfo fileInfo = provider.GetFileInfo(filename);
            var readStream = fileInfo.CreateReadStream();
            var mimeType = contenttype;
            var namefile = filename.Split("@")[1];
            return File(readStream, mimeType, namefile);
        }


        /// <summary>
        /// Delete File
        /// </summary>
        /// <param name="FileId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteFile(int FileId)
        {
            string wwwrootPath = _env.WebRootPath + "\\Uploads\\FormData";
            var fileinfo = _context.FilesManager.FirstOrDefault(e => e.Id == FileId);
            FileInfo file = new FileInfo(Path.Combine(wwwrootPath, fileinfo.Name));
            if (file != null)
            {
                file.Delete();
                _context.FilesManager.Remove(fileinfo);
                _context.SaveChanges();
            }


            return Json("Sucess");

        }

        /// <summary>
        /// Delete Form Entry By Id
        /// </summary>
        /// <param name="EntryId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteFormEntryById(int EntryId)
        {

            var fileinfo = _context.TagFormDataEntry.FirstOrDefault(e => e.Id == EntryId);
            _context.Remove(fileinfo);
            _context.SaveChanges();
            return Json("Sucess");

        }

        /// <summary>
        /// Request for Guest Access
        /// </summary>
        /// <param name="EntryId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RequestForGuestAccessTagForm(SpecialPermission specialPermission)
        {
            try
            {
                specialPermission.Requested_User = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var Req = _context.SpecialPermission.Where(e => e.Requested_User == specialPermission.Requested_User && e.Unique_Template_Id == specialPermission.Unique_Template_Id && e.Rejected ==false).ToList() ;

                if(Req.Count != 0)
                {
                    return Json(new { Success = false, msg = "Your Request Already Pending." });

                }

                int CompanyId = (from TaginsideTemp in _context.TaginsideTemplates
                                 join
                                 Temp in _context.Template on TaginsideTemp.OrignalTemplateId equals Temp.TemplateId
                                 where TaginsideTemp.TemplateuniqueId == specialPermission.Unique_Template_Id && TaginsideTemp.TagId == Convert.ToInt32(specialPermission.TagId)
                                 select Temp.CompanyId).FirstOrDefault();


                specialPermission.Company_Id = CompanyId;
                specialPermission.AccessGranted = false;
                specialPermission.Rejected = false;
                specialPermission.Requested_User = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                _context.SpecialPermission.Add(specialPermission);
                _context.SaveChanges();
                return Json(new { Success = true, msg = "Read Request Send Successfully." });
            }
            catch
            {
                return Json(new { Success = false, msg = "Something Wrong." });
            }

        }



        /// <summary>
        /// Tag Json To List Convert
        /// </summary>
        /// <param name="TagCode"></param>
        /// <returns>List TagInsideTemplatesJson</returns>
        public List<TagInsideTemplatesJson> TagJsonToListConvert(string TagCode)
        {
            //tag json to TagInsideTemplatesJson  type convert

            List<TagInsideTemplatesJson> TagInsideTemplatesJson = new List<TagInsideTemplatesJson>();
            var datatagcode = JsonConvert.DeserializeObject<List<object>>(TagCode);//remove extra bracket from json and convert to array 
            var MainList = JsonConvert.DeserializeObject<List<object>>(datatagcode[0].ToString());//second orignal list fatch from json


            foreach (var dt in MainList)
            {

                var parentsNode = JsonConvert.DeserializeObject<Dictionary<string, object>>(dt.ToString()); //p
                if (parentsNode["type"].ToString() == "Group")
                {

                    var GroupChildNode = JsonConvert.DeserializeObject<List<object>>(parentsNode["children"].ToString());
                    var GroupChildClass = JsonConvert.DeserializeObject<List<TagInsideTemplatesJson>>(GroupChildNode[0].ToString());

                    foreach (var dtchild in GroupChildClass)
                    {
                        TagInsideTemplatesJson lst = new TagInsideTemplatesJson();
                        lst.secondname = dtchild.secondname;
                        lst.uniqid = dtchild.uniqid;
                        TagInsideTemplatesJson.Add(lst);
                    }
                }

                else
                {

                    TagInsideTemplatesJson lst = new TagInsideTemplatesJson();
                    lst.secondname = Convert.ToString(parentsNode["secondname"]);
                    lst.uniqid = Convert.ToInt32(parentsNode["uniqid"]);
                    TagInsideTemplatesJson.Add(lst);

                }


            }

            return TagInsideTemplatesJson;

        }



    }

}
