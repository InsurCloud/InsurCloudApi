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
        public DateTime RateDate { get; set; }
        public List<QuoteVehicle> Vehicles {get; set;}
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
    }

    public class QuoteDriver
    {
        public int Number { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime dob { get; set; }
        public bool Primary { get; set; }
        public string Gender { get; set; }
        public string RelationToInsured { get; set; }
        public string MaritalStatus { get; set; }
    }

    public class BasicRate
    {
        public ProgramInfo Program { get; set; }
        public string CoverageLevel { get; set; }
        public float Premium { get; set; }
        public float Fees { get; set; }        
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

    public class Installment
    {
        public int InstallmentNumber { get; set; }
        public float Premium { get; set; }
        public float InstallmentFee { get; set; }
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
}