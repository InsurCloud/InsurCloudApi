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
    public static class PremiumRater
    {

        public static DataTable CalculateFullTermPremium(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            decimal policyFullTermPremium = 0;
            DataTable cappedFactors = null;
            DataTable factors = null;
            List<string> cappedFactorList = FactorsHelper.GetCappedFactors(pol, stateInfo, connectionString);

            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete)
                {
                    RateVehicle(pol, stateInfo, connectionString, ref policyFullTermPremium, ref cappedFactors, ref factors, cappedFactorList, veh);
                }
            }

            pol.FullTermPremium = policyFullTermPremium;
            return factors;
        }

        private static void RateVehicle(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, ref decimal policyFullTermPremium, ref DataTable cappedFactors, ref DataTable factors, List<string> cappedFactorList, clsVehicleUnit veh)
        {
            VehicleHelper.CheckRentToOwn(veh, pol);
            factors = VehicleHelper.CreateFactorTable("Factors", pol, stateInfo, connectionString);
            cappedFactors = VehicleHelper.CreateFactorTable("CappedFactors", pol, stateInfo, connectionString);

            DataRow factorRow = null;
            factorRow = cappedFactors.NewRow();
            factorRow["FactorName"] = "MaxDiscountAmt";
            for (int i = 1; i < cappedFactors.Columns.Count; i++)
            {
                factorRow[cappedFactors.Columns[i].ColumnName] = PolicyHelper.MaxDiscountAmount(pol, stateInfo, connectionString, cappedFactors.Columns[i].ColumnName);
            }
            cappedFactors.Rows.Add(DBHelper.CreateTotalsRow(cappedFactors));

            GetFactors(veh, factors, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);

            PremiumRater.CalculatePremium(veh, factors, pol, stateInfo, connectionString);

            policyFullTermPremium += veh.FullTermPremium;
        }

        public static void CleanDataTable(DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            Dictionary<string, string> covs = new Dictionary<string, string>();
            DataRow totalsRow = null;
            Dictionary<string, Dictionary<string, string>> progs = new Dictionary<string, Dictionary<string, string>>();

            progs.Add(pol.Program, covs);
            DataTable covTable = VehicleHelper.GetCoverageList(covs, progs[pol.Program], factorTable, pol, stateInfo, connectionString);
            foreach (DataRow row in covTable.Rows)
            {
                string coverage = row["Coverage"].ToString();
                if (!ProgramContainsCov(progs[pol.Program], coverage))
                {
                    covs.Add(coverage, coverage);
                }
            }
            progs.Remove(pol.Program);
            progs.Add(pol.Program, covs);
            totalsRow = DBHelper.GetRow(factorTable, "Totals");

            CleanTableByCoverage(factorTable, pol, totalsRow, progs);
            
        }

        private static bool ProgramContainsCov(Dictionary<string, string> prog, string coverage)
        {
            try
            {
                return (prog[coverage] != null);
            }
            catch
            {
                return false;
            }
        }

        private static void CleanTableByCoverage(DataTable factorTable, clsPolicyPPA pol, DataRow totalsRow, Dictionary<string, Dictionary<string, string>> progs)
        {
            for (int i = 1; i < factorTable.Columns.Count; i++)
            {
                Dictionary<string, string> prog = progs[pol.Program];
                string colName = factorTable.Columns[i].ColumnName;
                if (colName.ToUpper() == "FACTORNAME" || colName.ToUpper() == "FACTORTYPE" || colName.ToUpper() == "FLATFACTOR")
                {
                    //Don't mess with these types
                }
                else
                {
                    if (!ProgramContainsCov(prog, colName))
                    {
                        totalsRow[i] = 0;
                    }
                    if (prog != null)
                    {
                        prog = null;
                    }
                }
            }
        }

        public static void CalculatePremium(clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            factorTable.Rows.Add(DBHelper.CreateTotalsRow(factorTable));
            DataTable rateOrderTable = FactorsHelper.GetRateOrderTable(pol, connectionString);

            GetPreMultPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            GetPreAddPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);            
            GetMidMultPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            GetMidAddPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            CheckMinPremAmounts(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            GetPostMultPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            GetPostAddPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            UpdateFeeAddFactorAmounts(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            GetFeeAddPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            GetLastMultPremium(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            RoundTotalsRow(rateOrderTable, veh, factorTable, pol, stateInfo, connectionString);
            factorTable.AcceptChanges();
            //UpdateLog();
            GetTotalChangeInPremiumPolFactors(veh, factorTable, pol, stateInfo, connectionString);
            UpdateTotals(veh, factorTable, pol, stateInfo, connectionString);
            GetPremiums(veh, factorTable, pol, stateInfo, connectionString);

            veh.FullTermPremium = SummarizeVehicleFullTermPremium(veh);
        }

        private static decimal SummarizeVehicleFullTermPremium(clsVehicleUnit veh)
        {
            decimal fullTermPremium = 0;
            foreach (clsPACoverage cov in veh.Coverages)
            {
                if (!cov.IsMarkedForDelete)
                {
                    fullTermPremium = fullTermPremium + cov.FullTermPremium;
                }
            }
            return fullTermPremium;
        }

        private static void GetPremiums(clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow totalsRow = DBHelper.GetRow(factorTable, "Totals");
            foreach (clsPACoverage cov in veh.Coverages)
            {
                if (!cov.IsMarkedForDelete)
                {
                    foreach (DataColumn dataCol in totalsRow.Table.Columns)
                    {
                        if (dataCol.ColumnName.ToUpper() == cov.CovGroup.ToUpper())
                        {
                            cov.FullTermPremium = DBHelper.RoundStandard(decimal.Parse(totalsRow[dataCol.ColumnName.ToString()].ToString()), 0);
                            break;
                        }
                    }
                }
            }
        }

        private static void UpdateTotals(clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow totalsRow = null;

            totalsRow = DBHelper.GetRow(factorTable, "Totals");
            if (totalsRow != null)
            {
                foreach (DataColumn dataCol in factorTable.Columns)
                {
                    decimal colValue = 0;
                    if (decimal.TryParse(totalsRow[dataCol.ColumnName.ToString()].ToString(), out colValue))
                    {
                        if (!VehicleHelper.HasCoverageGroup(veh, dataCol.ColumnName.ToString()))
                        {
                            totalsRow[dataCol.ColumnName.ToString()] = 0;
                        }
                    }
                }
            }
        }

        public static void GetTotalChangeInPremiumPolFactors(clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {            
            SetChangeInPremiumFactorAmt(veh, pol.PolicyFactors);

            clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);
            if (drv.IndexNum < 98)
            {
                SetChangeInPremiumFactorAmt(veh, drv.Factors);
            }

            SetChangeInPremiumFactorAmt(veh, veh.Factors);
        }

        private static void SetChangeInPremiumFactorAmt(clsVehicleUnit veh, List<clsBaseFactor> factors)
        {
            foreach (clsBaseFactor factor in factors)
            {
                factor.FactorAmt = 0;
                foreach (clsPACoverage cov in veh.Coverages)
                {
                    if (!cov.IsMarkedForDelete)
                    {
                        foreach (clsPremiumFactor premFactor in cov.Factors)
                        {
                            if (factor.FactorCode.ToUpper().Trim() == premFactor.FactorCode.ToUpper().Trim())
                            {
                                factor.FactorAmt = DBHelper.RoundStandard(factor.FactorAmt + premFactor.FactorAmt, 0);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void SetChangeInPremiumFactorAmt(clsVehicleUnit veh, List<clsVehicleFactor> factors)
        {
            foreach (clsVehicleFactor factor in factors)
            {
                factor.FactorAmt = 0;
                foreach (clsPACoverage cov in veh.Coverages)
                {
                    if (!cov.IsMarkedForDelete)
                    {
                        foreach (clsPremiumFactor premFactor in cov.Factors)
                        {
                            if (factor.FactorCode.ToUpper().Trim() == premFactor.FactorCode.ToUpper().Trim())
                            {
                                factor.FactorAmt = DBHelper.RoundStandard(factor.FactorAmt + premFactor.FactorAmt, 0);
                            }
                        }
                    }
                }
            }
        }

        private static void GetPreMultPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //'get the values from the rate order table and use that to look up the factors on the data table that are
            //' pre mult according to the rate order table and process in the order according to the rate order table
            //'Get the factor value and multiply it to the Totals value for that coverage and replace the Totals value with the new value
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'PreMult'", "RateOrder");
            CalculateAmounts("MULT", veh, factorTable, rateOrderRows);

        }

        private static void GetPreAddPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'PreAdd'", "RateOrder");
            CalculateAmounts("ADD", veh, factorTable, rateOrderRows);
        }

        private static void GetMidMultPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'MidMult'", "RateOrder");
            CalculateAmounts("MULT", veh, factorTable, rateOrderRows, RoundNewTotals: false);
        }
                
        private static void GetMidAddPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'MidAdd'", "RateOrder");
            CalculateAmounts("ADD", veh, factorTable, rateOrderRows, RoundNewTotals: false);
        }

        private static void CheckMinPremAmounts(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            pol.MinPremApplied = false;
            DataRow totalsRow = DBHelper.GetRow(factorTable, "Totals");
            //decimal totalAmt = 0;
            decimal tempAmt = 0;
            foreach (DataColumn dataCol in totalsRow.Table.Columns)
            {
                if (dataCol.ColumnName.ToUpper() == "FACTORTYPE")
                {
                    break;
                }
                if (totalsRow[dataCol.ColumnName.ToUpper()] != DBNull.Value)
                {
                    
                    if (decimal.TryParse(totalsRow[dataCol.ColumnName.ToUpper()].ToString(), out tempAmt))
                    {
                        tempAmt = DBHelper.RoundStandard(tempAmt, 0);
                        totalsRow[dataCol.ColumnName.ToString()] = tempAmt;
                        //totalAmt += (decimal)totalsRow[dataCol.ColumnName.ToUpper()];
                    }
                }
            }
        }

        private static void GetPostMultPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'PostMult'", "RateOrder");
            CalculateAmounts("MULT", veh, factorTable, rateOrderRows);
        }

        private static void GetPostAddPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'PostAdd'", "RateOrder");
            CalculateAmounts("ADD", veh, factorTable, rateOrderRows);
        }

        private static void UpdateFeeAddFactorAmounts(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'FeeAdd'", "RateOrder");
            CalculateFeeFactorAmounts(veh, factorTable, rateOrderRows);

        }

        private static void GetFeeAddPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'FeeAdd'", "RateOrder");
            CalculateAmounts("ADD", veh, factorTable, rateOrderRows, false);
        }

        private static void GetLastMultPremium(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rateOrderRows = rateOrderTable.Select("FactorType = 'LastMult'", "RateOrder");
            CalculateAmounts("MULT", pol.VehicleUnits[0], factorTable, rateOrderRows);
        }

        private static void RoundTotalsRow(DataTable rateOrderTable, clsVehicleUnit veh, DataTable factorTable, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {

        }

        private static void CalculateFeeFactorAmounts(clsVehicleUnit veh, DataTable factorTable, DataRow[] rateOrderRows)
        {
            DataRow totalsRow = DBHelper.GetRow(factorTable, "Totals");

            decimal newTotal = 0;
            decimal factorAmt = 0;
            decimal totalAmt = 0;
            foreach (DataRow row in rateOrderRows)
            {
                DataRow feeAddRow = DBHelper.GetRow(factorTable, row["FactorName"].ToString());

                if(feeAddRow != null){
                    
                    for (int y = 1; y < factorTable.Columns.Count; y++)
                    {
                        newTotal = 0;
                        if (factorTable.Columns[y].ColumnName.ToUpper() == "FACTORTYPE")
                        {
                            break;
                        }
                        if (feeAddRow[y] != DBNull.Value)
                        {
                            factorAmt = (decimal)feeAddRow[y];
                            totalAmt = DBHelper.RoundStandard((decimal)totalsRow[y], 0);
                            newTotal = totalAmt * factorAmt;
                            feeAddRow[y] = newTotal;
                        }
                    }
                }               
            }
        }

        private static void CalculateAmounts(string factorType, clsVehicleUnit veh, DataTable factorTable, DataRow[] rateOrderRows, bool UpdatePremFactors = true, bool RoundNewTotals = true)
        {
            decimal newTotal = 0;
            decimal prevTotal = 0;
            decimal factorAmt = 0;
            decimal totalAmt = 0;
            foreach (DataRow row in rateOrderRows)
            {
                for (int x = 0; x < factorTable.Rows.Count; x++)
                {
                    if (row["FactorName"].ToString() == factorTable.Rows[x][0].ToString())
                    {
                        for (int y = 1; y < factorTable.Columns.Count; y++)
                        {
                            newTotal = 0;
                            if (factorTable.Columns[y].ColumnName.ToUpper() == "FACTORTYPE")
                            {
                                break;
                            }
                            if (factorTable.Rows[x][y] != DBNull.Value)
                            {
                                factorAmt = decimal.Parse(factorTable.Rows[x][y].ToString());
                                totalAmt = decimal.Parse(factorTable.Rows[factorTable.Rows.Count - 1][y].ToString());
                                if (totalAmt == 0) totalAmt = 1;
                                if (newTotal == 0)
                                {
                                    if (totalAmt == 1) prevTotal = 0;
                                    prevTotal = totalAmt;
                                }
                                else
                                {
                                    prevTotal = newTotal;
                                }
                                if (factorType.ToUpper() == "ADD")
                                {
                                    newTotal = totalAmt + factorAmt;
                                }
                                else
                                {
                                    newTotal = totalAmt * factorAmt;
                                }

                                if (RoundNewTotals)
                                {
                                    newTotal = DBHelper.RoundStandard(newTotal, 3);
                                }
                                    

                                factorTable.Rows[factorTable.Rows.Count - 1][y] = newTotal;

                                if (UpdatePremFactors && !row["FactorName"].ToString().ToUpper().Contains("-ENDORSE"))
                                {
                                    for (int p = 0; p < veh.Coverages.Count; p++)
                                    {
                                        if (factorTable.Columns[y].ColumnName == veh.Coverages[p].CovGroup)
                                        {
                                            clsPACoverage cov = veh.Coverages[p];
                                            clsPremiumFactor premFactor = new clsPremiumFactor();
                                            premFactor.Type = factorTable.Columns[y].ColumnName.ToUpper().Substring(0, 1);
                                            premFactor.FactorAmt = newTotal - prevTotal; //change in premium
                                            premFactor.FactorCode = row["FactorName"].ToString();
                                            premFactor.FactorName = row["FactorName"].ToString();
                                            if (!cov.IsMarkedForDelete)
                                            {
                                                cov.Factors.Add(premFactor);
                                            }
                                            break;
                                        }
                                    }
                                }
                                
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        private static void GetFactors(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            FactorsHelper.GetCombinedDriverFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetDriverAdjustmentFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetDriverFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetDriverAgePointsFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetDriverClassFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetDriverPointsFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetMarketPointsFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetBaseRateFactor(factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetHouseholdStructureFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetModelYearFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetPolicyFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetPolicyDiscountMatrixFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetMidMultCoverageFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetMidAddCoverageFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetStatedValueFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetSymbolFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetTerritoryFactor_Summit(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetMarketAdjustmentFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString); ;
            FactorsHelper.GetTierMatrixFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString); ;
            FactorsHelper.GetVehicleFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
            FactorsHelper.GetDiscountFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString); ;
            FactorsHelper.GetRatedFactor(veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
        }
    }
}
