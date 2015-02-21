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
    public static class FactorVehicleAutoApplySetup
    {
        private static DataTable LoadFactorTable(clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            string SQL = "";

            SQL = " SELECT Program, Coverage, FactorCode, Description, AutoApply, Factor, FactorType ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + ".." + "FactorVehicle with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " ORDER BY Program, FactorCode, Coverage ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            return DBHelper.GetDataTable(SQL, "FactorVehicle", connectionString, parms);

        }
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            StateRules2 rules = new StateRules2();
            DataTable factorTable = null;
            factorTable = LoadFactorTable(pol, connectionString, stateInfo);
            FactorsHelper.RemoveAutoApplyFactors(pol, factorTable);
            //FactorCode, Description, AutoApply, Factor, FactorType
            var grouped = from row in factorTable.Select("Program IN ('PPA', '" + pol.Program + "') AND AutoApply = 1 ").CopyToDataTable().AsEnumerable()
                          group row by row.Field<string>("FactorCode") into groupby
                          select new { FactorCode = groupby.Key};

            
            foreach (var grp in grouped)
            {
                switch (grp.FactorCode.ToString().ToUpper())
                {
                    case "IR":
                        if (!rules.HasSurchargeOverride(pol, "VEH", "IR", connectionString))
                        {
                            bool isInEligibleRisk = false;
                            string reason = string.Empty;
                            int businessUseCount = 0;

                            foreach (clsVehicleUnit otherVeh in pol.VehicleUnits)
                            {
                                if (!otherVeh.IsMarkedForDelete && otherVeh.VinNo != "NONOWNER")
                                {
                                    int maxSymbol = VehicleHelper.GetMaxMSRPSymbol(otherVeh.VehicleYear, pol, stateInfo, connectionString);
                                    //1. Vehicles with a value over $60,000
                                    if (!VehicleTooValuable(maxSymbol, pol, connectionString, stateInfo, otherVeh, ref isInEligibleRisk, ref reason, otherVeh))
                                    {
                                        //2. Vehicles rated with physical damage symbol 25 or higher for model years 2010 or older OR physical damage symbol 58 or higher for model years 2011 and newer
                                        if (!RepairsTooCostly(ref isInEligibleRisk, ref reason, otherVeh, maxSymbol))
                                        {
                                            //3. Vehicles Garaged out of state
                                            if (!InvalidVehicleZipCode(ref isInEligibleRisk, ref reason, otherVeh, pol, stateInfo, connectionString))
                                            {
                                                //4. More than 1 Business or Artisan User vehicle
                                                if (!MoreThanOneBusinessOrArtisonUseVehicle(ref isInEligibleRisk, ref reason, ref businessUseCount, otherVeh))
                                                {
                                                    //5. Vehicles that have title or registration indicating vehicle has been reconstructed, salvaged, or water damaged and asking coverage for physical damage
                                                    if (!DamagedVehicleRequestingPhysicalDamageCoverage(ref isInEligibleRisk, ref reason, otherVeh, pol))
                                                    {
                                                        if (!OlderVehicleRequestingPhysicalDamageCoverage(ref isInEligibleRisk, ref reason, otherVeh))
                                                        {
                                                            if (!VehicleOver40YearsOld(ref isInEligibleRisk, ref reason, otherVeh))
                                                            {
                                                                //do nothing
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (isInEligibleRisk)
                                    {                                        
                                        NotesHelper.AddNote(pol, "Warning: A surcharge has been applied to the added vehicle due to: " + otherVeh.IndexNum + ".", "IRSurcharge", "AAF");
                                        AddFactor(pol, otherVeh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());

                                    }
                                }                                
                            }                            
                        }
                        break;
                    case "RENT_TO_OWN":
                        break;
                    case "OTC1":
                        foreach (clsVehicleUnit veh in pol.VehicleUnits)
                        {
                            if (COLDedComparedToOTCDed(veh, "250", "100"))
                            {
                                AddFactor(pol, veh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                            }
                        }
                        break;
                    case "OTC2":
                        foreach (clsVehicleUnit veh in pol.VehicleUnits)
                        {
                            if (COLDedComparedToOTCDed(veh, "250", "250"))
                            {
                                AddFactor(pol, veh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                            }
                        }
                        break;
                    case "OTC3":
                        foreach (clsVehicleUnit veh in pol.VehicleUnits)
                        {
                            if (COLDedComparedToOTCDed(veh, "250", "150"))
                            {
                                AddFactor(pol, veh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                            }
                        }
                        break;
                    case "OTC4":
                        foreach (clsVehicleUnit veh in pol.VehicleUnits)
                        {
                            if (COLDedComparedToOTCDed(veh, "500", "250"))
                            {
                                AddFactor(pol, veh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                            }
                        }
                        break;
                    case "OTC5":
                        foreach (clsVehicleUnit veh in pol.VehicleUnits)
                        {
                            if (COLDedComparedToOTCDed(veh, "500", "500"))
                            {
                                AddFactor(pol, veh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                            }
                        }
                        break;
                    case "OTC6":
                        foreach (clsVehicleUnit veh in pol.VehicleUnits)
                        {
                            if (COLDedComparedToOTCDed(veh, "1000", "500"))
                            {
                                AddFactor(pol, veh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                            }
                        }
                        break;
                    case "EXCL":
                        foreach (clsVehicleUnit veh in pol.VehicleUnits)
                        {
                            if (DriverHelper.HasExcludedDrivers(pol))
                            {
                                AddFactor(pol, veh.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                                //Only do this for one of the vehicles
                                break;
                            }
                        }
                        break;
                    case "LIAB_ADJ":
                        break;
                }
            }            
            
            return "";
        }

        private static bool COLDedComparedToOTCDed(clsVehicleUnit veh, string colDedAmount, string otcDedAmount)
        {
            
            clsBaseCoverage colCov = VehicleHelper.FindCoverage(veh, "COL");
            if (colCov != null)
            {
                clsBaseCoverage otcCov = VehicleHelper.FindCoverage(veh, "OTC");
                if (otcCov != null)
                {
                    if (colCov.CovDeductible == colDedAmount && otcCov.CovDeductible == otcDedAmount)
                    {
                        return true;
                    }
                }
            }            
            return false;
        }

        private static bool VehicleOver40YearsOld(ref bool isInEligibleRisk, ref string reason, clsVehicleUnit otherVeh)
        {
            if (otherVeh.VehicleAge > 40)
            {
                reason = "Vehicles over 40 years old are unacceptable for all coverages.- " + otherVeh.IndexNum;
                isInEligibleRisk = true;
                return true;
            }
            return false;
        }

        private static bool OlderVehicleRequestingPhysicalDamageCoverage(ref bool isInEligibleRisk, ref string reason, clsVehicleUnit otherVeh)
        {
            if (Int32.Parse(otherVeh.VehicleYear) < DateTime.Now.AddYears(-15).Year && VehicleHelper.PhysicalDamageCoverageRequested(otherVeh))
            {
                reason = "Vehicles over 15 years old are unacceptable for all physical damage coverage.- " + otherVeh.IndexNum;
                isInEligibleRisk = true;
                return true;
            }
            return false;
        }

        private static bool DamagedVehicleRequestingPhysicalDamageCoverage(ref bool isInEligibleRisk, ref string reason, clsVehicleUnit otherVeh, clsPolicyPPA pol)
        {
            if (VehicleHelper.PhysicalDamageCoverageRequested(otherVeh))
            {
                if (UWQuestionHelper.UWQuestionAnsweredAffirmatively(pol.UWQuestions, "307"))
                {
                    reason = "Vehicle that has been reconstructed, salvaged, or water damaged requesting Physical Damage coverage.- " + otherVeh.IndexNum;
                    isInEligibleRisk = true;
                    return true;
                }
            }
            return false;
        }

        

        private static bool MoreThanOneBusinessOrArtisonUseVehicle(ref bool isInEligibleRisk, ref string reason, ref int businessUseCount, clsVehicleUnit otherVeh)
        {
            foreach (clsBaseFactor factor in otherVeh.Factors)
            {
                if (factor.FactorCode.ToUpper().Trim() == "BUS_USE")
                {
                    businessUseCount++;
                }
            }
            if (businessUseCount > 1)
            {
                reason = "More than 1 Business or Artisan use vehicle.";
                isInEligibleRisk = true;
                return true;
            }
            return false;

        }

        private static bool InvalidVehicleZipCode(ref bool isInEligibleRisk, ref string reason, clsVehicleUnit otherVeh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string SQL = "";

            SQL = " SELECT Top 1 Zip FROM pgm" + pol.Product + pol.StateCode + ".." + "CodeTerritoryDefinitions with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Zip <= @Zip ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@Zip", SqlDbType.VarChar, 5, otherVeh.Zip));


            string zip = DBHelper.GetScalarValue(SQL, "ValidZip", connectionString, parms);
            if (zip == string.Empty)
            {
                reason = "Vehicles is garaged out of state.- " + otherVeh.IndexNum;
                isInEligibleRisk = true;
                return true;
            }
            return false;
        }
        
        private static bool RepairsTooCostly(ref bool isInEligibleRisk, ref string reason, clsVehicleUnit otherVeh, int maxSymbol)
        {
            if (Int32.Parse(otherVeh.VehicleYear) <= 2010)
            {
                if (otherVeh.VehicleSymbolCode != string.Empty)
                {
                    try
                    {
                        if(Int32.Parse(otherVeh.VehicleSymbolCode.Trim()) > maxSymbol && otherVeh.VinNo.ToUpper() != "NONOWNER" && !VehicleHelper.VehicleSymbolIsStatedAmountSymbol(otherVeh.VehicleSymbolCode, Int32.Parse(otherVeh.VehicleYear))){
                            reason = "Vehicle with physical damage symbol greater than " + maxSymbol.ToString() + " - " + otherVeh.IndexNum;
                            isInEligibleRisk = true;
                            return true;
                        }
                    }catch{
                        //Do Nothing no symbol code available
                    }
                }
            }else{ //Vehicle Year >= 2011
                if (otherVeh.CollSymbolCode != string.Empty)
                {
                    try
                    {
                        if (Int32.Parse(otherVeh.CollSymbolCode.Trim()) > maxSymbol && otherVeh.VinNo.ToUpper() != "NONOWNER" && !VehicleHelper.VehicleSymbolIsStatedAmountSymbol(otherVeh.VehicleSymbolCode, Int32.Parse(otherVeh.VehicleYear)))
                        {
                            reason = "Vehicle with physical damage symbol greater than " + maxSymbol.ToString() + " - " + otherVeh.IndexNum;
                            isInEligibleRisk = true;
                            return true;
                        }
                    }catch{
                        //Do Nothing no symbol code available
                    }
                }
            }
            return false;
        }

        private static bool VehicleTooValuable(int maxSymbol, clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo, clsVehicleUnit veh, ref bool isInEligibleRisk, ref string reason, clsVehicleUnit otherVeh)
        {            
            int vehSymbol = 0;
            try
            {
                if (veh.PriceNewSymbolCode.Trim() == string.Empty)
                {
                    vehSymbol = 0;
                }
                else
                {
                    vehSymbol = Int32.Parse(otherVeh.PriceNewSymbolCode.Trim());
                }
            }
            catch
            {
                vehSymbol = 0;
            }

            if (VehicleHelper.VehicleSymbolIsStatedAmountSymbol(veh.VehicleSymbolCode, Int32.Parse(veh.VehicleYear)))
            {
                if(veh.StatedAmt > 60000){
                    reason = "Vehicle with a value over $60,000.- " + otherVeh.IndexNum;
                    isInEligibleRisk = true;
                    return true;
                }
            }else{
                if (vehSymbol > maxSymbol)
                {
                    reason = "Vehicle with a value over $60,000.- " + otherVeh.IndexNum;
                    isInEligibleRisk = true;
                    return true;
                }
            }
            return false;
        }
        private static void AddFactor(clsPolicyPPA pol, List<clsVehicleFactor> factors, string connectionString, string factorCode)
        {
            if (!FactorsHelper.FactorOn(pol.PolicyFactors, factorCode))
            {
                FactorsHelper.AddFactor(pol, factors, factorCode, "POLICY", connectionString);
            }
        }
        private static void AddFactor(clsPolicyPPA pol, List<clsVehicleFactor> factors, string connectionString, DataRow row)
        {
            AddFactor(pol, factors, connectionString, row["FactorCode"].ToString().ToUpper());
        }
    }
}
