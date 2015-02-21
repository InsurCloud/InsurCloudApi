using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Helpers;
using CorPolicy;
using RulesLib.Rules;

namespace QuoteWR
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class QuoteService : IQuoteService
    {

        public clsPolicyPPA QuotePersonalAuto(clsPolicyPPA pol)
        {
            string connectionString = "Server=tcp:emuxtovazm.database.windows.net,1433;Database=pgm242;User ID=AppUser@emuxtovazm;Password=AppU$er!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
            StateInfoHelper stateInfo = new StateInfoHelper(pol.Product, pol.StateCode, pol.RateDate, pol.AppliesToCode, pol.ProgramInfo.CompanyCode, connectionString);
            QuoteEngine.QuoteEngine qe = new QuoteEngine.QuoteEngine();
            qe.ApplyCapFactor(pol, stateInfo, connectionString);
            pol = qe.RatePPA(pol, stateInfo, connectionString);
            return pol;

            //pol.QuoteID = "24210000000000001";
            //pol.FullTermPremium = 1000;
            //pol.TotalFees = 50;
            //pol.DownPaymentAmt = 250;
            
        }


        public clsPolicyPPA EnoughToRate(clsPolicyPPA pol)
        {
            string connString = "Server=tcp:emuxtovazm.database.windows.net,1433;Database=pgm242;User ID=AppUser@emuxtovazm;Password=AppU$er!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
            StateInfoHelper stateInfo = new StateInfoHelper(pol.Product, pol.StateCode, pol.RateDate, pol.AppliesToCode, pol.ProgramInfo.CompanyCode, connString);
            Rules2 rules = new Rules2();
            rules.CheckNEI(pol, stateInfo, connString);
            return pol;
        }

        public clsPolicyPPA ValidRisk(clsPolicyPPA pol)
        {            
            string connectionString = "Server=tcp:emuxtovazm.database.windows.net,1433;Database=pgm242;User ID=AppUser@emuxtovazm;Password=AppU$er!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
            StateInfoHelper stateInfo = new StateInfoHelper(pol.Product, pol.StateCode, pol.RateDate, pol.AppliesToCode, pol.ProgramInfo.CompanyCode, connectionString);
            Rules2 rules = new Rules2();

            if (!rules.CheckNEI(pol, stateInfo, connectionString)) return pol;

            rules.SetupPolicyData(ref pol, connectionString, stateInfo);
            if (rules.CheckNEI(pol, stateInfo, connectionString, true))
            {
                rules.CheckIER("POLICY", pol, stateInfo, connectionString);
                rules.CheckUWW("POLICY", pol, stateInfo, connectionString);
                rules.CheckWRN("POLICY", pol, stateInfo, connectionString);
                rules.CheckRES("POLICY", pol, stateInfo, connectionString);
            }
            rules.Finish(ref pol, connectionString, stateInfo);

            return pol;
        }
    }
}
