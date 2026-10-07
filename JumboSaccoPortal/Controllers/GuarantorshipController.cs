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
                    return Json(new
                    {
                        success = true,
                        message = "Please provide the OTP sent to your mobile phone for confirmation of the guarantorship request."
                    });
                }

                if (navresponse == "INSUFFICIENTABILITY")
                {
                    return Json(new
                    {
                        success = false,
                        message = "You do not have sufficient guarantee ability for the amount entered."
                    });
                }

                if (navresponse == "ALREADYDONE")
                {
                    return Json(new
                    {
                        success = false,
                        message = "This guarantorship request has already been processed."
                    });
                }

                if (navresponse == "INVALIDAMOUNT")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please enter a valid amount to guarantee."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Something went wrong. Please try again later."
                });
            }
        }

        [HttpPost]
        public ActionResult FinalizeGuarantorshipApproval(int guarantorshipId, string otp)
        {
            if (Session["memberno"] == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Your session has expired. Please log in again."
                });
            }

            var membernumber = Session["memberno"] as string;

            try
            {
                System.Diagnostics.Debug.WriteLine(
                    "===== FINALIZE GUARANTORSHIP ====="
                );

                System.Diagnostics.Debug.WriteLine(
                    "Guarantorship ID: " + guarantorshipId
                );

                System.Diagnostics.Debug.WriteLine(
                    "Member No: " + membernumber
                );

                System.Diagnostics.Debug.WriteLine(
                    "OTP: " + otp
                );

                using (var client = new ProxyClient())
                {
                    string navresponse = client.CompleteGuarantorshipApproval(
                        guarantorshipId,
                        membernumber,
                        otp
                    );

                    System.Diagnostics.Debug.WriteLine(
                        "Proxy response: " + navresponse
                    );

                    if (navresponse == "TRUE")
                    {
                        TempData["SuccessMessage"] =
                            "Guarantorship request accepted successfully.";

                        return Json(new
                        {
                            success = true,
                            message = "Guarantorship request accepted successfully."
                        });
                    }

                    if (navresponse == "WRONGOTP")
                    {
                        return Json(new
                        {
                            success = false,
                            message = "The OTP provided is wrong. Kindly provide the correct OTP."
                        });
                    }

                    return Json(new
                    {
                        success = false,
                        message = "Something went wrong. Response: " + navresponse
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "FINALIZE ERROR: " + ex.ToString()
                );

                return Json(new
                {
                    success = false,
                    message = "OTP verification failed: " + ex.Message
                });
            }
        }
    }
}