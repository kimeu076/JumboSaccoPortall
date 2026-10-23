using JumboSaccoPortal.ProxyReference;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

namespace JumboSaccoPortal.Models
{

    public class ProxyClient : IDisposable
    {
        private ProxyService _client;

        public ProxyClient()
        {
            InitializeClient();
        }
        public class CustomProxyService : ProxyService
        {
            private readonly string _authorizationHeader;

            public CustomProxyService(string username, string password, string endpointUrl)
            {
                _authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                Url = endpointUrl;
            }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                var request = (HttpWebRequest)base.GetWebRequest(uri);

                request.Headers.Add("Authorization", $"Basic {_authorizationHeader}");

                return request;
            }
        }


        private void InitializeClient()
        {
            string endpointUrl = ConfigurationManager.AppSettings["ProxyServiceEndpoint"];
            string username = ConfigurationManager.AppSettings["APPUsername"];
            string password = ConfigurationManager.AppSettings["APPPassword"];

            _client = new CustomProxyService(username, password, endpointUrl);
        }


        public string DashboardInfo(string membid)
        {
            try
            {
                return _client.DashboardInfo(membid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string MemberStatementLoans(string membid)
        {
            try
            {
                return _client.MemberStatementLoans(membid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string MemberLoans(string membid)
        {
            try
            {
                return _client.MemberLoans(membid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string LoanUserDetails(string membid)
        {
            try
            {
                return _client.LoanUserDetails(membid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        
        public string LoanCalculatorLoanTypes()
        {
            try
            {
                return _client.LoanTypesCalculator("nothing");
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string LoanCalculatorLoanCharges(string loanProductType, decimal loanAmount)
        {
            try
            {
                Debug.WriteLine("==============================================");
                Debug.WriteLine("LOAN CHARGES - PROXY START");
                Debug.WriteLine("==============================================");

                Debug.WriteLine("Loan Product Type: " + loanProductType);
                Debug.WriteLine("Loan Amount: " + loanAmount);

                Debug.WriteLine("----------------------------------------------");
                Debug.WriteLine("Calling _client.LoanChargesCalculator...");
                Debug.WriteLine("----------------------------------------------");

                string response = _client.LoanChargesCalculator(
                    loanProductType,
                    loanAmount
                );

                Debug.WriteLine("----------------------------------------------");
                Debug.WriteLine("WEB SERVICE CALL SUCCESSFUL");
                Debug.WriteLine("Response:");
                Debug.WriteLine(response);
                Debug.WriteLine("----------------------------------------------");

                Debug.WriteLine("LOAN CHARGES - PROXY END");
                Debug.WriteLine("==============================================");

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("==============================================");
                Debug.WriteLine("LOAN CHARGES - PROXY ERROR");
                Debug.WriteLine("==============================================");

                Debug.WriteLine("Exception Type:");
                Debug.WriteLine(ex.GetType().FullName);

                Debug.WriteLine("Exception Message:");
                Debug.WriteLine(ex.Message);

                Debug.WriteLine("Exception Details:");
                Debug.WriteLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    Debug.WriteLine("----------------------------------------------");
                    Debug.WriteLine("INNER EXCEPTION:");
                    Debug.WriteLine(ex.InnerException.ToString());
                }

                Debug.WriteLine("==============================================");

                // IMPORTANT: preserve the original exception
                throw;
            }
        }


        //Statements
        public string MemberStatement(string membid, string startdate, string enddate)
        {
            try
            {
                return _client.MemberStatement(membid, startdate, enddate);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string LoanGuaranteedReport(string membid)
        {
            try
            {
                return _client.LoanGuaranteedReport(membid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string LoanGuarantorsReport(string membid)
        {
            try
            {
                return _client.LoanGuarantorsReport(membid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string LoansStatement(string membid)
        {
            try
            {
                return _client.LoansStatement(membid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string LoanRepaymentSchedule(string loannumber)
        {
            try
            {
                return _client.LoanRepaymentSchedule(loannumber);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string MemberLogin(string username, string password)
        {
            try
            {
                return _client.MemberLogin(username, password);
            }
            catch (Exception ex)
            {
                 throw new Exception("Login failed: " + ex.Message, ex);
            }
        }

        public string ConformLoginOTP(string username, string otp)
        {
            try
            {
                return _client.ConfirmLoginOTP(username, otp);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string ConfirmResetOTP(string username, string otp)
        {
            try
            {
                return _client.ConfirmResetOTP(username, otp);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string PasswordResetInitiation(string username)
        {
            try
            {
                return _client.PasswordResetInitiation(username);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string UpdateNewPasword(string username, string password)
        {
            try
            {
                return _client.UpdateNewPasword(username, password);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string ResetPasswordOnPortal(string username, string oldpassword, string newpassword)
        {
            try
            {
                return _client.ResetPasswordOnPortal(username, oldpassword, newpassword);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string NextOfkinDetails(string username)
        {
            try
            {
                return _client.NextOfkinDetails(username);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string MemberApplication(
            string firstName, string middleName, string lastName, string payrollNo,
            string address, string postalCode, string town, string countryRegionCode,
            string county, string mobilePhoneNo, string secondaryMobileNo, string email,
            DateTime dateOfBirth, string idNo, string passportNo, string kraPin,
            string gender, string maritalStatus, string howDidYouKnow,
            string recruitedBy, string recruiterName, string applicationCategory,
            string employerCode, string workstation, string department)
        {
            try
            {
                return _client.MemberApplication(
                    firstName, middleName, lastName, payrollNo, address, postalCode,
                    town, countryRegionCode, county, mobilePhoneNo, secondaryMobileNo,
                    email, dateOfBirth, idNo, passportNo, kraPin, gender, maritalStatus,
                    howDidYouKnow, recruitedBy, recruiterName, applicationCategory,
                    employerCode, workstation, department);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public string EmployerList()
        {
            try
            {
                return _client.EmployerList();
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string PostCodeList()
        {
            try
            {
                return _client.PostCodeList();
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string OnlineLoanApplications(string username)
        {
            try
            {
                return _client.GetMemberOnlineLoanApplications(username);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }

        public string GetLoanApplicationDetails(int loanid)
        {
            try
            {
                return _client.GetLoanApplicationDetails(loanid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string RemoveGuarantorByRequestor(int guarantorshipid)
        {
            try
            {
                return _client.RemoveGuarantorByRequestor(guarantorshipid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string SearchGuarantor(string guarantorid)
        {
            try
            {
                return _client.SearchGuarantor(guarantorid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string AddGuarantorToLoan(int loanid, string guarantorid)
        {
            try
            {
                return _client.AddGuarantor(loanid, guarantorid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string StartLoanApplication(string memberid, string loantype, decimal requestedamount, string foldername)
        {
            try
            {
                return _client.StartLoanApplications(memberid, loantype, requestedamount, foldername);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
                //throw new Exception(ex.Message);
            }
        }
        public string ModifyLoanApplication(
    int loanapp,
    string loantype,
    decimal requestedamount)
        {
            try
            {
                return _client.ModifyLoanApplication(
                    loanapp,
                    loantype,
                    requestedamount);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string GetMemberGuarantorships(string memberid)
        {
            try
            {
                return _client.GetMemberGuarantorships(memberid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        
        public string GetGuarantorshipDetails(int guarantorshipid)
        {
            try
            {
                return _client.GuarantroshipDetails(guarantorshipid);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string StartGuarantorshipApproval(int guarantorshipid, string memberid, decimal amount)
        {
            try
            {
                return _client.StartGuarantorshipApproval(guarantorshipid, memberid, amount);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }
        public string CompleteGuarantorshipApproval(int guarantorshipid, string memberid, string otp)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("===== CompleteGuarantorshipApproval =====");
                System.Diagnostics.Debug.WriteLine("Guarantorship ID: " + guarantorshipid);
                System.Diagnostics.Debug.WriteLine("Member ID/No: " + memberid);
                System.Diagnostics.Debug.WriteLine("OTP: " + otp);

                string response = _client.CompleteGuarantorshipApproval(
                    guarantorshipid,
                    memberid,
                    otp
                );

                System.Diagnostics.Debug.WriteLine("BC Response: " + response);

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "CompleteGuarantorshipApproval ERROR:"
                );

                System.Diagnostics.Debug.WriteLine(ex.ToString());

                // TEMPORARY: preserve the real error while debugging
                throw;
            }
        }
        public string RejectGuarantorship(int guarantorshipid, string memberid, string comment)
        {
            try
            {
                return _client.RejectGuarantorship(guarantorshipid, memberid, comment);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong. Please try again later.");
            }
        }


        public void Dispose()
        {
            if (_client != null)
            {
                try
                {
                    _client.Abort();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error disposing NAV client: " + ex.Message, ex);
                }
            }
        }
    }
}