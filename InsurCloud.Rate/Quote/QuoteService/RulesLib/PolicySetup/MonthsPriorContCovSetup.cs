using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.PolicySetup
{
    public static class MonthsPriorContCovSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            if (pol.PolicyTermTypeInd != null)
            {
                if (pol.PolicyTermTypeInd.ToUpper().Trim() == "R")
                {
                    if (pol.PolicyInsured.MonthsPriorContCov < 1)
                    {
                        pol.PolicyInsured.MonthsPriorContCov = 6;
                    }
                }
            }
            return pol.PolicyInsured.MonthsPriorContCov.ToString();
        }
    }
}
