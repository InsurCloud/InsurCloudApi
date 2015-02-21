using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using CorPolicy;
using Helpers;

namespace RulesLib.PolicySetup
{
    public static class UWTierSetup
    {

        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            
            
            string SQL = "";

            SQL = " SELECT Tier FROM pgm" + pol.Product + pol.StateCode + "..CodeUWTiers with(nolock)";
            SQL +=  " WHERE Program = @Program ";
            SQL +=  " AND EffDate <= @RateDate ";
            SQL +=  " AND ExpDate > @RateDate ";
            SQL +=  " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL +=  " AND PriorInsurance = @PriorInsurance ";
            SQL +=  " AND PriorLimits = @PriorLimits ";
            SQL +=  " AND ContCov IN ( @ContCov , 99 ) ";
            SQL +=  " ORDER BY Tier Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1,  pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@PriorInsurance", SqlDbType.VarChar, 3, pol.PolicyInsured.DaysLapse));
            parms.Add(DBHelper.AddParm("@PriorLimits", SqlDbType.VarChar, 3, pol.PolicyInsured.PriorLimitsCode));
            parms.Add(DBHelper.AddParm("@ContCov", SqlDbType.VarChar, 3, pol.PolicyInsured.MonthsPriorContCov >= 6 ? "1" : "0"));

            pol.PolicyInsured.UWTier = DBHelper.GetScalarValue(SQL, "Tier", connectionString, parms);
            return pol.PolicyInsured.UWTier;
        }
    }
}
