using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JumboSaccoPortal.Models;
using static JumboSaccoPortal.Models.infoclasses;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace JumboSaccoPortal.Controllers
{
    public class LoansController : Controller
    {
        // GET: Loans
        public ActionResult Index()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            RootLoanResponse response = new RootLoanResponse();
            using (var client = new ProxyClient())
            {
                string jsonstring = client.MemberLoans(membernumber);
                //string jsonstring = "NOTFOUND";

                if (jsonstring != "NOTFOUND")
                {
                    jsonstring = Regex.Replace(jsonstring, @",(\s*[}\]])", "$1");

                    var loanResponse = JsonSerializer.Deserialize<RootLoanResponse>(jsonstring);

                    response = loanResponse;
                    return View(response);
                }
                return View(response);
            }
        }

        public ActionResult LoanCalculator()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            LoanCalculatorTypes response = new LoanCalculatorTypes();
            using (var client = new ProxyClient())
            {
                string jsonstring = client.LoanCalculatorLoanTypes();

                if (jsonstring != "NOTFOUND")
                {
                    jsonstring = Regex.Replace(jsonstring, @",(\s*[}\]])", "$1");

                    try
                    {
                        var loanResponse = JsonSerializer.Deserialize<LoanCalculatorTypes>(jsonstring);

                        if (loanResponse?.LoanTypes != null)
                        {
                            foreach (var loanType in loanResponse.LoanTypes)
                            {
                                loanType.LoanInstallments = Regex.Replace(loanType.LoanInstallments, "M", "");
                            }
                            response = loanResponse;
                        }
                        else
                        {
                            response = new LoanCalculatorTypes { LoanTypes = new List<LoanType>() };
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                        response = new LoanCalculatorTypes { LoanTypes = new List<LoanType>() };
                    }
                    return View(response);
                }
                return View("LoanCalculator", response);
            }
        }

        public ActionResult OnlineLoanApplications()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            var groupedLoans = new LoanStatusGroups();

            using (var client = new ProxyClient())
            {
                string jsonstring = client.OnlineLoanApplications(membernumber);

                if (jsonstring != "NOTFOUND")
                {
                    jsonstring = Regex.Replace(jsonstring, @",(\s*[}\]])", "$1");

                    var apiResponse = JsonSerializer.Deserialize<LoanApiResponse>(jsonstring);

                    

                    foreach (var loan in apiResponse.OnlineLoansApps)
                    {
                        switch (loan.Status.ToLower())
                        {
                            case "new":
                                groupedLoans.NewLoans.Add(loan);
                                break;
                            case "pending":
                                groupedLoans.PendingLoans.Add(loan);
                                break;
                            case "approved":
                                groupedLoans.ApprovedLoans.Add(loan);
                                break;
                            case "rejected":
                                groupedLoans.RejectedLoans.Add(loan);
                                break;
                        }
                    }
                }
            }

            return View("OnlineLoanApplications", groupedLoans);
        }

        public ActionResult LoanApplicationDetails(int loanid)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var LoanInfor = new OnlineLoanApplication();

            using (var client = new ProxyClient())
            {
                string jsonstring = client.GetLoanApplicationDetails(loanid);

                if (jsonstring != "NOTFOUND")
                {
                    jsonstring = Regex.Replace(jsonstring, @",(\s*[}\]])", "$1");

                    var serializedresponse = JsonSerializer.Deserialize<OnlineLoanApplication>(jsonstring);

                    LoanInfor = serializedresponse;

                }
            }

            return View("LoanApplicationDetails", LoanInfor);
        }

        public ActionResult RemoveGuarantor(int loanid, int guarantorshipid)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            using (var client = new ProxyClient())
            {
                string jsonstring = client.RemoveGuarantorByRequestor(guarantorshipid);

                if (jsonstring == "TRUE")
                {
                    TempData["SuccessMessage"] = "Guarantor has been removed successfully";
                    return RedirectToAction("LoanApplicationDetails", new { loanid = loanid });
                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong. Please try again later";
                    return RedirectToAction("LoanApplicationDetails", new { loanid = loanid });
                }
            }
        }


        [HttpPost]
        public ActionResult SearchGuarantor(int loanId, string guarantorId)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                using (var client = new ProxyClient())
                {
                    // Call your service to search for guarantor
                    string guarantorInfo = client.SearchGuarantor(guarantorId);

                    if (guarantorInfo == "NOTFOUND")
                    {
                        return Json(new { success = false, message = "Guarantor not found" });
                    }

                    var guarantor = JsonSerializer.Deserialize<GuarantorInfo>(guarantorInfo);

                    return Json(new
                    {
                        success = true,
                        guarantorId = guarantor.GuarantorID,
                        guarantorName = guarantor.GuarantorName
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error
                return Json(new { success = false, message = "Error searching for guarantor" });
            }
        }

        [HttpPost]
        public ActionResult AddGuarantor(int loanId, string guarantorId)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                using (var client = new ProxyClient())
                {
                    string result = client.AddGuarantorToLoan(loanId, guarantorId);

                    if (result == "TRUE")
                    {
                        TempData["SuccessMessage"] = "Guarantor has been added successfully";
                    }
                    else if (result == "EXISTING")
                    {
                        TempData["ErrorMessage"] = "The guarantor is already added to your loan application. Kindly follow up with them for approval.";
                    }
                    else
                    {
                        //TempData["ErrorMessage"] = "Failed to add guarantor: " + result;
                        TempData["ErrorMessage"] = "Guarantor cannot be added at this time. Please try again later";
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                TempData["ErrorMessage"] = "An error occurred while adding the guarantor";
            }

            return RedirectToAction("LoanApplicationDetails", new { loanid = loanId });
        }


        public ActionResult NewLoanApplication()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            LoanUserModel dashboarddetails = new LoanUserModel();
            LoanApplicationPageModel pagemodeldetails = new LoanApplicationPageModel();

            using (var client = new ProxyClient())
            {
                // Get member dashboard info
                string jsonstring = client.LoanUserDetails(membernumber);
                // Get loan types
                string loanjsonstring = client.LoanCalculatorLoanTypes();

                // Process loan types
                if (loanjsonstring != "NOTFOUND")                {
                    // Clean the loan types JSON
                    loanjsonstring = Regex.Replace(loanjsonstring, @",(\s*[}\]])", "$1");
                    try
                    {
                        var loanResponse = JsonSerializer.Deserialize<LoanCalculatorTypes>(loanjsonstring);
                        if (loanResponse?.LoanTypes != null)
                        {
                            foreach (var loanType in loanResponse.LoanTypes)
                            {
                                loanType.LoanInstallments = Regex.Replace(loanType.LoanInstallments, "M", "");
                            }
                            pagemodeldetails.loantypesmodel = loanResponse;
                        }
                        else
                        {
                            pagemodeldetails.loantypesmodel = new LoanCalculatorTypes { LoanTypes = new List<LoanType>() };
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                        pagemodeldetails.loantypesmodel = new LoanCalculatorTypes { LoanTypes = new List<LoanType>() };
                    }
                }
                else
                {
                    pagemodeldetails.loantypesmodel = new LoanCalculatorTypes { LoanTypes = new List<LoanType>() };
                }

                // Process member dashboard info
                if (!string.IsNullOrEmpty(jsonstring))
                {
                    try
                    {
                        jsonstring = Regex.Replace(jsonstring, @"(?<=\d),(?=\d)", "");
                        dashboarddetails = JsonSerializer.Deserialize<LoanUserModel>(jsonstring);
                        pagemodeldetails.usermodel = dashboarddetails;
                    }
                    catch (JsonException ex)
                    {

                    }
                }
                else
                {
                    pagemodeldetails.usermodel = new LoanUserModel { };
                }
            }

            return View("NewLoanApplication", pagemodeldetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitLoanApplication(UserDetails model, HttpPostedFileBase Payslip1, HttpPostedFileBase Payslip2, HttpPostedFileBase Payslip3, HttpPostedFileBase IDCopy, HttpPostedFileBase KRAPin, HttpPostedFileBase CollateralCopy)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            try
            {
                // Validate files
                if (Payslip1 == null || Payslip1.ContentLength == 0 ||
                    Payslip2 == null || Payslip2.ContentLength == 0 ||
                    Payslip3 == null || Payslip3.ContentLength == 0 ||
                    IDCopy == null || IDCopy.ContentLength == 0 ||
                    KRAPin == null || KRAPin.ContentLength == 0 ||
                    CollateralCopy == null || CollateralCopy.ContentLength == 0)
                {
                    TempData["ErrorMessage"] = "All documents are required";
                    return RedirectToAction("NewLoanApplication");
                }

                // Validate file types
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                if (!allowedExtensions.Contains(Path.GetExtension(Payslip1.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(Payslip2.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(Payslip3.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(IDCopy.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(KRAPin.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(CollateralCopy.FileName).ToLower()))
                {
                    TempData["ErrorMessage"] = "Only PDF, JPG, JPEG, and PNG files are allowed";
                    return RedirectToAction("NewLoanApplication");
                }

                // Create loan application folder
                string loanFolderName = $"LoanApp_{model.MemberNumber}_{DateTime.Now:yyyyMMddHHmmss}";
                string uploadPath = Path.Combine(ConfigurationManager.AppSettings["LoanDocumentsPath"], loanFolderName);

                Directory.CreateDirectory(uploadPath);

                // Save documents
                string payslip1Path = Path.Combine(uploadPath, "FirstPayslip" + Path.GetExtension(Payslip1.FileName));
                string payslip2Path = Path.Combine(uploadPath, "SecondPayslip" + Path.GetExtension(Payslip2.FileName));
                string payslip3Path = Path.Combine(uploadPath, "ThirdPayslip" + Path.GetExtension(Payslip3.FileName));
                string idCopyPath = Path.Combine(uploadPath, "IDCopy" + Path.GetExtension(IDCopy.FileName));
                string kraPinPath = Path.Combine(uploadPath, "KRAPin" + Path.GetExtension(KRAPin.FileName));
                string CollateralCopyPath = Path.Combine(uploadPath, "Collateral" + Path.GetExtension(CollateralCopy.FileName));

                Payslip1.SaveAs(payslip1Path);
                Payslip2.SaveAs(payslip2Path);
                Payslip3.SaveAs(payslip3Path);
                IDCopy.SaveAs(idCopyPath);
                KRAPin.SaveAs(kraPinPath);
                CollateralCopy.SaveAs(CollateralCopyPath);

                // Prepare loan application data for web service
                var loanApplication = new
                {
                    MemberId = model.MemberNumber,
                    LoanType = Request.Form["LoanType"],
                    RequestedAmount = Request.Form["RequestedAmount"],
                    RepaymentPeriod = Request.Form["RepaymentPeriod"],
                    DocumentsFolder = loanFolderName,
                    ApplicationDate = DateTime.Now
                };

                // Submit to web service
                using (var client = new ProxyClient())
                {
                    string response = client.StartLoanApplication(loanApplication.MemberId, loanApplication.LoanType, decimal.Parse(loanApplication.RequestedAmount), loanApplication.DocumentsFolder);

                    if (response == "PENDINGAPP")
                    {
                        TempData["ErrorMessage"] = "You have a pending loan application and cannot apply for a second loan at the moment!";
                        Directory.Delete(uploadPath, true);
                        return RedirectToAction("OnlineLoanApplications");
                    }
                    else if (response == "TRUE")
                    {
                        TempData["SuccessMessage"] = "Loan application submitted successfully!";
                        return RedirectToAction("OnlineLoanApplications");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to submit loan application: " + response;
                        Directory.Delete(uploadPath, true);
                        return RedirectToAction("NewLoanApplication");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your application" + ex.Message;
                return RedirectToAction("NewLoanApplication");
            }
        }
    }

    public class LoanApplicationPageModel
    {
        public LoanUserModel usermodel { get; set; }
        public LoanCalculatorTypes loantypesmodel { get; set; }
    }

    public class LoanUserModel
    {
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal LoanLiability { get; set; }
    }
}