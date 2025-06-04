using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JumboSaccoPortal.Models;
using static JumboSaccoPortal.Models.infoclasses;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JumboSaccoPortal.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"].ToString();
            UserDetails dashboarddetails = new UserDetails();
            NextofKinsDetails nextofKinsDetails = new NextofKinsDetails();
            using (var client = new ProxyClient())
            {
                string jsonstring = client.DashboardInfo(membernumber);
                string nextjsonstring = client.NextOfkinDetails(membernumber);
                if (!string.IsNullOrEmpty(jsonstring))
                {
                    try
                    {
                        UserDetails response = JsonSerializer.Deserialize<UserDetails>(jsonstring);

                        dashboarddetails = response;


                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    }
                }
                if (!string.IsNullOrEmpty(nextjsonstring))
                {
                    try
                    {

                        nextjsonstring = Regex.Replace(nextjsonstring, @",(\s*[}\]])", "$1");
                        NextofKinsDetails nextofkinsjson = JsonSerializer.Deserialize<NextofKinsDetails>(nextjsonstring);

                        ViewBag.NextOfKInDetails = nextofkinsjson;


                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Error: No employee data received.");
                    Debug.WriteLine("Error: No approval data received.");
                }
            }
            return View(dashboarddetails);
        }

        public ActionResult SubmitRequest(string username, string oldpass, string newpass, string confnewpass)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            ViewBag.Username = username;
            ViewBag.Oldpass = oldpass;
            ViewBag.Newpass = newpass;
            ViewBag.Confnewpass = confnewpass;
            using (var client = new ProxyClient())
            {

                if (newpass != confnewpass)
                {
                    TempData["ErrorMessage"] = "The new provided passwords do not match. Kindly enter matching password.";

                    ViewBag.Newpass = "";
                    ViewBag.Confnewpass = "";
                    return RedirectToAction("Index");
                }

                if (oldpass == newpass)
                {
                    TempData["ErrorMessage"] = "The passwords are the same. Kindly ensure that the old and the new password are different.";

                    ViewBag.Newpass = "";
                    ViewBag.Oldpass = "";
                    ViewBag.Confnewpass = "";
                    return RedirectToAction("Index");
                }

                string response = client.ResetPasswordOnPortal(username, oldpass, newpass);

                if (response == "SUCCESS")
                {
                    TempData["SuccessMessage"] = "You have changed your password successfully.";
                    ViewBag.Newpass = "";
                    ViewBag.Oldpass = "";
                    ViewBag.Confnewpass = "";
                    return RedirectToAction("Index");
                }
                else if (response == "WRONGPASS")
                {
                    TempData["ErrorMessage"] = "The Old Password is wrong. Kindly try again";
                    ViewBag.Newpass = "";
                    ViewBag.Oldpass = "";
                    ViewBag.Confnewpass = "";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["SuccessMessage"] = "Your Password can't be changed at the moment. Please try again later.";
                    ViewBag.Newpass = "";
                    ViewBag.Oldpass = "";
                    ViewBag.Confnewpass = "";
                    return RedirectToAction("Index");
                }

            }
        }
    }
}