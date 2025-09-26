using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace JumboSaccoPortal.Controllers
{
    public class FeedbackController : Controller
    {
        public ActionResult Index()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Send(MemberFeedbackViewModel model)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter your feedback.";
                return RedirectToAction("Index");
            }

            try
            {
                string membernumber = Session["memberno"] as string;
                string subject = $"Feedback from Member {membernumber} - PORTAL";
                string body = $"Member Number: {membernumber}\n\nMessage:\n{model.Message}\n\nKindly call back the member.";


                // Read SMTP settings from web.config
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                bool enableSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"]);
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                string toAddress = ConfigurationManager.AppSettings["SmtpTo"];

                var mail = new MailMessage();
                mail.From = new MailAddress(smtpUser);
                mail.To.Add(toAddress);
                mail.Subject = subject;
                mail.Body = body;

                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtp.EnableSsl = enableSsl;
                    smtp.Send(mail);
                }

                TempData["SuccessMessage"] = "Your feedback has been sent successfully.";
            }
            catch (SmtpException ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                }

                TempData["ErrorMessage"] = "❌ SMTP Error: " + errorMsg;

                System.Diagnostics.Debug.WriteLine("SMTP Exception: " + ex.ToString());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ General Error: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("General Exception: " + ex.ToString());
            }


            return RedirectToAction("Index");
        }
    }

    public class MemberFeedbackViewModel
    {
        public string Message { get; set; }
    }
}
