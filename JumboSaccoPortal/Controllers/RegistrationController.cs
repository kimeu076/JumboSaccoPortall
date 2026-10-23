using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Web.Mvc;
using JumboSaccoPortal.Models;

namespace JumboSaccoPortal.Controllers
{
    public class RegistrationController : Controller
    {
        public ActionResult Index()
        {
            LoadLists();
            return View(new MemberRegistrationModel());
        }

        [HttpPost]
        public ActionResult RegisterSubmit(MemberRegistrationModel model)
        {
            try
            {


                if (string.IsNullOrWhiteSpace(model.FirstName) ||
                    string.IsNullOrWhiteSpace(model.LastName) ||
                    string.IsNullOrWhiteSpace(model.IDNo) ||
                    string.IsNullOrWhiteSpace(model.MobilePhoneNo) ||
                    string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.KRAPIN) ||
                    string.IsNullOrWhiteSpace(model.PayrollNo) ||
                    string.IsNullOrWhiteSpace(model.Address) ||
                    string.IsNullOrWhiteSpace(model.PostalCode) ||
                    string.IsNullOrWhiteSpace(model.EmployerCode))
                {
                    return Fail("Kindly fill in all the required fields.", model);
                }

                DateTime dob;
                if (!DateTime.TryParseExact(model.DateOfBirth, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out dob))
                {
                    return Fail("Kindly provide a valid Date of Birth.", model);
                }
                // BC SOAP rejects null parameters, so send empty strings instead
                model.FirstName = model.FirstName ?? "";
                model.MiddleName = model.MiddleName ?? "";
                model.LastName = model.LastName ?? "";
                model.PayrollNo = model.PayrollNo ?? "";
                model.Address = model.Address ?? "";
                model.PostalCode = model.PostalCode ?? "";
                model.Town = model.Town ?? "";
                model.CountryRegionCode = model.CountryRegionCode ?? "";
                model.County = model.County ?? "";
                model.MobilePhoneNo = model.MobilePhoneNo ?? "";
                model.SecondaryMobileNo = model.SecondaryMobileNo ?? "";
                model.Email = model.Email ?? "";
                model.IDNo = model.IDNo ?? "";
                model.PassportNo = model.PassportNo ?? "";
                model.KRAPIN = model.KRAPIN ?? "";
                model.Gender = model.Gender ?? "";
                model.MaritalStatus = model.MaritalStatus ?? "";
                model.HowDidYouKnow = model.HowDidYouKnow ?? "";
                model.RecruitedBy = model.RecruitedBy ?? "";
                model.RecruiterName = model.RecruiterName ?? "";
                model.ApplicationCategory = model.ApplicationCategory ?? "";
                model.EmployerCode = model.EmployerCode ?? "";
                model.Workstation = model.Workstation ?? "";
                model.Department = model.Department ?? "";
                using (var client = new ProxyClient())
                {
                    string response = client.MemberApplication(
                        model.FirstName, model.MiddleName, model.LastName, model.PayrollNo,
                        model.Address, model.PostalCode, model.Town, model.CountryRegionCode,
                        model.County, model.MobilePhoneNo, model.SecondaryMobileNo, model.Email,
                        dob, model.IDNo, model.PassportNo, model.KRAPIN,
                        model.Gender, model.MaritalStatus, model.HowDidYouKnow,
                        model.RecruitedBy, model.RecruiterName, model.ApplicationCategory,
                        model.EmployerCode, model.Workstation, model.Department);

                    if (response != null && response.StartsWith("SUCCESS"))
                    {
                        string applicationNo = response.Replace("SUCCESS:", "").Trim();
                        TempData["SuccessMessage"] =
                            "Your application has been received. Application No: " + applicationNo;
                        return RedirectToAction("Index", "Login");
                    }

                    return Fail(string.IsNullOrWhiteSpace(response)
                        ? "Something went wrong. Kindly try again later."
                        : response, model);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (ex.InnerException != null) msg += " | " + ex.InnerException.Message;
                return Fail("DEBUG: " + msg, model);
            }
        }

        private ActionResult Fail(string message, MemberRegistrationModel model)
        {
            TempData["ErrorMessage"] = message;
            LoadLists();
            return View("Index", model);
        }

        private void LoadLists()
        {
            ViewBag.Employers = new List<LookupItem>();
            ViewBag.PostCodes = new List<LookupItem>();
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using (var client = new ProxyClient())
                {
                    ViewBag.Employers = JsonSerializer.Deserialize<List<LookupItem>>(client.EmployerList(), opts);
                    ViewBag.PostCodes = JsonSerializer.Deserialize<List<LookupItem>>(client.PostCodeList(), opts);
                }
            }
            catch (Exception)
            {
                // Lists stay empty; the page still loads
            }
        }
    }
}