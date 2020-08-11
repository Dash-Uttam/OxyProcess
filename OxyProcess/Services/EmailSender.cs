using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OxyProcess.Enum;
using OxyProcess.Helpers;
using OxyProcess.Interface;
using OxyProcess.Models.SendEmail;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OxyProcess.Services
{
    //public interface IEmailSender
    //{
    //    Task SendEmailAsync(string email, string subject, string htmlMessage);
    //}

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        public EmailSender(IOptions<EmailSettings> emailSettings )
        {
            _emailSettings = emailSettings.Value;
           
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                // Credentials

                string EmailDec_Pass = EncryptDecryptHelper.Decrypt(_emailSettings.Password);

                var credentials = new NetworkCredential(_emailSettings.Sender, EmailDec_Pass);

                // Mail message
                var mail = new MailMessage()
                {
                    Subject = subject,
                    Body = message,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    SubjectEncoding = System.Text.Encoding.Default,
                    From = new MailAddress(_emailSettings.Sender, _emailSettings.SenderName),
                    IsBodyHtml = true
                };

                mail.To.Add(new MailAddress(email));

                // Smtp client
                var client = new SmtpClient()
                {
                    Port = _emailSettings.MailPort,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = _emailSettings.MailServer,
                    EnableSsl = true,
                    Credentials = credentials
                };

                // Send it...         
                client.Send(mail);

            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
               
            }

            return Task.CompletedTask;
        }


        public static string Templates(TemplatetypeEnum temptype)
        {


            StreamReader objStreamReader;
            string path = "";
            HttpContextAccessor req = new HttpContextAccessor();
            var request = req.HttpContext.Request;

        

            if (temptype == TemplatetypeEnum.Registerconform)
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\email-template\ConfirmAccount.html");

              
            }

            if (temptype == TemplatetypeEnum.Workerwelcome)
            {
              
                path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\email-template\welcome.html");
            }

            if (temptype == TemplatetypeEnum.resetpassword)
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\email-template\ResetPassword.html");
             
            }
            if (temptype == TemplatetypeEnum.WelcomeToOxyProcess)
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\email-template\WelcomeToOxyProcess.html");

            }
            
            if (!string.IsNullOrEmpty(path))
            {
                //this line content error
                 objStreamReader = File.OpenText(path);
                string emailText = objStreamReader.ReadToEnd();
                objStreamReader.Close();
                objStreamReader = null;
                objStreamReader = null;
                return emailText;
            }
            else
            {
                objStreamReader = null;
                return string.Empty;
            }
        }

  


    }
}
