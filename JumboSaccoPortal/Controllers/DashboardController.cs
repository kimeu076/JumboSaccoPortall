using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JumboSaccoPortal.Models;
using System.Text.Json;
using static JumboSaccoPortal.Models.infoclasses;

namespace JumboSaccoPortal.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"].ToString();
            UserDetails dashboarddetails = new UserDetails();
            using (var client = new ProxyClient())
            {
                string jsonstring = client.DashboardInfo(membernumber);
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
                else
                {
                    ModelState.AddModelError("", "Error: No employee data received.");
                    Debug.WriteLine("Error: No approval data received.");
                }
            }
            return View(dashboarddetails);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}