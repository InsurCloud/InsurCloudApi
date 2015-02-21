using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using CorPolicy;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace RulesLib.PolicySetup
{
    public static class ViolationPointsSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {

            foreach (clsEntityDriver drv in pol.Drivers)
            {
                ViolationHelper.CheckViolations(pol, drv, stateInfo, connectionString);
            }
            return "";

        }
    }
}
