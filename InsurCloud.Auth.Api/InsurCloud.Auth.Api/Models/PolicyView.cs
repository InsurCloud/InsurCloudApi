using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InsurCloud.Auth.Api.Models
{
    public class PolicyView
    {
        public PolicyView()
        {
            HouseholdMembers = new List<HouseholdMember>();
            CoveredUnits = new List<ICoveredUnit>();
            Company = new CompanyView();
            PrimaryNamedInsured = new HouseholdMember();
            Producer = new ProducerView();
        }
        public string PolicyUniqueId { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string PolicyTermType { get; set; }
        public string PayPlan { get; set; }
        public CompanyView Company { get; set; }
        public string PolicyStatus { get; set; }
        public HouseholdMember PrimaryNamedInsured { get; set; }
        public HouseholdMember JointNamedInsured { get; set; }
        public List<HouseholdMember> HouseholdMembers { get; set; }
        public List<ICoveredUnit> CoveredUnits { get; set; }
        public ProducerView Producer { get; set; }
        public List<Lienholder> AdditionalInsureds { get; set; }
    }
    public class PolicyNote
    {
        public string UniqueID { get; set; }
        public int TransactionNumber { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime EntryDate { get; set; }
        public string NoteText { get; set; }
        public string NoteType { get; set; }
        public string SourceCode { get; set; }
        public string EnteredByUserName { get; set; }
    }
    public class ProducerView
    {
        public ProducerView()
        {
            ContactInfo = new ContactView();
            AgencyInfo = new AgencyLocationView();
        }
        public string ProducerUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ContactView ContactInfo { get; set; }
        public AgencyLocationView AgencyInfo { get; set; }
    }
    public class AgencyLocationView {
        public AgencyLocationView()
        {
            ContactInfo = new List<ContactView>();
        }
        public Int64 AgencyId { get; set; }
        public string DisplayName { get; set; }
        public string ImageURL { get; set; }
        public List<ContactView> ContactInfo { get; set; }
    }   
    public class CompanyView
    {
        public CompanyView()
        {
            ContactInfo = new List<ContactView>();
        }
        public string CarrierName { get; set; }
        public string Program { get; set; }
        public string ProductLine { get; set; }
        public string Product { get; set; }
        public string CompanyName { get; set; }
        public string ImageURL { get; set; }
        public List<ContactView> ContactInfo { get; set; }
    }
    public class ContactView
    {
        public ContactView()
        {
            Address = new Address();
        }
        public string ContactType { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }        
    }
    public class Address
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string County { get; set; }
    }
    public class HouseholdMember
    {
        public HouseholdMember()
        {
            ContactInfo = new ContactView();
            Violations = new List<Violation>();
        }
        public int IndexNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ContactView ContactInfo { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string RelationToInsured { get; set; }
        public string MaritalStatus { get; set; }
        public string Gender { get; set; }
        public int RatedAge { get; set; }
        public string SetupForENotification { get; set; } //Yes or No on Insured/Self only
        public List<Violation> Violations { get; set; }
    }
    public class Violation
    {
        public int IndexNumber { get; set; }
        public int Points { get; set; }
        public string ViolationDescription { get; set; }
        public DateTime ViolationDate { get; set; }
    }
    public interface ICoveredUnit {
        List<Coverage> Coverages();
    }
    public class CoveredUnit : ICoveredUnit
    {
        private List<Coverage> _coverages = new List<Coverage>();

        public CoveredUnit()
        {
            Address = new Address();
            Lienholders = new List<Lienholder>();
        }
        public int IndexNumber { get; set; }
        public string Description { get; set; }        
        public Address Address { get; set; } //Propery Address for Homeowners, Vehicle Garaging Address for Auto        
        public List<Lienholder> Lienholders { get; set; }

        public List<Coverage> Coverages()
        { 
            return _coverages;
        }
    }
    public class PersonalAutoCoveredUnit : CoveredUnit{
        public string VIN { get; set; } 
        public double StatedAmount { get; set; }
        public int ModelYear { get; set; }
        public string Make {get; set;}
        public string Model {get; set; }
        public string BodyStyle {get; set;}
        public int AssignedDriverNumber {get; set;}
    }
    public class HomeownersCoveredUnit : CoveredUnit
    {
        public int ConstructionYear { get; set; }
        public string ConstructionType { get; set; }
        public string SidingType { get; set; }
        public string BuildingType { get; set; }
        public string FireDepartment { get; set; }
        public string OccupancyType { get; set; }
        public double ReplacementCost { get; set; }
        public double DwellingAmount { get; set; }
        public double ContentsAmount { get; set; }
        public double OtherStructureAmount { get; set; }
    }
    public class Lienholder
    {
        public Lienholder()
        {
            ContactInfo = new ContactView();
        }
        public int IndexNumber { get; set; }
        public string LienholderType { get; set; }  //Additional insured, Lienholder, Mortgagee
        public ContactView ContactInfo { get; set; }
        public string AccountNumber { get; set; }
    } 
    public class Coverage
    {
        public int IndexNumber { get; set; }
        public string CoverageCode { get; set; }
        public string CovGroupName { get; set; }
        public string CovGroupAbbr { get; set; }
        public string LimitDeductibleDescription { get; set; }
        public double WrittenPremium { get; set; }
    }
    public class BillingInfo
    {
        public BillingInfo()
        {
            PaymentHistory = new List<Payment>();
        }
        public double RemainingBalance { get; set; }
        public DateTime LastPaymentReceivedDate { get; set; }
        public double LastPaymentReceivedAmount { get; set; }
        public DateTime CurrentDueDate { get; set; }
        public double CurrentDueAmount { get; set; }
        public double PastDueAmount { get; set; }
        public int DaysPastDue { get; set; }
        public string AlertMessage { get; set; }
        public List<Payment> PaymentHistory { get; set;}
    }
    public class Payment
    {
        public string PaymentUniqueID { get; set; }
        public int IndexNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PaymentType { get; set; }
        public double Amount { get; set; }
    }
    public class PolicyDocument
    {
        public string DocumentUniqueID { get; set; }
        public string DocumentType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime PrintedDate { get; set; }
    }
}