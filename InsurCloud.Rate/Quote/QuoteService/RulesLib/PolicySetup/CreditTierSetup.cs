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
    public static class CreditTierSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            string SQL = "";

            SQL = " SELECT CreditTier FROM pgm" + pol.Product + pol.StateCode + ".." + "CodeCreditTiers with(nolock)";
            SQL +=  " WHERE Program = @Program ";
            SQL +=  " AND EffDate <= @RateDate ";
            SQL +=  " AND ExpDate > @RateDate ";
            SQL +=  " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL +=  " AND MinScore <= @CreditScore ";
            SQL +=  " AND MaxScore >= @CreditScore ";
            SQL +=  " AND AgeStart <= @Age ";
            SQL +=  " AND AgeEnd > @Age ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));            
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@CreditScore", SqlDbType.Int, 22, pol.PolicyInsured.CreditScore));
            parms.Add(DBHelper.AddParm("@Age", SqlDbType.Int, 22, pol.PolicyInsured.Age));

            pol.PolicyInsured.CreditTier = DBHelper.GetScalarValue(SQL, "CreditTier", connectionString, parms);
            return pol.PolicyInsured.CreditTier;
        }
    }
}
