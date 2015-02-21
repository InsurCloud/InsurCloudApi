using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace QuoteEngine
{
    public static class BillingRater
    {
        public static void SetDownPaymentAmount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (pol.FullTermPremium > 0 && PolicyHelper.ValidStatusForDownPayment(pol))
            {            
                DataTable payPlansTable = PolicyHelper.LoadPayPlanTable(pol, connectionString);
                DataRow[] rows = payPlansTable.Select("PayPlanCode = '" + pol.PayPlanCode + "'");

                decimal totalsForInstallCalcs = pol.FullTermPremium;
                foreach (DataRow row in rows)
                {
                    decimal totalsForInstall = pol.FullTermPremium;
                    if (Convert.ToBoolean(row["UsePremWFeesInCalc"]))
                    {
                        foreach(clsBaseFee fee in pol.Fees)
                        {
                            if (fee.FeeApplicationType.ToUpper().Trim() == "SPREAD")
                            {
                                totalsForInstall += fee.FeeAmt;
                            }
                        }
                    }

                    pol.DownPaymentAmt = DBHelper.RoundStandard(totalsForInstall * (decimal.Parse(row["DownPayPct"].ToString()) / 100), 2);

                    foreach (clsBaseFee fee in pol.Fees)
                    {
                        if (fee.FeeApplicationType.ToUpper().Trim() == "EARNED")
                        {
                            pol.DownPaymentAmt += fee.FeeAmt;
                        }
                    }

                }
            }
        }

        
    }
}
