using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.PolicySetup
{
    public static class DaysLapseSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            if (pol.PolicyInsured.PriorLimitsCode == "0")
            {
                pol.PolicyInsured.DaysLapse = 0;
            }
            else
            {
                int days = pol.EffDate.Subtract(pol.PolicyInsured.PriorExpDate).Days;
                if (days <= 7)
                {
                    pol.PolicyInsured.DaysLapse = 2;
                }
                else if (days <= 30)
                {
                    pol.PolicyInsured.DaysLapse = 1;
                }
                else
                {
                    pol.PolicyInsured.DaysLapse = 0;
                    pol.PolicyInsured.PriorLimitsCode = "0";
                }

            }
            return pol.PolicyInsured.DaysLapse.ToString();
        }
    }
}
