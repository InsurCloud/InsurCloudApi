using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using USPSAddress;

namespace InsurCloud.Auth.Api.Models
{ 
    public class NewQuoteRequest
    {
        public NewQuoteRequest()
        {
            Insured = new NewQuoteInsured();
            ZipCode = new ZipCode();
        }
        public string PostalCode { get; set; }
        public ZipCode ZipCode { get; set; }
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