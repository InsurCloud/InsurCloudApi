using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.PolicySetup
{
    public static class DriverSR22Setup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.SR22)
                {
                    ViolationHelper.AddSR22Violation(pol, drv);
                }
                else
                {
                    ViolationHelper.RemoveSR22Violation(pol, drv);
                }
            }
            return "";
        }

        
    }
}
