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
    public static class VehicleHelper
    {
        public static int CalculateAge(int vehYear, int effYear, int effMonth)
        {
            int vehAge = 0;            

            if (effMonth >= 10)
            {
                vehAge = effYear - vehYear + 2;
            }
            else
            {
                vehAge = effYear - vehYear + 1;
            }
            if (vehAge < 1) vehAge = 1;
            return vehAge;
        }

        public static void RemoveAutoApplyFactors(DataRow[] rows, clsPolicyPPA pol)
        {
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete)
                {
                    for (int i = veh.Factors.Count - 1; i >= 0; i--)
                    {
                        foreach (DataRow row in rows)
                        {
                            if (row["FactorCode"].ToString().ToUpper() == veh.Factors[i].FactorCode.ToUpper())
                            {
                                veh.Factors.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static int VehicleCount(clsPolicyPPA pol)
        {
            int count = 0;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete)
                {
                    count++;
                }
            }
            return count;
        }

        public static bool CheckForHasClaimsViol(clsPolicyPPA pol)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                foreach (clsBaseViolation viol in drv.Violations)
                {
                    if (viol.ViolTypeCode.Trim().Contains("99998"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static int GetMaxMSRPSymbol(string vehYear, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int retValue;
            try
            {
                retValue = Int32.Parse(stateInfo.GetStringValue(pol, "MAXMSRP", vehYear, vehYear, connectionString));
            }
            catch
            {
                retValue = 99;
            }
            if (retValue == 0) retValue = 99;
            return retValue;                            
        }

        public static bool VehicleHasDefaultSymbol(clsVehicleUnit veh)
        {
            switch (veh.VehicleSymbolCode.Trim())
            {                
                case "965":
                case "966":
                case "967":
                case "968":
                    if (int.Parse(veh.VehicleYear) >= 2011) return true;
                    return true;
                case "65":
                case "66":
                case "67":
                case "68":
                    if (int.Parse(veh.VehicleYear) < 2011) return true;
                    return true;
                case "999":
                    return true;
            }

            switch (veh.PriceNewSymbolCode.Trim())
            {                
                case "965":
                case "966":
                case "967":
                case "968":
                    if (int.Parse(veh.VehicleYear) >= 2011) return true;
                    return true;
                case "65":
                case "66":
                case "67":
                case "68":
                    if (int.Parse(veh.VehicleYear) < 2011) return true;
                    return true;
                case "999":
                    return true;
            }
            return false;
        }

        public static bool VehicleSymbolIsStatedAmountSymbol(string vehicleSymbolCode, int vehicleYear)
        {
            switch (vehicleSymbolCode.Trim())
            {
                case "999":
                case "965":
                case "966":
                case "967":
                case "968":
                    if (vehicleYear >= 2011) return true;
                    return false;
                case "65":
                case "66":
                case "67":
                case "68":
                    if (vehicleYear < 2011) return true;
                    return false;
                default:
                    return false;
            }
        }

        public static bool PhysicalDamageCoverageRequested(clsVehicleUnit veh)
        {
            if (HasCoverage(veh, "COL") || HasCoverage(veh, "OTC"))
            {
                return true;
            }
            return false;
        }

        public static clsBaseCoverage FindCoverage(clsVehicleUnit veh, string coverageCode)
        {
            foreach (clsBaseCoverage coverage in veh.Coverages)
            {
                if (!coverage.IsMarkedForDelete)
                {
                    if (coverage.CovCode.Contains(coverageCode))
                    {
                        return coverage;
                    }
                }
            }
            return null;
        }

        public static clsBaseCoverage FindCoverageGroup(clsVehicleUnit veh, string covGroup)
        {
            foreach (clsBaseCoverage coverage in veh.Coverages)
            {
                if (!coverage.IsMarkedForDelete)
                {
                    if (coverage.CovGroup.Trim().ToUpper() == covGroup.Trim().ToUpper())
                    {
                        return coverage;
                    }
                }
            }
            return null;
        }

        public static bool HasCoverageGroup(clsVehicleUnit veh, string covGroup)
        {
            if (FindCoverageGroup(veh, covGroup) != null) return true;
            return false;
        }
        public static bool HasCoverage(clsVehicleUnit veh, string coverageCode)
        {
            if (FindCoverage(veh, coverageCode) != null) return true;
            return false;
        }

        public static bool IsLeasedVehicle(clsVehicleUnit veh)
        {
            if (HasCoverage(veh, "LLS")) return true;
            return false;
        }

        public static bool HasLessorListed(clsVehicleUnit veh)
        {
            foreach (clsEntityLienHolder lienHolder in veh.LienHolders)
            {
                if (lienHolder.EntityType.ToUpper().Trim() == "AI" || lienHolder.EntityType.ToUpper().Trim() == "LP") return true;
            }
            return false;
        }

        public static string ValidateCoverages(clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string invalidCoverages = string.Empty;
            string covCodes = string.Empty;
            string covGroups = string.Empty;
            for (int i = 0; i < veh.Coverages.Count(); i++)
            {
                clsPACoverage cov = veh.Coverages[i];
                if (!cov.IsMarkedForDelete)
                {
                    if (covCodes != string.Empty)
                    {
                        covCodes += ",";
                        covGroups += ",";
                    }
                    covCodes += "'" + cov.CovCode + "'";
                    covGroups += "'" + cov.CovGroup + "'";
                }
            }

            string SQL = GetCoverageSql(pol, covCodes, covGroups);

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            DataTable invalidCovCombos = DBHelper.GetDataTable(SQL, "InvalidCovCombos", connectionString, parms);

            string previousCovCheck = string.Empty;
            foreach (DataRow row in invalidCovCombos.Rows)
            {
                switch (row["RequireType"].ToString().ToUpper().Trim())
                {
                    case "L":                                                
                        invalidCoverages += "Veh#" + veh.IndexNum + " cannot have " + GetCoverageDisplayName(pol.Program, row["CovCheck"].ToString()) + " and " + GetCoverageDisplayName(pol.Program, row["CovRequired"].ToString()) + "." + "<br />";
                        break;
                    case "I": //Required
                        invalidCoverages += "Veh#" + veh.IndexNum + " cannot have " + GetCoverageDisplayName(pol.Program, row["CovCheck"].ToString()) + " coverage without " + GetCoverageDisplayName(pol.Program, row["CovRequired"].ToString()) + " coverage." + "<br />";
                        break;
                    case "X": //Excluded
                        invalidCoverages += "Veh#" + veh.IndexNum + " cannot have " + GetCoverageDisplayName(pol.Program, row["CovCheck"].ToString()) + " coverage with " + GetCoverageDisplayName(pol.Program, row["CovRequired"].ToString()) + " coverage." + "<br />";
                        break;
                    case "O": // Either Or (Must have Coverage A or Coverage B)   
                        if(previousCovCheck == row["CovCheck"].ToString().ToUpper().Trim()){
                            invalidCoverages += " or " + GetCoverageDisplayName(pol.Program, row["CovRequired"].ToString()) + "<br />";
                        }else{
                            previousCovCheck = row["CovCheck"].ToString().ToUpper().Trim();
                            invalidCoverages += "Veh#" + veh.IndexNum + " cannot have " + GetCoverageDisplayName(pol.Program, row["CovCheck"].ToString()) + " coverage without " + GetCoverageDisplayName(pol.Program, row["CovRequired"].ToString());
                        }
                        break;
                }
            }
            return invalidCoverages;
        }
        public static string GetCoverageCode(clsVehicleUnit veh, string coverage)
        {
            string covCode = string.Empty;
            foreach (clsBaseCoverage cov in veh.Coverages)
            {
                if (cov.CovGroup == coverage)
                {
                    if (!cov.IsMarkedForDelete)
                    {
                        covCode = cov.CovCode;
                        break;
                    }
                }
            }
            return covCode;
        }
        public static string GetCoverageDisplayName(string program, string covGroup){
            if(program.ToUpper() == "DIRECT"){
                switch(covGroup.ToUpper().Trim()){
                    case "UUMPD":
                        return "Uninsured Motorist PD";
                    case "UMPD":
                        return "Uninsured Motorist PD";
                    case "UUMBI":
                        return "Uninsured Motorist BI";
                    case "MED":
                        return "Med Pay";
                    case "OTC":
                        return "Other Than Collision";
                    case "COL":
                        return "Collision";
                    case "REN":
                        return "Rental";
                    case "TOW":
                        return "Towing";
                    case "UIMBI":
                        return "Underinsured Motorist BI";
                    case "UMBI":
                        return "Underinsured Motorist BI";
                }
            }
            return covGroup;
        }

        private static string GetCoverageSql(clsPolicyPPA pol, string covCodes, string covGroups)
        {
            string sSql = String.Empty;

            sSql = "  SELECT CovCheck, CovRequired, RequireType ";
            sSql += "   FROM pgm" + pol.Product.Trim() + pol.StateCode.Trim() + "..CodeCovCombo with(nolock) ";
            sSql += "  WHERE Program = @Program ";
            sSql += "    AND EffDate <= @RateDate ";
            sSql += "    AND ExpDate > @RateDate ";
            sSql += "    AND AppliesToCode IN ('B', @AppliesToCode) ";
            sSql += "    AND CovType = 'CovCode' ";
            sSql += "    AND CovCheck IN (" + covCodes + ") ";
            sSql += "    AND RequireType = 'L' ";
            sSql += "    AND CovRequired IN (" + covCodes + ") ";
            sSql += "  UNION ";
            sSql += " SELECT CovCheck, CovRequired, RequireType ";
            sSql += "   FROM pgm" + pol.Product.Trim() + pol.StateCode.Trim() + "..CodeCovCombo with(nolock) ";
            sSql += "  WHERE Program = @Program ";
            sSql += "    AND  EffDate <= @RateDate ";
            sSql += "    AND ExpDate > @RateDate ";
            sSql += "    AND AppliesToCode IN ('B', @AppliesToCode) ";
            sSql += "    AND CovType = 'CovGroup' ";
            sSql += "    AND CovCheck IN (" + covGroups + ")";
            sSql += "    AND ((RequireType = 'I' AND CovRequired NOT IN (" + covGroups + ")) ";
            sSql += "     OR  (RequireType = 'X' AND CovRequired IN (" + covGroups + "))) ";
            sSql += "  UNION ";
            sSql += " SELECT CovCheck, CovRequired, RequireType ";
            sSql += "   FROM pgm" + pol.Product.Trim() + pol.StateCode.Trim() + "..CodeCovCombo with(nolock) ";
            sSql += "  WHERE Program = @Program ";
            sSql += "    AND  EffDate <= @RateDate ";
            sSql += "    AND ExpDate > @RateDate ";
            sSql += "    AND AppliesToCode IN ('B', @AppliesToCode) ";
            sSql += "    AND CovType = 'CovGroup' ";
            sSql += "    AND RequireType = 'O'";
            sSql += "    AND CovCheck IN (" + covGroups + ")";
            sSql += "    AND NOT EXISTS(SELECT CovCheck, CovRequired, RequireType ";
            sSql += "                   FROM pgm" + pol.Product.Trim() + pol.StateCode.Trim() + "..CodeCovCombo with(nolock) ";
            sSql += "                   WHERE Program = @Program ";
            sSql += "                     AND EffDate <= @RateDate ";
            sSql += "                     AND ExpDate > @RateDate ";
            sSql += "                     AND AppliesToCode IN ('B', @AppliesToCode) ";
            sSql += "                     AND CovType = 'CovGroup' ";
            sSql += "                     AND RequireType = 'O'";
            sSql += "                     AND CovRequired IN (" + covGroups + ")";
            sSql += "                   )";

            return sSql;
        }

        public static bool VehicleApplies(clsVehicleUnit veh, clsPolicyPPA pol)
        {
            if (pol.CallingSystem.ToUpper().Contains("OLE") || pol.CallingSystem.ToUpper().Contains("UWC"))
            {
                if (veh.IsNew) return true;
                if (veh.IsModified) return true;
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void SetTerritoryInfo(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable codeTerritoryDef = LoadCodeTerritoryDefinitionsTable(pol, stateInfo, connectionString);
            DataRow[] rows = null;
            
            for (int v = 0; v < pol.VehicleUnits.Count; v++)
            {
                if (!pol.VehicleUnits[v].IsMarkedForDelete)
                {
                    if (pol.VehicleUnits[v].Zip != null && pol.VehicleUnits[v].Zip != string.Empty)
                    {
                        rows = codeTerritoryDef.Select("Program = '" + pol.Program + "' AND Zip = '" + pol.VehicleUnits[v].Zip + "'");
                        foreach (DataRow row in rows)
                        {
                            for (int c = 0; c < pol.VehicleUnits[v].Coverages.Count; c++)
                            {
                                if (!pol.VehicleUnits[v].Coverages[c].IsMarkedForDelete && pol.VehicleUnits[v].Coverages[c].CovGroup.ToUpper() == row["Coverage"].ToString().ToUpper())
                                {
                                    pol.VehicleUnits[v].Coverages[c].Territory = row["Territory"].ToString();
                                    pol.VehicleUnits[v].Territory = row["Territory"].ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static DataTable LoadCodeTerritoryDefinitionsTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string zipCodes = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                zipCodes = (zipCodes == string.Empty) ? string.Concat("'", veh.Zip, "'") : string.Concat(zipCodes, ",'", veh.Zip, "'");
            }
         
            string SQL = " SELECT Program, Coverage, Zip, County, City, State, Territory, Region, Disabled";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..CodeTerritoryDefinitions with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Zip IN (" + zipCodes + ")";
            SQL += " ORDER BY Program, Coverage";


            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            return DBHelper.GetDataTable(SQL, "CodeTerritoryDefinitions", connectionString, parms);
            
        }

        public static void ClearPremiumFactors(clsPolicyPPA pol)
        {
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                int index = 1;
                foreach (clsBaseCoverage cov in veh.Coverages)
                {
                    if (pol.CallingSystem.ToUpper() == "WEBRATER")
                    {
                        cov.IndexNum = index;
                        index++;
                    }
                    cov.Factors.Clear();
                }                
            }
        }

        public static void CheckRentToOwn(clsVehicleUnit veh, clsPolicyPPA pol)
        {
            bool hasRTO = false;
            foreach (clsBaseFactor factor in veh.Factors)
            {
                if (factor.FactorCode.ToUpper() == "RENT_TO_OWN")
                {
                    NotesHelper.AddNote(pol, factor.FactorCode + "-" + veh.IndexNum, factor.FactorCode + "-" + veh.IndexNum, "RTO");
                    hasRTO = true;
                }
            }
            if (!hasRTO)
            {
                NotesHelper.RemoveNotes(pol.Notes, "RTO");
            }
        }

        public static DataTable CreateFactorTable(string tableName, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable factorTable = null;
            DataTable baseRateFactors = null;
            DataColumn colFactorName = null;
            DataColumn colFactorType = null;
            

            try
            {
                factorTable = new DataTable(tableName);
                colFactorName = new DataColumn("FactorName");
                factorTable.Columns.Add(colFactorName);
                
                string SQL = " SELECT DISTINCT(Coverage) ";
                SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorBaseRate with(nolock) ";
                SQL += " WHERE Program = @Program ";
                SQL += " AND EffDate <= @RateDate ";
                SQL += " AND ExpDate > @RateDate ";
                SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                SQL += " ORDER BY Coverage Asc";


                List<SqlParameter> parms = new List<SqlParameter>();

                parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
                parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
                parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

                baseRateFactors = DBHelper.GetDataTable(SQL, tableName, connectionString, parms);
                DataRow[] rows = baseRateFactors.Select();
                foreach(DataRow row in rows)
                {
                    DataColumn colCov2 = new DataColumn(row["Coverage"].ToString());
                    factorTable.Columns.Add(colCov2);
                    if (colCov2 != null)
                    {
                        colCov2.Dispose();
                        colCov2 = null;
                    }
                }

                DataColumn colFlat = new DataColumn("FlatFactor");
                factorTable.Columns.Add(colFlat);
                if (colFlat != null)
                {
                    colFlat.Dispose();
                    colFlat = null;
                }

                colFactorType = new DataColumn("FactorType");
                factorTable.Columns.Add(colFactorType);

                return factorTable;

            }
            catch
            {
                return null;
            }
            finally
            {
                if (colFactorType != null)
                {
                    colFactorType.Dispose();
                    colFactorType = null;
                }
                if (colFactorName != null)
                {
                    colFactorName.Dispose();
                    colFactorName = null;
                }
                if (factorTable != null)
                {
                    factorTable.Dispose();
                    factorTable = null;
                }                
            }       
        }

        public static string GetVehicleYearForSymbolFactorLookup(clsVehicleUnit veh)
        {
            if (veh.VehicleYear.Trim() == "1") return "1900";
            return veh.VehicleYear.Trim();
        }

        public static int GetModelYear(clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int vehYear = 0;
            if (pol.Program.ToUpper() == "MONTHLY")
            {
                vehYear = veh.VehicleAge;
            }
            else
            {
                if (int.Parse(veh.VehicleYear) < 1980 && int.Parse(veh.VehicleYear) > 1)
                {
                    vehYear = 1980;
                }
                else
                {
                    vehYear = int.Parse(veh.VehicleYear);
                }
            }
            if (veh.VinNo.ToUpper().Trim() == "NONOWNER")
            {
                vehYear = 1;
            }
            return vehYear;
        }

        public static string GetVehicleCountType(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            
            int vehCount = 0;

            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete)
                {
                    vehCount++;
                }
            }
            if (vehCount > 1)
            {
                return "M";
            }
            else
            {
                return "S";
            }

        }

        public static void CheckCoverageDeductiblesAndLimits(clsVehicleUnit veh)
        {
            foreach (clsPACoverage cov in veh.Coverages)
            {
                if (!cov.IsMarkedForDelete)
                {
                    string[] covValues = cov.CovCode.Split(':');
                    if (covValues.Length > 3)
                    {
                        string covGroup = covValues[0];
                        string covValue = covValues[1];
                        string covType = covValues[2];
                        bool covLevel = (covValues[3] == "P") ? true : false;
                        switch (covType.ToUpper())
                        {
                            case "D":
                                if (cov.CovDeductible == string.Empty)
                                {
                                    cov.CovDeductible = covValue;
                                }
                                break;
                            case "L":
                                if (cov.CovLimit == string.Empty)
                                {
                                    if (cov.CovGroup != "PID")
                                    {
                                        cov.CovLimit = covValue;
                                    }
                                    else
                                    {
                                        cov.CovLimit = "0";
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        public static string GetSymbolForFactorLookup(clsPACoverage cov, clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            
            string symbol = string.Empty;
            string symbolGroup = stateInfo.GetStringValue(pol, "COVERAGE", "SYMBOL", cov.CovGroup, connectionString);
            if (veh.StatedAmt > 0 && VehicleHelper.VehicleSymbolIsStatedAmountSymbol(veh.VehicleSymbolCode, int.Parse(veh.VehicleYear)))
            {
                symbol = GetVINSymbol("STATEDVALUE", veh.VehicleYear, pol, stateInfo, connectionString);
            }
            else
            {
                switch (symbolGroup.ToUpper().Trim())
                {
                    case "LIA":
                        symbol = veh.LiabilitySymbolCode.Trim();
                        break;
                    case "PIP":
                        symbol = veh.PIPMedLiabilityCode.Trim();
                        break;
                    case "VEH":
                        symbol = GetVehicleSymbolCodeForFactorLookup(veh, cov);
                        break;
                }
            }
            return symbol;
        }

        private static string GetVINSymbol(string itemSubCode, string vehicleYear, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string symbol = string.Empty;
            StateInfoHelper commonStateInfo = new StateInfoHelper("", pol.StateCode, new DateTime(int.Parse(vehicleYear), 1, 1), pol.AppliesToCode, pol.ProgramInfo.CompanyCode, connectionString);
            symbol = commonStateInfo.GetStringValue(pol, "VIN", "Lookup", itemSubCode, connectionString);
            return symbol;

        }

        private static string GetVehicleSymbolCodeForFactorLookup(clsVehicleUnit veh, clsPACoverage cov)
        {
            string symbol = string.Empty;

            if (int.Parse(veh.VehicleYear) >= 2011 && (cov.CovGroup == "LLS" || cov.CovGroup == "COL" || cov.CovGroup == "UMPD") && veh.CollSymbolCode.Trim().Length > 0)
            {
                symbol = veh.CollSymbolCode.Trim();
            }
            else if (int.Parse(veh.VehicleYear) < 2011 && cov.CovGroup == "OTC" && veh.CompSymbolCode.Trim().Length > 0)
            {
                symbol = veh.CompSymbolCode.Trim();
            }
            else
            {
                symbol = veh.VehicleSymbolCode.Trim();
            }

            if (symbol.Length == 1)
            {
                symbol = string.Concat("0", symbol);
            }

            if (symbol.Length > 2 && symbol.ToCharArray()[0] == '0')
            {
                symbol = symbol.Substring(1, 2);
            }
            return symbol;
        }
               
        public static bool VehicleHasValidZipCode(clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            return true;
        }

        public static DataTable GetCoverageList(Dictionary<string, string> covs, Dictionary<string, string> prog, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string SQL = " SELECT Distinct Coverage FROM pgm" + pol.Product + pol.StateCode + "..FactorBaseRate with(nolock)";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            DataTable covTable = DBHelper.GetDataTable(SQL, "CoverageList", connectionString, parms);
            return covTable;
        }
    }
}
