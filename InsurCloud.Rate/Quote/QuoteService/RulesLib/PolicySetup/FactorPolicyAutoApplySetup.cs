using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CorPolicy;
using Helpers;
using RulesLib.Rules;

namespace RulesLib.PolicySetup
{
    public static class FactorPolicyAutoApplySetup
    {
        private static DataTable LoadFactorTable(clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            string SQL = "";

            SQL = " SELECT Program, Coverage, FactorCode, Description, AutoApply, Factor, FactorType ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + ".." + "FactorPolicy with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " ORDER BY Program, FactorCode, Coverage ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            return DBHelper.GetDataTable(SQL, "FactorPolicy", connectionString, parms);

        }
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            StateRules2 rules = new StateRules2();
            bool addFactor = false;            
            DataTable factorTable = null;
            factorTable = LoadFactorTable(pol, connectionString, stateInfo);
            FactorsHelper.RemoveAutoApplyFactors(pol, factorTable);

            var grouped = from row in factorTable.Select("Program IN ('PPA', '" + pol.Program + "') AND AutoApply = 1 ").CopyToDataTable().AsEnumerable()
                          group row by row.Field<string>("FactorCode") into groupby
                          select new { FactorCode = groupby.Key };
            foreach (var grp in grouped){            
                switch(grp.FactorCode.ToString().ToUpper()){
                    case "INELIGIBLE":
                        if(rules.PolicyHasIneligibleRisk(pol) && !rules.HasSurchargeOverride(pol, "POL", "INELIGIBLE", connectionString)){
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "PIF":
                        if (!rules.HasOPF(pol) && (pol.PayPlanCode == "100" || pol.ApplyPIFDiscount)){
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }                       
                        break;
                    case "HOMEOWNER":
                        if (pol.PolicyInsured.OccupancyType.ToUpper() == "HOMEOWNER" || pol.PolicyInsured.OccupancyType.ToUpper() == "MOBILEHOMEOWNER"){
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }                       
                        break;
                    case "6_TERM":
                        if (pol.Term == 6)
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "1_TERM":
                        if (pol.Term == 1)
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "3_TERM":
                        if (pol.Term == 3)
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "2_TERM":
                        if (pol.Term == 2)
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "12_TERM":
                        if (pol.Term == 12)
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "MULTICAR":
                        if (VehicleHelper.VehicleCount(pol) > 1)
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "TRANSFER":
                        if (rules.EligibleForTransferDiscount(pol, stateInfo, connectionString))
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "ADV_QUOTE":
                        //if (rules.EligibleForTransferDiscount(pol, stateInfo, connectionString))
                        //{
                        //    AddFactor(pol, connectionString, row);
                        //}
                        break;
                    case "EFT_DISC":
                    case "EFT":
                        if (pol.PayPlanCode != "100" && !pol.ApplyPIFDiscount)
                        {
                            if (pol.IsEFT)
                            {
                                AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                            }
                        }
                        break;
                    case "NO_VIOL":
                        addFactor = true;
                        if(pol.PolicyInsured.DaysLapse != 0){
                            foreach(clsEntityDriver drv in pol.Drivers){
                                if(!drv.IsMarkedForDelete){
                                    if(drv.DriverStatus.ToUpper() == "ACTIVE"){
                                        if(drv.IndexNum < 98){
                                            if(drv.IndexNum < 16){
                                                addFactor = false;
                                                break;
                                            }
                                            foreach(clsBaseViolation viol in drv.Violations){
                                                bool ignoreViol = false;
                                                if(stateInfo.Contains(pol, "COMBINEDDRIVER", "VIOLGROUPIGNORE", viol.ViolGroup, connectionString)){
                                                    ignoreViol = true;
                                                }
                                                if(stateInfo.Contains(pol, "NOVIOL", "VIOLGROUPIGNORE", viol.ViolGroup, connectionString)){
                                                    ignoreViol = true;
                                                }

                                                DateTime ignoreAdminStartDate = DateTime.MinValue;
                                                ignoreAdminStartDate = stateInfo.GetDateTimeValue(pol, "MERIT", "IGNORE", "ADMINMSG", connectionString);
                                                if(viol.ViolDesc.ToUpper().Trim() == "ADMINISTRATION MESSAGE"){
                                                    if(ignoreAdminStartDate == DateTime.MinValue || pol.RateDate >= ignoreAdminStartDate){
                                                        ignoreViol = true;
                                                    }
                                                }

                                                if(!ignoreViol){
                                                    if(pol.StateCode == "03"){
                                                        if(DBHelper.DateDiffMonths(viol.ViolDate, pol.EffDate) < 36){
                                                            addFactor = false;
                                                            break;
                                                        }
                                                    }else{
                                                        if(DBHelper.DateDiffMonths(viol.ViolDate, pol.EffDate) < 35){
                                                            addFactor = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }else{
                            addFactor = false;
                        }
                        if(addFactor){
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "COMPANION_POLICY":
                        if (pol.CompanionHOMCarrierName != "")
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "COMPANION_FLOOD":
                        clsBaseNote note = NotesHelper.FindNote(pol, "DIS", "Discount:Companion Flood");
                        if (note != null)
                        {
                            pol.CompanionFloodCarrierName = "IMPERIAL";
                        }
                        if (pol.CompanionFloodCarrierName != "")
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "COMPANION_HOME":
                        if (pol.CompanionHOMCarrierName != "")
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "EXCL":
                        
                        if (DriverHelper.HasExcludedDrivers(pol))
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "PIP_X_WL_NIO":
                        if (FactorsHelper.CheckForPIPXFactor(pol, "NIO"))
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                    case "PIP_X_WL_NIRR":
                        if (FactorsHelper.CheckForPIPXFactor(pol, "NIRR"))
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }                        
                        break;
                    case "COVERAGE_FEE":
                        AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        break;
                    case "CFRD":
                        if (pol.Type.ToUpper() == "RENEWAL" && !VehicleHelper.CheckForHasClaimsViol(pol))
                        {
                            AddFactor(pol, connectionString, grp.FactorCode.ToString().ToUpper());
                        }
                        break;
                }
            }

            if (UsingPolicyDiscountMatrix(rules, pol, connectionString))
            {
                RemoveMatrixFactors(pol);
                AddMatrixFactors(rules, pol, connectionString);
            }
            return "";
        }

        private static bool UsingPolicyDiscountMatrix(IPPAStateRule rules, clsPolicyPPA pol, string connectionString){
            string SQL = "";

            SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorPolicyDiscountMatrix with(nolock)";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND UWTier = @UWTier ";
            SQL += " AND MultiCar = @MultiCar ";
            SQL += " AND PaidInFull = @PaidInFull ";
            SQL += " AND HomeOwner = @HomeOwner ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@UWTier", SqlDbType.VarChar, 3, pol.PolicyInsured.UWTier));
            parms.Add(DBHelper.AddParm("@MultiCar", SqlDbType.VarChar, 1, VehicleHelper.VehicleCount(pol) > 1 ? "Y" : "N"));

            
            if (!rules.HasOPF(pol) && (pol.PayPlanCode == "100" || pol.ApplyPIFDiscount))
            {
                parms.Add(DBHelper.AddParm("@PaidInFull", SqlDbType.VarChar, 1, "Y"));
            }
            else
            {
                parms.Add(DBHelper.AddParm("@PaidInFull", SqlDbType.VarChar, 1, "N"));
            }
            parms.Add(DBHelper.AddParm("@HomeOwner", SqlDbType.VarChar, 1, pol.PolicyInsured.OccupancyType.ToUpper() == "HOMEOWNER" ? "Y" : "N"));

            DataTable dt = DBHelper.GetDataTable(SQL, "FactorPolicyDiscountMatrix", connectionString, parms);
            if (dt != null && dt.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }
        private static void AddMatrixFactors(IPPAStateRule rules,  clsPolicyPPA pol, string connectionString){
            if (!rules.HasOPF(pol) && (pol.PayPlanCode == "100" || pol.ApplyPIFDiscount))
            {
                AddFactor(pol, connectionString, "PIF");
            }
            if (pol.PolicyInsured.OccupancyType.ToUpper() == "HOMEOWNER" || pol.PolicyInsured.OccupancyType.ToUpper() == "MOBILEHOMEOWNER")
            {
                AddFactor(pol, connectionString, "HOMEOWNER");
            }
            if (VehicleHelper.VehicleCount(pol) > 1)
            {
                AddFactor(pol, connectionString, "MULTICAR");
            }
        }
        private static void RemoveMatrixFactors(clsPolicyPPA pol)
        {
            FactorsHelper.RemoveFactor("PIF", pol.PolicyFactors);
            FactorsHelper.RemoveFactor("HOMEOWNER", pol.PolicyFactors);
            FactorsHelper.RemoveFactor("MULTICAR", pol.PolicyFactors);
        }
        private static void AddFactor(clsPolicyPPA pol, string connectionString, string factorCode)
        {
            if (!FactorsHelper.FactorOn(pol.PolicyFactors, factorCode))
            {
                FactorsHelper.AddFactor(pol, pol.PolicyFactors, factorCode, "POLICY", connectionString);
            }
        }
        private static void AddFactor(clsPolicyPPA pol, string connectionString, DataRow row)
        {
            AddFactor(pol, connectionString, row["FactorCode"].ToString().ToUpper());
        }
    }
}
