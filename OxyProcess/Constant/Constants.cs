using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Constant
{
    public static class Constants
    {
        #region Comman
        public const string ExMsg = "Something is wrong ! please try again";
        public const string Success = "Success";
        #endregion

        #region Templates
        public const string TemplateNameExists = "TemplateName already available";
        #endregion


        #region Groups
        public const string userNotFound = "Member does not exists";
        public const string memberAddSuccess = "Group member added successfully";
        public const string memberAvailable = "Member already available in this group";
        public const string emailRequired = "Member email must be required";
        #endregion
    }
}
