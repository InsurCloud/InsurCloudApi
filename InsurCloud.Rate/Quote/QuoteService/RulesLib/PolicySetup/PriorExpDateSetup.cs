using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.PolicySetup
{
    public static class PriorExpDateSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            if (pol.Type.ToUpper() == "RENEWAL")
            {
                if (pol.PolicyInsured.PriorExpDate == DateTime.MinValue)
                {
                    pol.PolicyInsured.PriorExpDate = pol.EffDate;
                }
            }
            return pol.PolicyInsured.PriorExpDate.ToString();
        }
    }
}
