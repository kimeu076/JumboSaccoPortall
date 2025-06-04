using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JumboSaccoPortal.Models;
using static JumboSaccoPortal.Models.infoclasses;

namespace JumboSaccoPortal.Controllers
{
    public class StatementsController : Controller
    {
        // GET: Statements
        public ActionResult Index()
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;
            using (var proxyClient = new ProxyClient())
            {
                string fosaaccountsstring = proxyClient.MemberStatementLoans(membernumber);

                List<MemberLoansModel> memberloans = ParseMemberLoans(fosaaccountsstring);

                ViewBag.MemberLoans = memberloans;
                return View();
            }
        }

        private List<MemberLoansModel> ParseMemberLoans(string memberloanstsring)
        {
            return memberloanstsring.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(fosaaccount =>
                {
                    return new MemberLoansModel
                    {
                        LoanCode = fosaaccount
                    };
                })
                .ToList();
        }

        [HttpPost]
        public ActionResult GetFOSAPdf(StatementRequest request)
        {
            if (Session["memberno"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            var membernumber = Session["memberno"] as string;

            if (request == null || string.IsNullOrEmpty(request.StatementType))
            {
                return new HttpStatusCodeResult(400, "Invalid input");
            }

            if (string.IsNullOrEmpty(membernumber))
            {
                return new HttpStatusCodeResult(403, "Member is not logged in");
            }

            if (request.StatementType == "MSTMT")
            {
                try
                {
                    string starttime = request.StartDate.ToString("MM/dd/yyyy");
                    string endtime = request.EndDate.ToString("MM/dd/yyyy");
                    string base64Pdf = string.Empty;

                    using (var proxyClient = new ProxyClient())
                    {
                        base64Pdf = proxyClient.MemberStatement(membernumber, starttime, endtime);
                    }

                    if (string.IsNullOrEmpty(base64Pdf))
                    {
                        return new HttpStatusCodeResult(404, "No PDF found");
                    }

                    return Json(new { base64Pdf });
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(500, "Something went wrong: " + ex.Message);
                }
            }

            else if (request.StatementType == "LGRD")
            {
                try
                {
                    string base64Pdf = string.Empty;

                    using (var proxyClient = new ProxyClient())
                    {
                        base64Pdf = proxyClient.LoanGuaranteedReport(membernumber);
                    }

                    if (string.IsNullOrEmpty(base64Pdf))
                    {
                        return new HttpStatusCodeResult(404, "No PDF found");
                    }

                    return Json(new { base64Pdf });
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(500, "Something went wrong: " + ex.Message);
                }
            }

            else if (request.StatementType == "LGRS")
            {
                try
                {
                    string base64Pdf = string.Empty;

                    using (var proxyClient = new ProxyClient())
                    {
                        base64Pdf = proxyClient.LoanGuarantorsReport(membernumber);
                    }

                    if (string.IsNullOrEmpty(base64Pdf))
                    {
                        return new HttpStatusCodeResult(404, "No PDF found");
                    }

                    return Json(new { base64Pdf });
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(500, "Something went wrong: " + ex.Message);
                }
            }

            else if (request.StatementType == "LOANSTMT")
            {
                try
                {
                    string base64Pdf = string.Empty;

                    using (var proxyClient = new ProxyClient())
                    {
                        base64Pdf = proxyClient.LoansStatement(membernumber);
                    }

                    if (string.IsNullOrEmpty(base64Pdf))
                    {
                        return new HttpStatusCodeResult(404, "No PDF found");
                    }

                    return Json(new { base64Pdf });
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(500, "Something went wrong: " + ex.Message);
                }
            }

            else if (request.StatementType == "REPAYSCH")
            {
                try
                {
                    string base64Pdf = string.Empty;

                    using (var proxyClient = new ProxyClient())
                    {
                        base64Pdf = proxyClient.LoanRepaymentSchedule(request.FOSAAccount);
                    }

                    if (string.IsNullOrEmpty(base64Pdf))
                    {
                        return new HttpStatusCodeResult(404, "No PDF found");
                    }

                    return Json(new { base64Pdf });
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(500, "Something went wrong: " + ex.Message);
                }
            }

            return new HttpStatusCodeResult(400, "Invalid Statement Type");
        }
    }
    public class StatementRequest
    {
        public string StatementType { get; set; }
        public string FOSAAccount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MemberLoansModel
    {
        public string LoanCode { get; set; }
    }
}