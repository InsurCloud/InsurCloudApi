using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InsurCloud.Policy.Api.Model
{
    public class Policy
    {
        [Key]
        public Int64 PolicyId { get; set; }
        public Guid PolicyUniqueId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Program Program { get; set; }
        public Agency Agency { get; set; }
        public AgencyUser Producer { get; set; }


    }
}