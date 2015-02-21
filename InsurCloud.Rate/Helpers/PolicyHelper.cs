using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Helpers
{
    public static class PolicyHelper
    {
        public static string NormalizeCallingSystem(string callingSystem)
        {
            if (callingSystem.ToUpper().Trim() == "BRIDGE" || callingSystem.ToUpper().Trim() == "BRG" || callingSystem.ToUpper().Trim() == "EZLYNX")
            {
                return "WEBRATER";
            }
            else
            {
                if (callingSystem.ToUpper().Trim().Contains("OLE"))
                {
                    return "OLE";
                }
                return callingSystem.ToUpper().Trim();
            }
        }

        public static object MaxDiscountAmount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, string coverage)
        {
            decimal maxDiscountAmt = 99;
            DataRow[] rows = stateInfo.GetRows(pol, "MAXDISCOUNT", "PERCENT", coverage, connectionString);
            foreach (DataRow row in rows)
            {
                maxDiscountAmt = decimal.Parse(row["ItemValue"].ToString());
            }
            return maxDiscountAmt;
        }

        public static DataTable LoadPayPlanTable(clsPolicyPPA pol, string connectionString)
        {
            string SQL = " SELECT Program, PayPlanCode, Name, DownPayPct, NumInstallments, InstallmentType, UsePremWFeesInCalc ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..PayPlan with(nolock)";
            SQL += "  WHERE Program IN ('PPA', @Program) ";
            SQL += "  AND EffDate <= @RateDate ";
            SQL += "  AND ExpDate > @RateDate ";
            SQL += "  AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += "  ORDER BY Program, PayPlanCode, DownPayPct ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            return DBHelper.GetDataTable(SQL, "PayPlan", connectionString, parms);
        }

        public static bool PayPlanIsPaidInFull(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (PolicyHasOutsidePremiumFinance(pol))
            {
                return false;
            }
            else
            {
                if (pol.PayPlanCode == "100") return true;
                if (pol.ApplyPIFDiscount) return true;
            }
            return false;
        }

        public static bool PolicyHasOutsidePremiumFinance(clsPolicyPPA pol){
            foreach (clsEntityLienHolder lien in pol.LienHolders)
            {
                if (lien.EntityType.ToUpper() == "PFC")
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetUWTierForCoverageFactors(clsPolicyPPA pol)
        {
            if (pol.Program.ToUpper() == "SUMMIT" || ((pol.Program.ToUpper() == "CLASSIC" || pol.Program.ToUpper() == "DIRECT") && pol.StateCode == "42"))
            {
                return pol.PolicyInsured.UWTier;
            }
            else
            {
                return "1";
            }
        }
        
        public static object UpdateMidAddFactorBasedOnTerm(decimal factorAmt, clsPolicyPPA pol)
        {
            //Only Florida
            return factorAmt;
        }
        public static string GetDriverAssignmentType(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, string Override = "")
        {
            string drvAssignType = "";
            if (Override != string.Empty)
            {
                drvAssignType = Override;
            }
            else
            {
                DataRow[] rows = null;                
                rows = stateInfo.GetRows(pol, "DRIVER", "ASSIGNMENT", "TYPE", connectionString);
                foreach (DataRow row in rows)
                {
                    drvAssignType = row["ItemValue"].ToString();
                }
            }
            return drvAssignType;

        }
        public static int NormalizeStatus(clsPolicyPPA pol){
            if (pol.Status == "1") return 1;
            if (pol.Status == "2") return 2;
            if (pol.Status == "3") return 3;
            if (pol.Status == "4") return 4;
            return 5;
        }
        public static bool ValidStatusForDownPayment(clsPolicyPPA pol)
        {
            if (pol.Status == "1" || pol.Status == "2" || pol.Status == "3")
            {
                return true;
            }
            else
            {
                if (pol.TransactionNum <= 1 && pol.Status == "4" && pol.Type.ToUpper().Trim() != "RENEWAL")
                {
                    return true;
                }
            }
            return false;
        }

        public static void UpdateExpirationDate(clsPolicyPPA pol)
        {
            pol.ExpDate = pol.EffDate.AddMonths(pol.Term);
        }

        public static bool ValidPayPlan(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable payPlans = LoadPayPlanTable(pol, connectionString);
            foreach (DataRow row in payPlans.Rows)
            {
                if (row["PayPlanCode"].ToString().ToUpper() == pol.PayPlanCode.ToUpper())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
