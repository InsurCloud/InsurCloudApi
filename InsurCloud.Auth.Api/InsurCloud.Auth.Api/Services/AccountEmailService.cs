using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace InsurCloud.Auth.Api.Services
{
    public enum EmailType
    {
        NewAccountCreated = 1,
        AccountActivated = 2,
        PasswordChanged = 3,
        ChangePasswordToken = 4,
        ForgotPasswordToken = 5
    }
    public class SendEmailInfo
    {
        public string EmailAddress {get; set;}
        public EmailType EmailType { get; set; }
        public string Token {get; set;}
    }

    public class AccountEmailService
    {
        NetworkCredential cred = null;

        public AccountEmailService()
        {
            cred = new NetworkCredential("azure_58aec298061d36500f4c7ec758f7b6dc@azure.com", "5OJoZE3M4NhuJ0R");
        }

        public async Task<bool> SendEmail(SendEmailInfo emailInfo)
        {
            SendGridMessage msg = null;
            if (emailInfo.EmailType == EmailType.ForgotPasswordToken)
            {
                msg = ForgotPasswordCreated(emailInfo);
            }else if (emailInfo.EmailType == EmailType.NewAccountCreated)
            {
                msg = NewAccountCreated(emailInfo.EmailAddress);
            }
            else if (emailInfo.EmailType == EmailType.PasswordChanged)
            {
                msg = PasswordChanged(emailInfo);
            }
            if (msg != null)
            {
                return await SendEmail(msg);
            }
            return false;
        } 
        public async Task<bool> SendEmail(SendGridMessage msg)
        {
            try
            {                
                var trans = new SendGrid.Web(cred);
                await trans.DeliverAsync(msg);                
                
                return true;
            }
            catch (Exception ex)
            {
                //TODO: Add Logging of Errors
                //return Request.CreateResponse(HttpStatusCode.BadRequest, new responseString() { value = ex.Message });
                return false;
                //Must be local
            }
            
        }

        private SendGridMessage NewAccountCreated(string emailAddress)
        {            
            if (emailAddress != "")
            {
                var msg = new SendGridMessage();
                msg.From = new MailAddress("donotreply@insurcloud.com");

                msg.AddTo(emailAddress);
                msg.AddBcc("info@insurcloud.com");

                msg.Subject = "Account Created";

                //Add the HTML and Text bodies
                msg.Html = "<h1>Welcome!</h1><hr/><p>Thank you for signing up to use InsurCloud!<br/><br/><h3>Your Administrator will send you an email once the account has been Activated</h3><br/><br/>Enjoy!</p>";
                return msg;

            }
            return null;
        }

        private SendGridMessage ForgotPasswordCreated(SendEmailInfo emailInfo)
        {
            if (emailInfo.EmailAddress != "")
            {
                var msg = new SendGridMessage();
                msg.From = new MailAddress("donotreply@insurcloud.com");                
                msg.AddTo(emailInfo.EmailAddress);
                //msg.AddBcc("info@insurcloud.com");

                msg.Subject = "Password Reset";

                //Add the HTML and Text bodies
                msg.Html = "<h1>Hello!</h1><hr/><p>A forgot password request was recently submitted at InsurCloud.com. Click the link below to reset your password.<br/><br/><h3>If you did not request this password change request, please report this to your administrator.</h3><br/><br/>Thank you!</p>";
                msg.Html += "<br/><br/><br/><center><a href='http://localhost:59099/reset.html#/reset?token=" + emailInfo.Token + "'>http://localhost:59099/reset.html#/reset?token=" + emailInfo.Token + "</a></center>";
                return msg;

            }
            return null;
        }

        private SendGridMessage PasswordChanged(SendEmailInfo emailInfo)
        {
            if (emailInfo.EmailAddress != "")
            {
                var msg = new SendGridMessage();
                msg.From = new MailAddress("donotreply@insurcloud.com");
                msg.AddTo(emailInfo.EmailAddress);
                //msg.AddBcc("info@insurcloud.com");

                msg.Subject = "Password Changed";

                //Add the HTML and Text bodies
                msg.Html = "<h1>Hello!</h1><hr/><p>We wanted to let you know that your password was recently changed on InsurCloud.com.<br/><br/><h3>If you did not change your password, please report this to your administrator.</h3><br/><br/>Thank you!</p>";

                return msg;

            }
            return null;
        }

    }
}