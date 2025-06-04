using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using JumboSaccoPortal.Models;
using static JumboSaccoPortal.Models.infoclasses;
using System.Text.Json;

namespace JumboSaccoPortal.Controllers
{
    public class GuarantorshipController : Controller
    {
        // GET: Guarantorship
        public ActionResult Index()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            var memberguarantorships = new MemberGuarantorships();

            using (var client = new ProxyClient())
            {
                string jsonstring = client.GetMemberGuarantorships(membernumber);

                if (jsonstring != "NOTFOUND")
                {
                    jsonstring = Regex.Replace(jsonstring, @",(\s*[}\]])", "$1");

                    var apiResponse = JsonSerializer.Deserialize<MemberGuarantorships>(jsonstring);
                    memberguarantorships = apiResponse;
                }
            }

            return View("Index", memberguarantorships);
        }

        public ActionResult GuarantorshipDetails(int guarantorshipid)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var guarantorshipdits = new GuarantorshipDetails();

            using (var client = new ProxyClient())
            {
                string jsonstring = client.GetGuarantorshipDetails(guarantorshipid);

                if (jsonstring != "NOTFOUND")
                {
                    jsonstring = Regex.Replace(jsonstring, @",(\s*[}\]])", "$1");

                    var serializedresponse = JsonSerializer.Deserialize<GuarantorshipDetails>(jsonstring);

                    guarantorshipdits = serializedresponse;

                }
            }

            return View("GuarantorshipDetails", guarantorshipdits);
        }

        [HttpPost]
        public ActionResult RejectGuarantorship(int guarantorshipId, string rejectComment)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;

            using (var client = new ProxyClient())
            {
                string navresponse = client.RejectGuarantorship(guarantorshipId, membernumber, rejectComment);

                if (navresponse == "TRUE")
                {
                    TempData["SuccessMessage"] = "Guarantorship request rejected successfully.";
                    return Json(new { success = true, message = "Guarantorship request rejected successfully." });
                }
                else
                {
                    //return Json(new { success = false, message = "An error occurred: " + navresponse });
                    return Json(new { success = false, message = "Something went wrong. Please try again later" });
                }
            }
        }
        [HttpPost]
        public ActionResult InitiateGuarantorshipApproval(int guarantorshipId, decimal amountToGuarantee)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            using (var client = new ProxyClient())
            {
                string navresponse = client.StartGuarantorshipApproval(guarantorshipId, membernumber, amountToGuarantee);

                
                if (navresponse == "TRUE")
                {
                    return Json(new { success = true, message = "Please provide the OTP sent to your mobile phone for confirmation of the guarantorship request." });
                }
                else
                {
                    //return Json(new { success = false, message = "An error occurred: " + navresponse });
                    return Json(new { success = false, message = "Something went wrong. Please try again later" });
                }
            }
        }

        [HttpPost]
        public ActionResult FinalizeGuarantorshipApproval(int guarantorshipId, string otp)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            using (var client = new ProxyClient())
            {
                string navresponse = client.CompleteGuarantorshipApproval(guarantorshipId, membernumber, otp);

                if (navresponse == "TRUE")
                {
                    TempData["SuccessMessage"] = "Guarantorship request accepted successfully.";
                    return Json(new { success = true, message = "Guarantorship request accepted successfully." });
                }

                else if (navresponse == "WRONGOTP")
                {
                    return Json(new { success = false, message = "The OTP provided is wrong. Kindly provide the correct OTP." });
                }
                else
                {
                    //return Json(new { success = false, message = "An error occurred: " + navresponse });
                    return Json(new { success = false, message = "Something went wrong. Please try again later" });
                }
            }
        }
    }
}