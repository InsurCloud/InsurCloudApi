using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.PolicySetup
{
    public static class PriorLimitsSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            if (pol.PolicyInsured.PriorLimitsCode == "0" && pol.PriorCarrierName.ToUpper().Contains("IMPERIAL"))
            {
                if (pol.PriorCarrierName.ToUpper() == "IMPERIAL MONTHLY")
                {
                    pol.PolicyInsured.PriorLimitsCode = "0";
                }
                else
                {
                    pol.PolicyInsured.PriorLimitsCode = "1";
                    PriorExpDateSetup.Execute(ref pol, connectionString, stateInfo);
                }                
            }
            return pol.PolicyInsured.PriorLimitsCode;
        }
    }
}
