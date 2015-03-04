using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InsurCloud.Auth.Api.Models
{
    public class QuoteEntryView
    {
        public string QuoteUniqueId { get; set; }
        public string QuoteStatus { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string EffectiveDateFormatted { get; set; }
        public DateTime RateDate { get; set; }
        public string RateDateFormatted { get; set; }
        public QuoteInsured Insured { get; set; }
        public List<QuoteVehicle> Vehicles {get; set;}
        public List<QuoteDriver> Drivers { get; set; }
        public QuotePolicyLevelCoverage Coverages { get; set; }
        public List<Rate> Rates { get; set; }
        public List<QuoteLienholder> Lienholders { get; set; }
    }
    public class QuotePolicyLevelCoverage
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public bool Selected { get; set; }
        public List<QuoteCoverageGrouping> policyCoverages { get; set; }
    }
    public class QuoteCoverageGrouping
    {
        public QuoteCoverageGroupingItem coverageGrouping { get; set; }
    }
    public class QuoteCoverageGroupingItem
    {
        public string Name { get; set; }
        public List<QuoteCoverage> coverages { get; set; }
    }
    public class QuoteCoverage
    {
        public string CoverageCode { get; set; }
        public string CoverageGroup { get; set; }
        public string CoverageLimit { get; set; }
    }
    public class QuoteVehicle
    {
        public int Number { get; set; }
        public VehicleOption Item { get; set; }
        public List<QuoteDriver> Drivers { get; set; }
        public string PhotoSrc { get; set; }
        public int Ownership { get; set; }
        public int PrimaryUse { get; set; }
        public int CommuteMiles { get; set; }
        public int CommuteDaysPerWeek { get; set; }
        public int MilesDriverPerYear { get; set; }
        public List<QuoteCoverage> Coverages { get; set; }
        public string GaragingZipCode { get; set; }
    }
    public class QuoteLienholder
    {
        public int Number { get; set; }
        public int VehicleNumber { get; set; }
        public string Name { get; set; }
        public QuoteAddress Address {get; set;}
        public string PhoneNumber { get; set; }
        public int LienholderType { get; set; }
    }
    public class QuoteDiscountInfo
    {
        public bool Homeowner { get; set; }
        public int ResidenceType { get; set; }
        public bool CurrentlyInsured { get; set; }
        public int ContinuousCoverageLength { get; set; }
        public int PriorCoverageLimit { get; set; } 
        public int LapseInCoverage { get; set; }
        public int PriorCarrier { get; set; }
        public double PriorRate { get; set; }
        public int CareerStatus { get; set; }
        public int Industry { get; set; }
        public int Occupation { get; set; }
        public bool GoodStudent { get; set; }
        public int EducationHighestLevel { get; set; }
    }
    public class QuoteDriverLicense
    {
        public int AgeFirstLicensed { get; set; }
        public int DriversLicenseStatus { get; set; }
        public string DriversLicenseNumber { get; set; }
        public string DriversLicenseState { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string ExpirationDateFormatted { get; set; }
    }
    public class QuoteDriver
    {
        public QuoteDriver()
        {
            MailingAddress = new QuoteAddress();
            PhysicalAddress = new QuoteAddress();
            Violations = new List<QuoteDriverViolation>();
            DiscountInfo = new QuoteDiscountInfo();
            License = new QuoteDriverLicense();
        }
        public int Number { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthDateFormatted { get; set; }        
        public bool PrimaryDriver { get; set; }
        public QuoteDriverLicense License {get; set;}
        public QuoteDiscountInfo DiscountInfo { get; set; }
        public bool IsPrimaryNamedInsured { get; set; }
        public string RelationToInsured { get; set; }        
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public bool LivesWithPrimaryNamedInsured { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public QuoteAddress PhysicalAddress { get; set; }
        public QuoteAddress MailingAddress { get; set; }
        public bool mailingSameAsPhysical { get; set; }
        public bool HasViolations { get; set; }
        public List<QuoteDriverViolation> Violations { get; set; }
        
    }
    public class QuoteDriverViolation
    {
        public int Number { get; set; }
        public string ViolationGroup { get; set; }
        public string ViolationCode { get; set; }
        public DateTime ViolationDate { get; set; }
        public string ViolationDateFormatted { get; set; }
        public string ViolationSource { get; set; }
        
    }
    public class QuoteInsured
    {
        public QuoteInsured()
        {
            Address = new QuoteAddress();
            DiscountInfo = new QuoteDiscountInfo();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public QuoteDiscountInfo DiscountInfo { get; set; }        
        public QuoteAddress Address { get; set; }
    }
    public class QuoteAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
    }
    public class Rate
    {
        public ProgramInfo Program { get; set; }
        public string CoverageLevel { get; set; }
        public double Premium { get; set; }
        public double Fees { get; set; }
        public PayPlan PayPlan { get; set; }
        public List<Installment> Installments { get; set; }
    }
    public class ProgramInfo 
    {
        public Int64 ProgramId { get; set; }
        public string CarrierName { get; set; }
        public string CompanyName { get; set; }
        public int TermMonths { get; set; }
        public string ProductLine { get; set; }
        public string Product { get; set; }
        public string Program { get; set; }
        public string StateAbbreviation { get; set; }
    }
    public class PayPlan
    {
        public PayPlan()
            : this("0:100:0.00")
        {
        }
        public PayPlan(string payPlanCode)
        {
            var items = payPlanCode.Split(':');
            PayPlanCode = payPlanCode;
            if(items[1] == "100"){
                PayPlanName = "Full Pay";
            }else{
                PayPlanName = "Installments";
            }
            DownPaymentPercent = int.Parse(items[1]);
            NumberOfInstallments = int.Parse(items[0]);
            InstallmentFeeAmount = double.Parse(items[2]);

            
        }
        public string PayPlanCode { get; set; }
        public string PayPlanName { get; set; }
        public int DownPaymentPercent { get; set; }
        public double InstallmentFeeAmount { get; set; }
        public int NumberOfInstallments { get; set; }
        public double DownPaymentAmount { get; set; }
        public double InstallmentAmount { get; set; }
        public double InstallmentPlusFeeAmount { get; set; }
    }
    public class Installment
    {
        public int InstallmentNumber { get; set; }
        public double Amount { get; set; }
        public double InstallmentFee { get; set; }
        public DateTime DueDate { get; set; }
        public string DueDateFormatted {
            get {
                return DueDate.ToString("MM/dd/yyyy");
            }
            set { 
                    //do nothing 
            }
        }
    }
    public class NewQuoteRequest
    {
        public string PostalCode { get; set; }
        public int NumberOfVehicles { get; set; }
        public int NumberOfDrivers { get; set; }
        public bool Homeowner { get; set; }
        public bool PriorCoverage { get; set; }
        public bool Married { get; set; }
        public NewQuoteInsured Insured { get; set; }
    }
    public class NewQuoteInsured
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
    }
}