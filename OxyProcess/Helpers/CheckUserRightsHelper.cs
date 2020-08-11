using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using OxyProcess.Data;
using OxyProcess.Models.Template;
using OxyProcess.Models.UserRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Helpers
{
    public  class CheckUserRightsHelper
    {

        private readonly ApplicationDbContext _context;


        //UserRights UserRights  = new UserRights();

     
        public CheckUserRightsHelper(ApplicationDbContext _ApplicationDbContext)
        {
        
            _context = _ApplicationDbContext;
        }


        public UserRights CheckUserAcceess(int TemplateId, int UserId,int TempUniqId=0)
        {
            UserRights UserRights = new UserRights();
            Template template = _context.Template.FirstOrDefault(e => e.TemplateId == TemplateId);
            UserRights.Read = false;
            UserRights.Write = false;
            UserRights.None = true;



            UserRights.Read =  UserHasReadPermission(TemplateId, UserId, TempUniqId, template);
            UserRights.Write = UserHasWritePermission(TemplateId, UserId, template);

            if (UserRights.Read == true || UserRights.Write == true)
            {
                UserRights.None = false;

            }

            return UserRights;
        }

        public bool UserHasReadPermission(int TemplateId, int UserId,int TempUniqId, Template template)
        {

            bool HasReadPermission = false;
            //if template is public and is for read 
            if (template.Groups == "0")
            {

                HasReadPermission = true;

            }
            else
            {

                if (template.CreatedById == UserId.ToString())// For Creator or Owner Access Allow
                {

                    HasReadPermission = true;

                }

                else
                {

                    //for check user in group and he has read permission
                    List<string> CommaSaperatedGroup = template.Groups.Split(',').ToList();

                    var MembersinGroup = _context.GroupMembers.Where(c => CommaSaperatedGroup.Contains(c.GroupId.ToString())).ToList();

                    foreach (var d in MembersinGroup)
                    {

                        if (d.UserId == UserId)
                        {
                            var permission = _context.GroupPermission.FirstOrDefault(e => e.GroupId == d.GroupId);
                            if (permission.Read)
                            {
                                HasReadPermission = true;
                            }

                        }
                    }

                    if(TempUniqId != 0)
                    {
                      var SpecialPermission =  _context.SpecialPermission.FirstOrDefault(e => e.Requested_User == UserId && e.Unique_Template_Id == TempUniqId && e.Rejected == false);
                       

                        if(SpecialPermission != null)
                        {

                            HasReadPermission = SpecialPermission.AccessGranted == true ?   true :  false;

                        }

                    }
                   

                }
            }

            return HasReadPermission;

        }



        public bool UserHasWritePermission(int TemplateId, int UserId, Template template)
        {

            bool HasWritePermission = false;

            if (template.Groups == "0")
            {
                var Users = (from u1 in _context.Users
                             join u2 in _context.Users on u1.CompanyId equals u2.CompanyId
                             where u1.Id == Convert.ToInt64(template.CreatedById)
                             select u2).ToList();

                var Workerfromsame = Users.FirstOrDefault(e => e.Id == UserId);

                if (Workerfromsame != null)
                {
                    HasWritePermission = true;
                }
              
            }

            else
            {

                if (template.CreatedById == UserId.ToString())// For Creator or Owner Access Allow
                {
                    HasWritePermission = true;
                }
                else
                {

                    //for check user in group and he has read permission
                    List<string> CommaSaperatedGroup = template.Groups.Split(',').ToList();

                    var MembersinGroup = _context.GroupMembers.Where(c => CommaSaperatedGroup.Contains(c.GroupId.ToString())).ToList();

                    foreach (var d in MembersinGroup)
                    {

                        if (d.UserId == UserId)
                        {
                            var permission = _context.GroupPermission.FirstOrDefault(e => e.GroupId == d.GroupId);
                            if (permission.Write)
                            {
                                HasWritePermission = true;
                            }

                        }
                    }

                }

            }
            return HasWritePermission;

        }
    }
}
