using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JumboSaccoPortal.Models;

namespace JumboSaccoPortal.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            Session.Clear();
            return View();
        }
        public ActionResult ResetPassword()
        {
            return View("ResetPassword");
        }
        public ActionResult OTPConfirm()
        {
            if ((TempData["LoggingIn"] != null) || (TempData["ResettingPass"] != null))
            {
                return View("OTPConfirm");
            }
            return RedirectToAction("Index");
        }
        public ActionResult SetPassword()
        {
            if ((TempData["SetNewPassword"] != null))
            {
                ViewBag.Payroll = Session["membernumber"].ToString();
                return View("SetPassword");
            }
            return RedirectToAction("Index");
        }
        public ActionResult Captcha()
        {
            var rand = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var captchaText = new string(Enumerable.Repeat(chars, 6)
                                              .Select(s => s[rand.Next(s.Length)]).ToArray());
            Session["Captcha"] = captchaText;

            using (var bitmap = new Bitmap(150, 50))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                for (int i = 0; i < 20; i++)
                {
                    var x1 = rand.Next(0, bitmap.Width);
                    var y1 = rand.Next(0, bitmap.Height);
                    var x2 = rand.Next(0, bitmap.Width);
                    var y2 = rand.Next(0, bitmap.Height);
                    graphics.DrawLine(new Pen(Color.LightGray), x1, y1, x2, y2);
                }

                var font = new Font("Arial", 22, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
                var brush = Brushes.Black;

                for (int i = 0; i < captchaText.Length; i++)
                {
                    var angle = rand.Next(-30, 30);
                    graphics.TranslateTransform(20 * i + 10, 25);
                    graphics.RotateTransform(angle);
                    graphics.DrawString(captchaText[i].ToString(), font, brush, -10, -10);
                    graphics.ResetTransform();
                }

                for (int i = 0; i < 100; i++)
                {
                    var x = rand.Next(0, bitmap.Width);
                    var y = rand.Next(0, bitmap.Height);
                    bitmap.SetPixel(x, y, Color.Gray);
                }

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }

        public ActionResult LoginSubmit(string username, string password)
        {
            try
            {
                using (var client = new ProxyClient())
                {
                    string response = client.MemberLogin(username, password);

                    Session["userid"] = username;

                    if (response == "SUCCESS")
                    {
                        TempData["SuccessMessage"] = "Kindly provide the OTP sent to your mobile Phone to log in.";
                        TempData["LoggingIn"] = "Okay";
                        TempData["LoggingIn1"] = "Okay";
                        return RedirectToAction("OTPConfirm");
                    }
                    else if (response == "NOTACTIVE")
                    {
                        TempData["ErrorMessage"] = "Your account has not been activated...";
                        return View("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "The details provided are not correct...";
                        return View("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                // TEMP: show full error to help debug
                TempData["ErrorMessage"] = "Login failed: " + ex.Message;
                return View("Index");
            }
        }


        public ActionResult SetPasswordSubmit(string username, string password, string cpassword)
        {
            using (var client = new ProxyClient())
            {

                if (cpassword != password)
                {
                    TempData["ErrorMessage"] = "The provided passwords do not match. Kindly enter new password.";
                    return View("SetPassword");
                }

                string response = client.UpdateNewPasword(username, password);

                //response = "00434|TRUE";

                if (response.Contains("TRUE"))
                {
                    var parts = response.Split(new string[] { "|" }, StringSplitOptions.None);
                    Session["memberno"] = parts[0];
                    TempData["LoggingIn"] = null;
                    TempData["SuccessMessage"] = "Your password has been reset successfully";

                    return RedirectToAction("Index", "Dashboard");
                }
                else if (response == "WRONGOTP")
                {
                    TempData["ErrorMessage"] = "The OTP provided is wrong. Kindly provide the correct OTP";
                    return View("OTPConfirm");
                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong. Kindly try again later.";
                    return View("OTPConfirm");
                }
            }
        }

        public ActionResult OTPLogin(string otp)
        {
            using (var client = new ProxyClient())
            {                   

                if (TempData["ResettingPass1"] != null)
                {
                    string payroll = Session["membernumber"].ToString();

                    string response = client.ConfirmResetOTP(payroll, otp);
                    //response = "00444|TRUE";

                    if (response.Contains("TRUE"))
                    {
                        var parts = response.Split(new string[] { "|" }, StringSplitOptions.None);
                        Session["memberno"] = parts[0];
                        TempData["ResettingPass"] = null;
                        TempData["ResettingPass1"] = null;
                        TempData["SetNewPassword"] = "Okay";
                        TempData["SuccessMessage"] = "Kindly set your new Password below";
                        return RedirectToAction("SetPassword");
                    }
                    else if (response == "WRONGOTP")
                    {
                        TempData["ErrorMessage"] = "The OTP provided is wrong. Kindly provide the correct OTP";
                        return View("OTPConfirm");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Something went wrong. Kindly try again later.";
                        return View("OTPConfirm");
                    }
                }

                if (TempData["LoggingIn1"] != null)
                {
                    string userid = Session["userid"].ToString();

                    string response = client.ConformLoginOTP(userid, otp);
                    //response = "00444|TRUE";

                    if (response.Contains("TRUE"))
                    {
                        var parts = response.Split(new string[] { "|" }, StringSplitOptions.None);
                        Session["memberno"] = parts[0];
                        TempData["LoggingIn"] = null;
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (response == "WRONGOTP")
                    {
                        TempData["ErrorMessage"] = "The OTP provided is wrong. Kindly provide the correct OTP";
                        return View("OTPConfirm");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Something went wrong. Kindly try again later.";
                        return View("OTPConfirm");
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong. Kindly try again later.";
                    return View("OTPConfirm");
                }
            }
        }

        public ActionResult ResetPassSubmit(string username)
        {
            using (var client = new ProxyClient())
            {
                Session["membernumber"] = username;
                string response = client.PasswordResetInitiation(username);
                //response = "TRUE";

                if (response == "TRUE")
                {
                    TempData["SuccessMessage"] = "Kindly provide the OTP sent to your mobile Phone to reset your password.";
                    TempData["ResettingPass"] = "Okay";
                    TempData["ResettingPass1"] = "Okay";
                    return RedirectToAction("OTPConfirm");
                }
                else if (response == "NOTFOUND")
                {
                    TempData["ErrorMessage"] = "The provided details are wrong. Kindly provide the correct credentials";
                    return View("ResetPassword");
                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong. Kindly try again later.";
                    return View("ResetPassword");
                }
            }
        }
    }
}