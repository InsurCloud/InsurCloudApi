using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InsurCloud.Auth.Api.Models
{
    public class AgentSummary
    {
        public DateTime ReportDate { get; set; }
        public int QuotesStarted { get; set; }
        public int QuotesRated { get; set; }
        public int ApplicationsStarted { get; set; }
        public int PoliciesIssued { get; set; }
        public int CancelPending { get; set; }
        public int NonRenewals { get; set; }
        public int RenewalsPending { get; set; }
        public int RenewalQuotesSent { get; set; }
        public int RenewalsIssued { get; set; }
        public int NewClaimsReceived { get; set; }
        public int EndorsementsProcessed { get; set; }        
    }
}