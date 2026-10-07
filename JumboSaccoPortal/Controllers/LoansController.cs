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
        [HttpGet]
        public JsonResult GetLoanCharges(string loanProductType, decimal loanAmount)
        {
            Debug.WriteLine("");
            Debug.WriteLine("==============================================");
            Debug.WriteLine("GET LOAN CHARGES - CONTROLLER START");
            Debug.WriteLine("==============================================");

            Debug.WriteLine("Loan Product Type: " + loanProductType);
            Debug.WriteLine("Loan Amount: " + loanAmount);

            if (Session["memberno"] == null)
            {
                Debug.WriteLine("ERROR: Session memberno is NULL");

                return Json(new
                {
                    success = false,
                    message = "Your session has expired. Please login again."
                }, JsonRequestBehavior.AllowGet);
            }

            Debug.WriteLine("Session memberno: " + Session["memberno"]);

            if (string.IsNullOrWhiteSpace(loanProductType))
            {
                Debug.WriteLine("ERROR: Loan product type is empty");

                return Json(new
                {
                    success = false,
                    message = "Loan product type is required."
                }, JsonRequestBehavior.AllowGet);
            }

            if (loanAmount <= 0)
            {
                Debug.WriteLine("ERROR: Loan amount is invalid");

                return Json(new
                {
                    success = false,
                    message = "Loan amount must be greater than zero."
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                Debug.WriteLine("----------------------------------------------");
                Debug.WriteLine("Creating ProxyClient...");
                Debug.WriteLine("----------------------------------------------");

                using (var client = new ProxyClient())
                {
                    Debug.WriteLine("ProxyClient created successfully.");

                    Debug.WriteLine("----------------------------------------------");
                    Debug.WriteLine("Calling LoanCalculatorLoanCharges...");
                    Debug.WriteLine("----------------------------------------------");

                    string jsonstring = client.LoanCalculatorLoanCharges(
                        loanProductType,
                        loanAmount
                    );

                    Debug.WriteLine("----------------------------------------------");
                    Debug.WriteLine("PROXY RETURNED SUCCESSFULLY");
                    Debug.WriteLine("Raw BC Response:");
                    Debug.WriteLine(jsonstring);
                    Debug.WriteLine("----------------------------------------------");

                    if (string.IsNullOrWhiteSpace(jsonstring))
                    {
                        Debug.WriteLine("ERROR: BC response is empty.");

                        return Json(new
                        {
                            success = false,
                            message = "BC returned an empty response."
                        }, JsonRequestBehavior.AllowGet);
                    }

                    if (jsonstring == "NOTFOUND")
                    {
                        Debug.WriteLine("ERROR: BC returned NOTFOUND.");

                        return Json(new
                        {
                            success = false,
                            message = "BC returned NOTFOUND."
                        }, JsonRequestBehavior.AllowGet);
                    }

                    Debug.WriteLine("Cleaning JSON...");

                    jsonstring = Regex.Replace(
                        jsonstring,
                        @",(\s*[}\]])",
                        "$1"
                    );

                    Debug.WriteLine("Cleaned JSON:");
                    Debug.WriteLine(jsonstring);

                    Debug.WriteLine("----------------------------------------------");
                    Debug.WriteLine("Deserializing LoanCharges...");
                    Debug.WriteLine("----------------------------------------------");

                    var charges =
                        System.Text.Json.JsonSerializer.Deserialize<LoanCharges>(
                            jsonstring
                        );

                    if (charges == null)
                    {
                        Debug.WriteLine("ERROR: LoanCharges deserialized as NULL.");

                        return Json(new
                        {
                            success = false,
                            message = "Invalid loan charges response."
                        }, JsonRequestBehavior.AllowGet);
                    }

                    Debug.WriteLine("----------------------------------------------");
                    Debug.WriteLine("DESERIALIZATION SUCCESSFUL");
                    Debug.WriteLine("Appraisal: " + charges.Appraisal);
                    Debug.WriteLine("Insurance: " + charges.Insurance);
                    Debug.WriteLine("----------------------------------------------");

                    return Json(new
                    {
                        success = true,
                        appraisal = charges.Appraisal,
                        insurance = charges.Insurance
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("");
                Debug.WriteLine("==============================================");
                Debug.WriteLine("GET LOAN CHARGES - CONTROLLER ERROR");
                Debug.WriteLine("==============================================");

                Debug.WriteLine("Exception Type:");
                Debug.WriteLine(ex.GetType().FullName);

                Debug.WriteLine("Exception Message:");
                Debug.WriteLine(ex.Message);

                Debug.WriteLine("FULL EXCEPTION:");
                Debug.WriteLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    Debug.WriteLine("----------------------------------------------");
                    Debug.WriteLine("INNER EXCEPTION:");
                    Debug.WriteLine(ex.InnerException.ToString());
                }

                Debug.WriteLine("==============================================");

                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
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
                // ---------------------------------------------------------
                // GET LOAN APPLICATION DETAILS
                // ---------------------------------------------------------

                string jsonstring =
                    client.GetLoanApplicationDetails(loanid);

                if (jsonstring != "NOTFOUND")
                {
                    jsonstring = Regex.Replace(
                        jsonstring,
                        @",(\s*[}\]])",
                        "$1");

                    var serializedresponse =
                        JsonSerializer.Deserialize<OnlineLoanApplication>(
                            jsonstring);

                    LoanInfor = serializedresponse;
                }

                // ---------------------------------------------------------
                // GET AVAILABLE LOAN TYPES
                // ---------------------------------------------------------

                string loanTypesJson =
                    client.LoanCalculatorLoanTypes();

                if (!string.IsNullOrEmpty(loanTypesJson) &&
                    loanTypesJson != "NOTFOUND")
                {
                    loanTypesJson = Regex.Replace(
                        loanTypesJson,
                        @",(\s*[}\]])",
                        "$1");

                    var loanTypes =
                        JsonSerializer.Deserialize<LoanCalculatorTypes>(
                            loanTypesJson);

                    if (loanTypes != null)
                    {
                        ViewBag.LoanTypes = loanTypes.LoanTypes;
                    }
                }
            }

            return View(
                "LoanApplicationDetails",
                LoanInfor);
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
        public ActionResult SubmitLoanApplication(
            LoanApplicationPageModel model,
            HttpPostedFileBase Payslip1,
            HttpPostedFileBase Payslip2,
            HttpPostedFileBase Payslip3,
            HttpPostedFileBase IDCopy,
            HttpPostedFileBase KRAPin,
            HttpPostedFileBase CollateralCopy)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                // Validate required files
                if (Payslip1 == null || Payslip1.ContentLength == 0 ||
                    Payslip2 == null || Payslip2.ContentLength == 0 ||
                    Payslip3 == null || Payslip3.ContentLength == 0 ||
                    IDCopy == null || IDCopy.ContentLength == 0 ||
                    KRAPin == null || KRAPin.ContentLength == 0)
                {
                    TempData["ErrorMessage"] = "National ID, KRA PIN and all three payslips are required.";
                    return RedirectToAction("NewLoanApplication");
                }

                // Validate required file types
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

                if (!allowedExtensions.Contains(Path.GetExtension(Payslip1.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(Payslip2.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(Payslip3.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(IDCopy.FileName).ToLower()) ||
                    !allowedExtensions.Contains(Path.GetExtension(KRAPin.FileName).ToLower()))
                {
                    TempData["ErrorMessage"] = "Only PDF, JPG, JPEG, and PNG files are allowed.";
                    return RedirectToAction("NewLoanApplication");
                }

                // If collateral was provided, validate its file type
                if (CollateralCopy != null && CollateralCopy.ContentLength > 0)
                {
                    if (!allowedExtensions.Contains(
                        Path.GetExtension(CollateralCopy.FileName).ToLower()))
                    {
                        TempData["ErrorMessage"] = "Only PDF, JPG, JPEG, and PNG files are allowed for the collateral document.";
                        return RedirectToAction("NewLoanApplication");
                    }
                }

                // Create loan application folder
                string loanFolderName =
                    $"LoanApp_{model.usermodel.MemberNumber}_{DateTime.Now:yyyyMMddHHmmss}";

                string uploadPath = Path.Combine(
                    ConfigurationManager.AppSettings["LoanDocumentsPath"],
                    loanFolderName);

                Directory.CreateDirectory(uploadPath);

                // Save required documents
                string payslip1Path = Path.Combine(
                    uploadPath,
                    "FirstPayslip" + Path.GetExtension(Payslip1.FileName));

                string payslip2Path = Path.Combine(
                    uploadPath,
                    "SecondPayslip" + Path.GetExtension(Payslip2.FileName));

                string payslip3Path = Path.Combine(
                    uploadPath,
                    "ThirdPayslip" + Path.GetExtension(Payslip3.FileName));

                string idCopyPath = Path.Combine(
                    uploadPath,
                    "IDCopy" + Path.GetExtension(IDCopy.FileName));

                string kraPinPath = Path.Combine(
                    uploadPath,
                    "KRAPin" + Path.GetExtension(KRAPin.FileName));

                Payslip1.SaveAs(payslip1Path);
                Payslip2.SaveAs(payslip2Path);
                Payslip3.SaveAs(payslip3Path);
                IDCopy.SaveAs(idCopyPath);
                KRAPin.SaveAs(kraPinPath);

                // Save collateral only if the member provided one
                if (CollateralCopy != null && CollateralCopy.ContentLength > 0)
                {
                    string collateralCopyPath = Path.Combine(
                        uploadPath,
                        "Collateral" + Path.GetExtension(CollateralCopy.FileName));

                    CollateralCopy.SaveAs(collateralCopyPath);
                }

                // Prepare loan application data for web service
                var loanApplication = new
                {
                    MemberId = model.usermodel.MemberNumber,
                    LoanType = Request.Form["LoanType"],
                    RequestedAmount = Request.Form["RequestedAmount"],
                    RepaymentPeriod = Request.Form["RepaymentPeriod"],
                    DocumentsFolder = loanFolderName,
                    ApplicationDate = DateTime.Now
                };

                // Submit to web service
                using (var client = new ProxyClient())
                {
                    string response = client.StartLoanApplication(
                        loanApplication.MemberId,
                        loanApplication.LoanType,
                        decimal.Parse(loanApplication.RequestedAmount),
                        loanApplication.DocumentsFolder);

                    if (response == "PENDINGAPP")
                    {
                        TempData["ErrorMessage"] =
                            "You have a pending loan application and cannot apply for a second loan at the moment!";

                        Directory.Delete(uploadPath, true);

                        return RedirectToAction("OnlineLoanApplications");
                    }
                    else if (response == "TRUE")
                    {
                        TempData["SuccessMessage"] =
                            "Loan application submitted successfully!";

                        return RedirectToAction("OnlineLoanApplications");
                    }
                    else
                    {
                        TempData["ErrorMessage"] =
                            "Failed to submit loan application: " + response;

                        Directory.Delete(uploadPath, true);

                        return RedirectToAction("NewLoanApplication");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while processing your application" + ex.Message; ;

                return RedirectToAction("NewLoanApplication");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyLoanApplication(
    int loanId,
    string loanType,
    decimal requestedAmount,
    HttpPostedFileBase Payslip1,
    HttpPostedFileBase Payslip2,
    HttpPostedFileBase Payslip3,
    HttpPostedFileBase IDCopy,
    HttpPostedFileBase KRAPin,
    HttpPostedFileBase CollateralCopy)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                // ---------------------------------------------------------
                // GET EXISTING LOAN APPLICATION
                // ---------------------------------------------------------

                OnlineLoanApplication application;

                using (var client = new ProxyClient())
                {
                    string jsonstring = client.GetLoanApplicationDetails(loanId);

                    if (string.IsNullOrEmpty(jsonstring) ||
                        jsonstring == "NOTFOUND")
                    {
                        TempData["ErrorMessage"] =
                            "Loan application was not found.";

                        return RedirectToAction("OnlineLoanApplications");
                    }

                    jsonstring = Regex.Replace(
                        jsonstring,
                        @",(\s*[}\]])",
                        "$1");

                    application =
                        JsonSerializer.Deserialize<OnlineLoanApplication>(jsonstring);
                }

                if (application == null)
                {
                    TempData["ErrorMessage"] =
                        "Loan application was not found.";

                    return RedirectToAction("OnlineLoanApplications");
                }

                // ---------------------------------------------------------
                // ONLY NEW APPLICATIONS CAN BE MODIFIED
                // ---------------------------------------------------------

                if (application.Status != "New")
                {
                    TempData["ErrorMessage"] =
                        "This loan application can no longer be modified.";

                    return RedirectToAction(
                        "LoanApplicationDetails",
                        new { loanid = loanId });
                }

                // ---------------------------------------------------------
                // VALIDATE LOAN AMOUNT
                // ---------------------------------------------------------

                if (requestedAmount <= 0)
                {
                    TempData["ErrorMessage"] =
                        "Please enter a valid loan amount.";

                    return RedirectToAction(
                        "LoanApplicationDetails",
                        new { loanid = loanId });
                }

                // ---------------------------------------------------------
                // VALIDATE LOAN TYPE
                // ---------------------------------------------------------

                if (string.IsNullOrWhiteSpace(loanType))
                {
                    TempData["ErrorMessage"] =
                        "Please select a loan type.";

                    return RedirectToAction(
                        "LoanApplicationDetails",
                        new { loanid = loanId });
                }

                // ---------------------------------------------------------
                // GET EXISTING DOCUMENT FOLDER
                // ---------------------------------------------------------

                string folderName = application.DocumentsFolder;

                if (string.IsNullOrWhiteSpace(folderName))
                {
                    TempData["ErrorMessage"] =
                        "The document folder for this application could not be found.";

                    return RedirectToAction(
                        "LoanApplicationDetails",
                        new { loanid = loanId });
                }

                string uploadPath = Path.Combine(
                    ConfigurationManager.AppSettings["LoanDocumentsPath"],
                    folderName);

                if (!Directory.Exists(uploadPath))
                {
                    TempData["ErrorMessage"] =
                        "The document folder for this application could not be found.";

                    return RedirectToAction(
                        "LoanApplicationDetails",
                        new { loanid = loanId });
                }

                // ---------------------------------------------------------
                // ALLOWED FILE TYPES
                // ---------------------------------------------------------

                var allowedExtensions = new[]
                {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

                // ---------------------------------------------------------
                // VALIDATE ONLY FILES THAT WERE UPLOADED
                // ---------------------------------------------------------

                var files = new[]
                {
            Payslip1,
            Payslip2,
            Payslip3,
            IDCopy,
            KRAPin,
            CollateralCopy
        };

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string extension =
                            Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            TempData["ErrorMessage"] =
                                "Only PDF, JPG, JPEG, and PNG files are allowed.";

                            return RedirectToAction(
                                "LoanApplicationDetails",
                                new { loanid = loanId });
                        }
                    }
                }

                // ---------------------------------------------------------
                // DOCUMENT REPLACEMENT HELPER
                // ---------------------------------------------------------

                Action<HttpPostedFileBase, string> replaceDocument =
                    (file, documentName) =>
                    {
                        // No new document uploaded.
                        // Leave the existing document unchanged.
                        if (file == null || file.ContentLength <= 0)
                        {
                            return;
                        }

                        // Find existing document regardless of extension.
                        string[] existingFiles =
                            Directory.GetFiles(
                                uploadPath,
                                documentName + ".*");

                        // Delete the existing document.
                        foreach (string existingFile in existingFiles)
                        {
                            System.IO.File.Delete(existingFile);
                        }

                        // Get extension of newly uploaded document.
                        string extension =
                            Path.GetExtension(file.FileName);

                        // Save new document using the same document name.
                        string newFilePath =
                            Path.Combine(
                                uploadPath,
                                documentName + extension);

                        file.SaveAs(newFilePath);
                    };

                // ---------------------------------------------------------
                // UPDATE LOAN APPLICATION IN NAV
                // ---------------------------------------------------------

                using (var client = new ProxyClient())
                {
                    string response =
                        client.ModifyLoanApplication(
                            loanId,
                            loanType,
                            requestedAmount);

                    switch (response)
                    {
                        // -------------------------------------------------
                        // SUCCESS
                        // -------------------------------------------------

                        case "TRUE":

                            // NAV was successfully updated.
                            // Now replace ONLY documents that were uploaded.

                            replaceDocument(
                                Payslip1,
                                "FirstPayslip");

                            replaceDocument(
                                Payslip2,
                                "SecondPayslip");

                            replaceDocument(
                                Payslip3,
                                "ThirdPayslip");

                            replaceDocument(
                                IDCopy,
                                "IDCopy");

                            replaceDocument(
                                KRAPin,
                                "KRAPin");

                            replaceDocument(
                                CollateralCopy,
                                "Collateral");

                            TempData["SuccessMessage"] =
                                "Loan application modified successfully.";

                            return RedirectToAction(
                                "LoanApplicationDetails",
                                new { loanid = loanId });


                        // -------------------------------------------------
                        // APPLICATION NOT FOUND
                        // -------------------------------------------------

                        case "NOTFOUND":

                            TempData["ErrorMessage"] =
                                "Loan application was not found.";

                            break;


                        // -------------------------------------------------
                        // APPLICATION CANNOT BE MODIFIED
                        // -------------------------------------------------

                        case "NOTALLOWED":

                            TempData["ErrorMessage"] =
                                "This loan application can no longer be modified.";

                            break;


                        // -------------------------------------------------
                        // INVALID AMOUNT
                        // -------------------------------------------------

                        case "INVALIDAMOUNT":

                            TempData["ErrorMessage"] =
                                "Please enter a valid loan amount.";

                            break;


                        // -------------------------------------------------
                        // INVALID LOAN TYPE
                        // -------------------------------------------------

                        case "NOLOANTYPE":

                            TempData["ErrorMessage"] =
                                "The selected loan product could not be found.";

                            break;


                        // -------------------------------------------------
                        // OTHER ERROR
                        // -------------------------------------------------

                        default:

                            TempData["ErrorMessage"] =
                                "Failed to modify loan application: " + response;

                            break;
                    }
                }

                // ---------------------------------------------------------
                // RETURN TO APPLICATION DETAILS
                // ---------------------------------------------------------

                return RedirectToAction(
                    "LoanApplicationDetails",
                    new { loanid = loanId });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                TempData["ErrorMessage"] =
                    "An error occurred while modifying your loan application."+ ex;

                return RedirectToAction(
                    "LoanApplicationDetails",
                    new { loanid = loanId });
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