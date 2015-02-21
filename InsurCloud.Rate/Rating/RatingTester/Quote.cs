using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Rate.Models
{
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Quote
    {
        [DataMember]
        public string QuoteID { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string StatusMessage { get; set; }
        [DataMember]
        public DateTime RateDate { get; set; }
        [DataMember]
        public DateTime EffectiveDate { get; set; }
        [DataMember]
        public Int32 Term { get; set; }
        [DataMember]
        public List<QuoteOption> Options { get; set; }

    }
    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class QuoteOption
    {
        [DataMember]
        public int OptionNumber { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
        [DataMember]
        public string OptionName { get; set; }
        [DataMember]
        public string OptionDescription { get; set; }
        [DataMember]
        public string ImageURL { get; set; }
        [DataMember]
        public string ActionURL { get; set; }
        [DataMember]
        public Decimal FullTermPremium { get; set; }
        [DataMember]
        public Decimal TotalPolicyFees { get; set; }
        [DataMember]
        public Decimal DownPaymentAmt { get; set; }
        [DataMember]
        public int NumberOfPayments { get; set; }
        [DataMember]
        public Decimal InstallmentAmount { get; set; }
        [DataMember]
        public Decimal InstallmentFeeAmount { get; set; }
        [DataMember]
        public Decimal TotalDiscountAmount { get; set; }
        [DataMember]
        public List<Discount> Discounts { get; set; }
    }

    [DataContract(Namespace = "com.insurcloud/2014/07/Rate.Models")]
    public class Discount
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Decimal Amount { get; set; }
    }

}