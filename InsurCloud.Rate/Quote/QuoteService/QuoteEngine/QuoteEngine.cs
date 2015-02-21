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
    public class QuoteEngine
    { 
        public int RatedVehicleIndex { get; set; }

        public void ApplyCapFactor(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Do Nothing - this system is not for renewal quoting yet
        }

        public clsPolicyPPA RatePPA(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            
            //These should be in the rules engine
            PolicyHelper.UpdateExpirationDate(pol);
            VehicleHelper.SetTerritoryInfo(pol, stateInfo, connectionString);
            VehicleHelper.ClearPremiumFactors(pol);
            DataTable factors = PremiumRater.CalculateFullTermPremium(pol, stateInfo, connectionString);
            FeeRater.CalculateFees(pol, stateInfo, connectionString);
            BillingRater.SetDownPaymentAmount(pol, stateInfo, connectionString);
            PremiumRater.CleanDataTable(factors, pol, stateInfo, connectionString);
            DriverHelper.RemoveDefaultAndCombinedAverageDrivers(pol);
            //FinishLogging

            return pol;
        }
    }
}
