using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using OxyProcess.Enum;
using OxyProcess.Interface;
using OxyProcess.Services;

namespace OxyProcess.Extensions
{
    public static class EmailSenderExtensions
    {

        public static Task resetpasswordConfirmationAsync(this IEmailSender emailSender, string FirstName,string LastName,string email, string link)
        {
            string getResetPasswordTemplate = EmailSender.Templates(TemplatetypeEnum.resetpassword);
            getResetPasswordTemplate = getResetPasswordTemplate.Replace("{{link}}", link);
            getResetPasswordTemplate = getResetPasswordTemplate.Replace("{{FirstName}}",FirstName);
            getResetPasswordTemplate = getResetPasswordTemplate.Replace("{{LastName}}",LastName);
            return emailSender.SendEmailAsync(email, "Reset password for Oxy process account", getResetPasswordTemplate);

        }

        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            string getEmailConfirmationTemplate = EmailSender.Templates(TemplatetypeEnum.Registerconform);
            getEmailConfirmationTemplate = getEmailConfirmationTemplate.Replace("{{link}}", link);
            return emailSender.SendEmailAsync(email, "Confirm your email", getEmailConfirmationTemplate);

        }

        public static Task SendWelcomeMessage(this IEmailSender emailSender, string email, string link)
        {
            string getWelcomeMsgTemplate = EmailSender.Templates(TemplatetypeEnum.WelcomeToOxyProcess);
            getWelcomeMsgTemplate = getWelcomeMsgTemplate.Replace("{{link}}", link);
            return emailSender.SendEmailAsync(email, "Welcome To OxyProcess",getWelcomeMsgTemplate);
        }

        
        public static Task SendLoginCredentialAsync(this IEmailSender emailSender, string email, string userId, string password,string url)
        {
            string getWorkerWelcomeTemplate = EmailSender.Templates(TemplatetypeEnum.Workerwelcome);
            getWorkerWelcomeTemplate = getWorkerWelcomeTemplate.Replace("{{userId}}", userId);
            getWorkerWelcomeTemplate = getWorkerWelcomeTemplate.Replace("{{password}}", password);
            getWorkerWelcomeTemplate = getWorkerWelcomeTemplate.Replace("{{link}}", url);
            return emailSender.SendEmailAsync(email, "Your login credential",getWorkerWelcomeTemplate);
        }
    }
}
