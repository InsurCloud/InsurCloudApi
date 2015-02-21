using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Rate.Models
{
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Policy
    {
        [DataMember]
        public DateTime EffectiveDate { get; set; }
        [DataMember]
        public Int32 Term { get; set; }
        [DataMember]
        public PriorCoverageInfo PriorCoverageInfo { get; set; }
        [DataMember]
        public List<Coverage> PolicyLevelCoverages { get; set; }
        [DataMember]
        public List<Driver> Drivers { get; set; }
        [DataMember]
        public List<Vehicle> Vehicles { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }
        [DataMember]
        public List<Note> Notes { get; set; }

    }

    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Note
    {
        [DataMember]
        public string NoteText { get; set; }
        [DataMember]
        public string NoteType { get; set; }
        [DataMember]
        public DateTime EntryDate { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }
    }


    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public abstract class Entity
    {
        [DataMember]
        public ContactInfo ContactInfo { get; set; }
        [DataMember]
        public EntityType EntityType { get; set; }

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum EntityType
    {
        [EnumMember]
        Individual,
        [EnumMember]
        Company
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Individual : Entity
    {
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Suffix { get; set; }
        [DataMember]
        public string SSN { get; set; }
        [DataMember]
        public DateTime DOB { get; set; }
        [DataMember]
        public Gender Gender { get; set; }
        [DataMember]
        public MaritalStatus MaritalStatus { get; set; } //M, S, W


    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum Gender
    {
        [EnumMember]
        Male,
        [EnumMember]
        Female
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum MaritalStatus
    {
        [EnumMember]
        Married,
        [EnumMember]
        Single,
        [EnumMember]
        Widowed
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Corporation : Entity
    {
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string DBA { get; set; }
        [DataMember]
        public string FederalIDNo { get; set; }
        [DataMember]
        public Individual MainContact { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class ContactInfo
    {
        [DataMember]
        public List<Address> Addresses { get; set; }
        [DataMember]
        public List<PhoneNumber> PhoneNumbers { get; set; }
        [DataMember]
        public List<EmailAddress> EmailAddresses { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class EmailAddress
    {
        [DataMember]
        public EmailType EmailType { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum EmailType
    {
        [EnumMember]
        Home,
        [EnumMember]
        Work,
        [EnumMember]
        Other,
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class PhoneNumber
    {
        [DataMember]
        public PhoneType PhoneType { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public string Extension { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum PhoneType
    {
        [EnumMember]
        Home,
        [EnumMember]
        Work,
        [EnumMember]
        Cell,
        [EnumMember]
        Fax
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Address
    {
        [DataMember]
        public AddressType AddressType { get; set; }
        [DataMember]
        public string Address1 { get; set; }
        [DataMember]
        public string Address2 { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string PostalCode { get; set; }
        [DataMember]
        public string County { get; set; }

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum AddressType
    {
        [EnumMember]
        Mail,
        [EnumMember]
        Billing
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Driver : Individual
    {
        [DataMember]
        public RelationToInsured RelationToInsured { get; set; }
        [DataMember]
        public bool IsNamedInsured { get; set; } //Use this for Multiple Named Insureds
        [DataMember]
        public DriverStatus DriverStatus { get; set; }
        [DataMember]
        public DriversLicenseInfo DriversLicense { get; set; }
        [DataMember]
        public OccupancyType OccupancyType { get; set; }
        [DataMember]
        public EmploymentInfo EmploymentInfo { get; set; }
        [DataMember]
        public FinancialResponsibilityFiling FRFiling { get; set; }
        [DataMember]
        public List<Violation> Violations { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum DriverStatus
    {
        [EnumMember]
        Active,
        [EnumMember]
        LearnersPermit,
        [EnumMember]
        Excluded,
        [EnumMember]
        NotInHousehold

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class DriversLicenseInfo
    {
        [DataMember]
        public string DLN { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public LicenseStatus Status { get; set; }
        [DataMember]
        public DateTime IssuanceDate { get; set; }
        [DataMember]
        public DateTime ExpirationDate { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum LicenseStatus
    {
        [EnumMember]
        Expired,
        [EnumMember]
        IDOnly,
        [EnumMember]
        Revoked_Cancelled,
        [EnumMember]
        Suspended,
        [EnumMember]
        Valid

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class PriorCoverageInfo
    {
        [DataMember]
        public string PolicyID { get; set; }
        [DataMember]
        public string CarrierName { get; set; } //Provide a List of these
        [DataMember]
        public DateTime ExpirationDate { get; set; }
        [DataMember]
        public int MonthsContinuousCoverage { get; set; }
        [DataMember]
        public PriorLimits PriorLimits { get; set; }

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum PriorLimits
    {
        [EnumMember]
        NotApplicable = 0,
        [EnumMember]
        Under_30_60 = 1,
        [EnumMember]
        At_30_60 = 2,
        [EnumMember]
        At_50_100 = 3,
        [EnumMember]
        At_100_300_or_Higher = 4
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum OccupancyType
    {
        [EnumMember]
        OwnHome,
        [EnumMember]
        OwnMobileHomeOwnLand,
        [EnumMember]
        OwnMobileHomeDontOwnLand,
        [EnumMember]
        LiveWithParents,
        [EnumMember]
        LiveWithOthers,
        [EnumMember]
        Rent
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum RelationToInsured
    {
        [EnumMember]
        Self,
        [EnumMember]
        Spouse,
        [EnumMember]
        Child,
        [EnumMember]
        Parent,
        [EnumMember]
        Sibling,
        [EnumMember]
        Other
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Violation
    {
        [DataMember]
        public DateTime ViolationDate { get; set; }
        [DataMember]
        public DateTime ConvictionDate { get; set; }
        [DataMember]
        public string ViolationCode { get; set; }
        [DataMember]
        public string ViolationDescription { get; set; }
        [DataMember]
        public ViolationType ViolationType { get; set; }
        [DataMember]
        public bool IsAtFault { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum ViolationType
    {
        [EnumMember]
        Citation,
        [EnumMember]
        Accident,
        [EnumMember]
        Claim
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class FinancialResponsibilityFiling
    {
        [DataMember]
        public bool NeedSR22Filing { get; set; }
        [DataMember]
        public DateTime SR22Date { get; set; }
        [DataMember]
        public string CaseCode { get; set; }
        [DataMember]
        public string SR22State { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class EmploymentInfo
    {
        [DataMember]
        public string Occupation { get; set; }
        [DataMember]
        public string Employer { get; set; }
        [DataMember]
        public int YearsWithEmployer { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Vehicle
    {
        [DataMember]
        public string VIN { get; set; }
        [DataMember]
        public int ModelYear { get; set; }
        [DataMember]
        public string Make { get; set; }
        [DataMember]
        public string Model { get; set; }
        [DataMember]
        public string BodyStyle { get; set; }
        [DataMember]
        public double StatedAmount { get; set; }
        [DataMember]
        public int AnnualMilesDriven { get; set; }
        [DataMember]
        public OwnershipType OwnershipType { get; set; }
        [DataMember]
        public UsageType UsageType { get; set; }
        [DataMember]
        public VehicleType VehicleType { get; set; }
        [DataMember]
        public Address GaragingAddress { get; set; }
        [DataMember]
        public List<Lienholder> Lienholders { get; set; }
        [DataMember]
        public List<Coverage> VehicleLevelCoverages { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum VehicleType
    {
        [EnumMember]
        Camper, //= "MOUNTED_CAMPER"
        [EnumMember]
        ConventionalTrailer, // = "CONVENTIONAL_TRAVEL_TRAILER"
        [EnumMember]
        FifthWheel,//= "FIVE_WHEEL_TRAVEL_TRAILER"
        [EnumMember]
        HomemadeTrailer,//= "HOMEMADE_TRAILER"
        [EnumMember]
        HorseTrailer,//= "HORSE_TRAILER"
        [EnumMember]
        PopUpTrailer,// = "POPUP_TRAVEL_TRAILER"
        [EnumMember]
        UtilityTrailer,//= "UTILITY_TRAILER"
        [EnumMember]
        Van,//= "CONVERSION_VAN"
        [EnumMember]
        StatedAmountVehicle,//= "VEHICLE"
        [EnumMember]
        PrivatePassenger,//= "PPA"
        [EnumMember]
        Trailer //= "TRAILER"

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum OwnershipType
    {
        [EnumMember]
        Lease,//= "LEASE"
        [EnumMember]
        OwnWithPayments,// = "OWNPAY"
        [EnumMember]
        OwnNoPayments //= "OWNNOPAY"
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum UsageType
    {
        [EnumMember]
        Business,//= "BUS"
        [EnumMember]
        Commute,//= "COM"
        [EnumMember]
        Pleasure //= "PLS"
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Coverage
    {
        [DataMember]
        public CoverageGroup Group { get; set; }
        [DataMember]
        public string Limit { get; set; }
        [DataMember]
        public string Deductible { get; set; }
        [DataMember]
        public List<UnderwritingQuestion> UWQuestions { get; set; }

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum CoverageGroup
    {
        [EnumMember]
        Policy_BodilyInjury,//= "BI"
        [EnumMember]
        Policy_PropertyDamage,//= "PD"
        [EnumMember]
        Policy_UninsuredUnderinsuredMotoristBI,//= "UUMBI"
        [EnumMember]
        Policy_UninsuredUnderinsuredMotoristPD,//= "UUMPD"
        [EnumMember]
        Policy_MedicalPayments,//= "MED"
        [EnumMember]
        Policy_PersonalInjuryProtection,//= "PIP"
        [EnumMember]
        Vehicle_Comprehensive,//= "OTC"
        [EnumMember]
        Vehicle_Collision,//= "COL"
        [EnumMember]
        Vehicle_Rental,//= "REN"
        [EnumMember]
        Vehicle_Towing,//= "TOW"
        [EnumMember]
        Vehicle_SpecialEquipement //= "SPE"
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Lienholder : Corporation
    {
        [DataMember]
        public LienHolderType LienHolderType { get; set; }
        [DataMember]
        public string LoanNumber { get; set; }
        [DataMember]
        public int NumberOfInstallments { get; set; }
        [DataMember]
        public double DownPaymentAmount { get; set; }
        [DataMember]
        public string PaymentInterval { get; set; }
        [DataMember]
        public DateTime FirstPaymentDate { get; set; }
        [DataMember]
        public string PaymentMethod { get; set; }
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public enum LienHolderType
    {
        [EnumMember]
        AdditionalInsured,//= "AI"
        [EnumMember]
        LossPayee //= "LP"
    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class UnderwritingQuestion
    {
        [DataMember]
        public string QuestionCode { get; set; }
        [DataMember]
        public string QuestionText { get; set; }
        [DataMember]
        public string AnswerText { get; set; }
        [DataMember]
        public string AnswerCode { get; set; }

    }

}