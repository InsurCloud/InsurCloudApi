using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InsurCloud.Auth.Api.Models
{
    public class PolicySearchResult
    {
        public string PolicyNumber { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string EffectiveDateFormatted { get; set; }
        public string PolicyStatus { get; set; }
        public DateTime CancellationDate { get; set; }
        public string CancellationDateFormatted { get; set; }
        public string InsuredFullName { get; set; }
        public string InsuredPhoneNumber { get; set; }
        public double CurrentAmountDue { get; set; }
        public DateTime DueDate { get; set; }
        public string DueDateFormatted { get; set; }
        public bool IsPastDue { get; set; }
    }

    
}