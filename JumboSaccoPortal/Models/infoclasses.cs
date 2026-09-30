using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace JumboSaccoPortal.Models
{
    public class infoclasses
    {
        public class UserDetails
        {
            [JsonPropertyName("MemberNumber")]
            public string MemberNumber { get; set; }

            [JsonPropertyName("PayrollNumber")]
            public string PayrollNumber { get; set; }

            [JsonPropertyName("Name")]
            public string Name { get; set; }

            [JsonPropertyName("Email")]
            public string Email { get; set; }

            [JsonPropertyName("PhoneNumber")]
            public string PhoneNumber { get; set; }

            [JsonPropertyName("IDNumber")]
            public string IDNumber { get; set; }

            [JsonPropertyName("Gender")]
            public string Gender { get; set; }

            [JsonPropertyName("DateOfBirth")]
            public string DateOfBirth { get; set; }

            [JsonPropertyName("EmployerCode")]
            public string EmployerCode { get; set; }

            [JsonPropertyName("AccountCategory")]
            public string AccountCategory { get; set; }

            [JsonPropertyName("BankName")]
            public string BankName { get; set; }

            [JsonPropertyName("BankAccountNo")]
            public string BankAccountNo { get; set; }

            [JsonPropertyName("BankBranchCode")]
            public string BankBranchCode { get; set; }

            [JsonPropertyName("SharesCapitalAmount")]
            public string SharesCapitalAmount { get; set; }

            [JsonPropertyName("TotalDepositsAmount")]
            public string TotalDepositsAmount { get; set; }

            [JsonPropertyName("NormalDepositsAmount")]
            public string NormalDepositsAmount { get; set; }

            [JsonPropertyName("HolidaySavingsAmount")]
            public string HolidaySavingsAmount { get; set; }

            [JsonPropertyName("HousingSavingsAmount")]
            public string HousingSavingsAmount { get; set; }

            [JsonPropertyName("SchoolFeesAmount")]
            public string SchoolFeesAmount { get; set; }

            [JsonPropertyName("SpecialsAmount")]
            public string SpecialsAmount { get; set; }

            [JsonPropertyName("FreeSharestoSelf")]
            public string FreeSharestoSelf { get; set; }

            [JsonPropertyName("FreeSharestoOthers")]
            public string FreeSharestoOthers { get; set; }

            [JsonPropertyName("UnallocatedAmount")]
            public string UnallocatedAmount { get; set; }

            [JsonPropertyName("DividendAmount")]
            public string DividendAmount { get; set; }

            [JsonPropertyName("LoanBalanceAmount")]
            public string LoanBalanceAmount { get; set; }
            [JsonPropertyName("LoanInterestAmount")]
            public string LoanInterestAmount { get; set; }
            [JsonPropertyName("MaxLoanEligibility")]
            public string MaxLoanEligibility { get; set; }
            [JsonPropertyName("CollateralValue")]
            public string CollateralValue { get; set; }
            [JsonPropertyName("CollateralCommittedValue")]
            public string CollateralCommittedValue { get; set; }

            // Add this property to hold the list of ministatements
            [JsonPropertyName("UserMinistatements")]
            public List<UserMinistatement> UserMinistatements { get; set; }
        }

        public class UserMinistatement
        {
            [JsonPropertyName("Description")]
            public string Description { get; set; }

            [JsonPropertyName("TransactionAmount")]
            public string TransactionAmount { get; set; }

            [JsonPropertyName("TransactionDate")]
            public string TransactionDate { get; set; }
        }

        public class RootLoanResponse
        {
            public List<Loan> Loans { get; set; }
        }

        public class OnlineLoanApplication
        {
            public string LoanAppID { get; set; }
            public string RequestedAmount { get; set; }
            public string Status { get; set; }
            public string RequestDate { get; set; }
            public string LastUpdateDate { get; set; }
            public string LoanType { get; set; }
            public string TotalGuaranteed { get; set; }
            public List<OnlineGuarantor> OnlineGuarantors { get; set; }
        }

        public class OnlineGuarantor
        {
            public string LoanAppID { get; set; }
            public string GuarantorName { get; set; }
            public string LoanAmount { get; set; }
            public string GuarantorshipStatus { get; set; }
            public string ApprovedAmount { get; set; }
        }

        public class LoanApiResponse
        {
            public List<OnlineLoanApplication> OnlineLoansApps { get; set; }
        }

        public class LoanStatusGroups
        {
            public List<OnlineLoanApplication> NewLoans { get; set; } = new List<OnlineLoanApplication>();
            public List<OnlineLoanApplication> PendingLoans { get; set; } = new List<OnlineLoanApplication>();
            public List<OnlineLoanApplication> ApprovedLoans { get; set; } = new List<OnlineLoanApplication>();
            public List<OnlineLoanApplication> RejectedLoans { get; set; } = new List<OnlineLoanApplication>();
        }


        public class Loan
        {
            public string LoanCode { get; set; }
            public string OutstandingBalance { get; set; }
            public string LoanProductCode { get; set; }
            public string LoanProductName { get; set; }
            public string LoanSASRAStatus { get; set; }
            public string LoanStatus { get; set; }
            public string Installments { get; set; }
            public string RemainingInstallments { get; set; }
            public string RequestedAmount { get; set; }
        }



        public class LoanCalculatorTypes
        {
            public List<LoanType> LoanTypes { get; set; }
        }

        public class  LoanType
        {
            public string LoanCode { get; set; }
            public string LoanDescription { get; set; }
            public string LoanInterest { get; set; }
            public string LoanInstallments { get; set; }
            public string LoanRepaymentMethod { get; set; }
            public int LoanMultiplier { get; set; }
        }
        public class NextofKinsDetails
        {
            [JsonPropertyName("NextofKins")] // Important for matching JSON property name
            public List<NextofKin> NextofKins { get; set; }
        }

        public class NextofKin
        {
            [JsonPropertyName("KinName")]
            public string KinName { get; set; }

            [JsonPropertyName("KinRelationship")]
            public string KinRelationship { get; set; }

            [JsonPropertyName("Address")]
            public string Address { get; set; }

            [JsonPropertyName("Allocation")]
            public string Allocation { get; set; }

            [JsonPropertyName("EntryNumber")]
            public string EntryNumber { get; set; }
        }

        public class GuarantorInfo
        {
            public string GuarantorID { get; set; }
            public string GuarantorName { get; set; }
        }

        public class MemberGuarantorships
        {
            public List<Guarantorship> Guarantorships { get; set;}
        }

        public class Guarantorship
        {
            public string GuarantorshipID { get; set; }
            public string RequestorName { get; set; }
            public string RequestedAmount { get; set; }
            public string Status { get; set; }
            public string GuaranteedAmount { get; set; }
            public string RequestedDate { get; set; }
        }

        public class GuarantorshipDetails
        {
            public string GuarantorshipID { get; set; }
            public string RequestorName { get; set; }
            public string LoanAmount { get; set; }
            public string Status { get; set; }
            public string GuarantorshipAmount { get; set; }
            public string LoanRequestDate { get; set; }
            public string GuarantorshipRequestDate { get; set; }
            public string GuarantorshipAppovalDate { get; set; }
            public string TotalGuaranteed { get; set; }
            public string MemberMaximumAbility { get; set; }
        }
    }
}