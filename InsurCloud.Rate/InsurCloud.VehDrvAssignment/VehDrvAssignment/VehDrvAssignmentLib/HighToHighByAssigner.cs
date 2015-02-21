using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using CorPolicy;
using Helpers;

namespace VehDrvAssignmentLib
{
    public class HighToHighByAssigner : IAssigner
    {
        public clsEntityDriver DefaultDriver { get; set; }
        public DataRow[] Rows { get; set; }
        public bool NeedsDefaultDriver { get; set; }
        public virtual void Execute(List<string> coverageList, List<System.Data.DataTable> driverFactorTables, CorPolicy.clsPolicyPPA pol, Helpers.StateInfoHelper stateInfo, string connectionString)
        {
            //Nothing here
        }

        protected void HighToHighByCoverage(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, bool allCoverages)
        {
            if (pol.Drivers.Count == 1 && pol.VehicleUnits.Count == 1)
            {
                pol.VehicleUnits[0].AssignedDriverNum = pol.Drivers[0].IndexNum;
            }
            else
            {
                // Match the highest rated driver with the highest rated
                // vehicle taking coverage into consideration
                DataSet vehDataSet = GetVehFactorDataSet(pol, stateInfo, connectionString, allCoverages);
                DataTable vehDataTable = vehDataSet.Tables[0];
                DataRow[] vehDataRows = vehDataTable.Select("", "VehFactor Desc");
                int extraDriverNum = 0;
                DataSet drvDataSet = GetEmptyDrvDataSet(pol);
                DataTable drvDataTable = drvDataSet.Tables[0];
                DataRow[] drvRows = drvDataTable.Select();

                List<DataTable> driverFactorTables = LoadDrvFactorDataTables(pol, stateInfo, connectionString, allCoverages);

                foreach (DataRow vehRow in vehDataRows)
                {
                    foreach (clsVehicleUnit veh in pol.VehicleUnits)
                    {
                        if (!veh.IsMarkedForDelete)
                        {
                            if (Int32.Parse(vehRow["VehicleNum"].ToString()) == veh.IndexNum)
                            {
                                veh.AssignedDriverNum = 0;
                                if (extraDriverNum > 0)
                                {
                                    veh.AssignedDriverNum = extraDriverNum;
                                }
                                else
                                {
                                    veh.AssignedDriverNum = FindDrvHighestDrvFactor(driverFactorTables, veh, drvRows, pol, stateInfo, connectionString, allCoverages);
                                    if (veh.AssignedDriverNum == 0)
                                    {
                                        if (extraDriverNum > 0)
                                        {
                                            veh.AssignedDriverNum = extraDriverNum;
                                        }
                                        else
                                        {
                                            veh.AssignedDriverNum = FindDrvExtraDrvFactor(driverFactorTables, veh, pol, stateInfo, connectionString, drvRows, allCoverages);
                                            extraDriverNum = veh.AssignedDriverNum;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private int FindDrvExtraDrvFactor(List<DataTable> driverFactorTables, clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, DataRow[] drvRows, bool allCoverages)
        {
            int extraDriver = 0;
            DataTable drvDataTable = GetExtraDriverFactorData(driverFactorTables, veh, drvRows, pol, stateInfo, connectionString, true, allCoverages);
            DataRow[] drvPremiumRows = drvDataTable.Select("", "DrvRatingFactor Asc");
            foreach (DataRow row in drvPremiumRows)
            {
                extraDriver = Int32.Parse(row["DriverNum"].ToString());
            }
            bool haveDriverData = false;
            clsEntityDriver drv = new clsEntityDriver();
            foreach (clsEntityDriver eDriver in pol.Drivers)
            {
                if (eDriver.IndexNum == extraDriver)
                {
                    haveDriverData = true;
                    drv.Age = eDriver.Age;
                    drv.CreditTier = eDriver.CreditTier;
                    drv.DriverStatus = "DEFAULT";
                    drv.Gender = eDriver.Gender;
                    drv.IndexNum = 98;
                    drv.MaritalStatus = eDriver.MaritalStatus;
                    drv.Points = 0;
                    drv.SR22 = false;
                    drv.UWTier = eDriver.UWTier;
                    drv.DOB = eDriver.DOB;
                    break;
                }
            }

            if (haveDriverData)
            {
                pol.Drivers.Add(drv);
                drv = null;
            }

            // Return the default driver number
            return 98;
        }

        private DataTable GetExtraDriverFactorData(List<DataTable> driverFactorTables, clsVehicleUnit veh, DataRow[] drvRows, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, bool p, bool allCoverages)
        {
            return GetDriverFactorData(driverFactorTables, veh, drvRows, pol, stateInfo, connectionString, allCoverages, true);
        }

        private int FindDrvHighestDrvFactor(List<DataTable> driverFactorTables, clsVehicleUnit veh, DataRow[] drvRows, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, bool allCoverages)
        {
            int highestDriver = 0;
            int driverCount = 0;

            foreach (DataRow row in drvRows)
            {
                if (Int32.Parse(row["AssignedToVeh"].ToString()) == 0)
                {
                    highestDriver = Int32.Parse(row["DriverNum"].ToString());
                    driverCount++;
                }
            }

            if (driverCount > 1)
            {
                DataTable drvFactorTable = GetDriverFactorData(driverFactorTables, veh, drvRows, pol, stateInfo, connectionString, allCoverages, false);
                DataRow[] drvPremiumRows = null;
                drvPremiumRows = drvFactorTable.Select("", "DrvRatingFactor Desc");
                if (drvPremiumRows.Length > 0)
                {
                    highestDriver = Int32.Parse(drvPremiumRows[0]["DriverNum"].ToString());
                }
            }

            foreach (DataRow row in drvRows)
            {
                if (Int32.Parse(row["DriverNum"].ToString()) == highestDriver)
                {
                    row["AssignedToVeh"] = "1";
                    break;
                }
            }
            return highestDriver;
        }
        
        private DataTable GetDriverFactorData(List<DataTable> driverFactorTables, clsVehicleUnit veh, DataRow[] drvRows, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, bool allCoverages, bool forExtra = false)
        {
            DataTable drvDataTable = new DataTable("Drivers");

            //DataColumn col = null;
            DataRow row = null;
            Decimal drvRatingFactor = 0;

            drvDataTable.Columns.Add(DBHelper.AddColumn("DriverNum"));
            drvDataTable.Columns.Add("DrvRatingFactor", System.Type.GetType("System.Decimal"));
            DataTable baseRateTable = driverFactorTables.Find(p => p.TableName == "FactorBaseRate");
            DataRow[] rows = null;
            DataRow[] dataRows = null;
            foreach (DataRow drvRow in drvRows)
            {
                if (drvRow["AssignedToVeh"].ToString() == "0" || forExtra)
                {
                    foreach (clsEntityDriver drv in pol.Drivers)
                    {
                        if (!drv.IsMarkedForDelete && drv.IndexNum == Int32.Parse(drvRow["DriverNum"].ToString()))
                        {
                            Decimal totalPremium = 0;
                            row = drvDataTable.NewRow();
                            row["DriverNum"] = drv.IndexNum;

                            if (!forExtra)
                            {
                                rows = baseRateTable.Select();
                            }
                            else
                            {
                                rows = baseRateTable.Select("Coverage IN('BI','PD')");
                            }


                            foreach (DataRow baseRateRow in rows)
                            {
                                bool useDriverAdjustment = false;
                                string coverage = baseRateRow["Coverage"].ToString();
                                drvRatingFactor = 1;
                                if (forExtra || VehicleHelper.HasCoverage(veh, coverage))
                                {
                                    if ((Decimal)baseRateRow["Factor"] > 0)
                                    {
                                        drvRatingFactor *= (Decimal)baseRateRow["Factor"];
                                    }
                                    else
                                    {
                                        DataTable terrFactorTable = driverFactorTables.Find(p => p.TableName == "FactorTerritory");
                                        //this is florida, get the base rate from the territory factor table
                                        dataRows = terrFactorTable.Select("Coverage='" + coverage + "' AND Territory='" + veh.Territory + "' ");
                                        foreach (DataRow terRow in dataRows)
                                        {
                                            drvRatingFactor *= (Decimal)terRow["Factor"];
                                        }
                                    }

                                    if (allCoverages)
                                    {
                                        string covCode = VehicleHelper.GetCoverageCode(veh, coverage);
                                        DataTable covFactorTable = driverFactorTables.Find(p => p.TableName == "FactorCoverage");
                                        dataRows = covFactorTable.Select("Code = '" + covCode + "'");
                                        foreach (DataRow covRow in dataRows)
                                        {
                                            drvRatingFactor *= (Decimal)covRow["Factor"];
                                        }
                                    }

                                    if (!forExtra)
                                    {
                                        string driverClass = DriverHelper.GetDriverClassDefinition(drv, pol, stateInfo, connectionString);
                                        DataTable drvAdjustFactorTable = driverFactorTables.Find(p => p.TableName == "FactorDriverAdjustment");
                                        dataRows = drvAdjustFactorTable.Select("Coverage='" + coverage + "' AND Points='" + drv.Points + "' AND DriverClass='" + driverClass + "'");
                                        foreach (DataRow adjRow in dataRows)
                                        {
                                            drvRatingFactor *= (Decimal)adjRow["Factor"];
                                            useDriverAdjustment = true;
                                        }
                                    }
                                    if (!useDriverAdjustment)
                                    {
                                        string driverClass =  DriverHelper.GetDriverClassDefinition(drv, pol, stateInfo, connectionString);
                                        DataTable drvClassFactorTable = driverFactorTables.Find(p => p.TableName == "FactorDriverClass");
                                        dataRows = drvClassFactorTable.Select("Coverage='" + coverage + "' AND DriverClass='" + driverClass + "'");
                                        foreach (DataRow clsRow in dataRows)
                                        {
                                            drvRatingFactor *= (Decimal)clsRow["Factor"];
                                        }

                                        if (!forExtra)
                                        {
                                            DataTable driverPointsFactorTable = driverFactorTables.Find(p => p.TableName == "FactorDriverPoints");
                                            dataRows = driverPointsFactorTable.Select("Coverage='" + coverage + "' AND Points='" + drv.Points + "'");
                                            foreach (DataRow ptsRow in dataRows)
                                            {
                                                drvRatingFactor *= (Decimal)ptsRow["Factor"];
                                            }
                                        }
                                    }

                                    DataTable driverFactorsTable = driverFactorTables.Find(p => p.TableName == "FactorDriver");
                                    foreach (clsBaseFactor factor in drv.Factors)
                                    {
                                        
                                        dataRows = driverFactorsTable.Select("Coverage='" + coverage + "' AND FactorCode='" + factor.FactorCode.Trim() + "' AND FactorType='MIDMULT'");
                                        foreach (DataRow drfRow in dataRows)
                                        {
                                            drvRatingFactor *= (Decimal)drfRow["Factor"];
                                        }
                                    }

                                    foreach (clsBaseFactor factor in drv.Factors)
                                    {
                                        dataRows = driverFactorsTable.Select("Coverage='" + coverage + "' AND FactorCode='" + factor.FactorCode.Trim() + "' AND FactorType='MIDADD'");
                                        foreach (DataRow drfRow in dataRows)
                                        {
                                            drvRatingFactor += (Decimal)drfRow["Factor"];
                                        }
                                    }
                                    totalPremium += drvRatingFactor;
                                }
                                row["DrvRatingFactor"] = totalPremium;

                                if (row != null)
                                {
                                    drvDataTable.Rows.Add(row);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return drvDataTable;

        }

        private List<DataTable> LoadDrvFactorDataTables(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, bool allCoverages)
        {
            List<DataTable> driverFactorDS = new List<DataTable>();

            driverFactorDS.Add(FactorsHelper.GetBaseRateTable(pol, connectionString)); //FactorBaseRate
            driverFactorDS.Add(FactorsHelper.GetDriverClassFactorTableFilterByDriverClass(pol, stateInfo, connectionString)); //FactorDriverClass
            driverFactorDS.Add(FactorsHelper.GetDriverPointsTableFilterByPolicyPoints(pol, stateInfo, connectionString)); //FactorDriverPoints
            driverFactorDS.Add(FactorsHelper.GetDriverFactorTable(pol, stateInfo, connectionString)); //FactorDriver
            driverFactorDS.Add(FactorsHelper.GetDriverAdjustmentTable(pol, stateInfo, connectionString)); //FactorDriverAdjustment
            if (allCoverages)
            {
                driverFactorDS.Add(FactorsHelper.GetCoverageFactorTable(pol, stateInfo, connectionString)); //FactorCoverage               
            }
            return driverFactorDS;
        }
        
        private DataSet GetEmptyDrvDataSet(clsPolicyPPA pol)
        {
            DataSet drvDataSet = new DataSet();
            DataTable drvDataTable = new DataTable("Drivers");
            DataRow row = null;

            drvDataTable.Columns.Add(DBHelper.AddColumn("DriverNum"));
            drvDataTable.Columns.Add(DBHelper.AddColumn("AssignedToVeh"));

            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete && DriverHelper.ShouldRateDriver(drv, pol))
                {
                    row = drvDataTable.NewRow();
                    row["DriverNum"] = drv.IndexNum;
                    row["AssignedToVeh"] = "0";
                    drvDataTable.Rows.Add(row);
                }
            }
            drvDataSet.Tables.Add(drvDataTable);
            return drvDataSet;

        }

        private DataSet GetVehFactorDataSet(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, bool allCoverages)
        {
            DataSet vehDataSet = new DataSet();
            DataTable vehDataTable = new DataTable("Vehicles");

            DataRow row = null;
            Decimal totalPremium = 0;

            vehDataTable.Columns.Add(DBHelper.AddColumn("VehicleNum"));
            vehDataTable.Columns.Add("VehFactor", System.Type.GetType("System.Decimal"));

            //Load the Rate Tables
            DataRow[] rows = null;
            DataTable baseRateTable = FactorsHelper.GetBaseRateTable(pol, connectionString);
            DataTable symbolTable = FactorsHelper.GetSymbolFactorTable(pol, stateInfo, connectionString);
            DataTable statedValueTable = FactorsHelper.GetStatedValueFactorTable(pol, connectionString);
            DataTable modelYearTable = FactorsHelper.GetModelYearTable(pol, stateInfo, connectionString);//) LoadFactorModelYearTable(pol, connectionString);
            DataTable vehicleFactorTable = FactorsHelper.GetVehicleFactorTable(pol, connectionString);
            DataTable territoryTable = FactorsHelper.GetTerritoryCodeTable(pol, connectionString);
            DataTable territoryFactorTable = FactorsHelper.GetTerritoryFactorTable(pol, stateInfo, connectionString);
            DataTable coverageFactorTable = null;
            if (allCoverages)
            {
                coverageFactorTable = FactorsHelper.GetCoverageFactorTable(pol, stateInfo, connectionString);
            }

            //Calculate the premium for each vehicle
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete)
                {
                    Decimal vehRatingFactor = 1;
                    string coverage = "";
                    totalPremium = 0;

                    row = vehDataTable.NewRow();
                    row["VehicleNum"] = veh.IndexNum;
                    rows = baseRateTable.Select("Program = '" + pol.Program + "'");
                    foreach (DataRow baseRateRow in rows)
                    {
                        vehRatingFactor = 1;
                        coverage = baseRateRow["Coverage"].ToString();

                        //Loop through each coverage on the vehicle and calculate the premium
                        if (VehicleHelper.HasCoverage(veh, coverage))
                        {
                            DataRow[] dataRows = null;
                            if ((Decimal)baseRateRow["Factor"] > 0)
                            {
                                vehRatingFactor *= (Decimal)baseRateRow["Factor"];
                            }
                            else
                            {
                                if (veh.Territory.Length == 0)
                                {
                                    dataRows = territoryTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Zip='" + veh.Zip + "'");
                                    foreach (DataRow dataRow in dataRows)
                                    {
                                        veh.Territory = dataRow["Territory"].ToString();
                                    }
                                }
                                //this is florida, get the base rate from the territory factor table
                                dataRows = territoryFactorTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Coverage='" + coverage + "' AND Territory='" + veh.Territory + "' ");
                                foreach (DataRow dataRow in dataRows)
                                {
                                    vehRatingFactor *= (Decimal)dataRow["Factor"];
                                }
                            }
                            if (allCoverages)
                            {
                                string covCode = VehicleHelper.GetCoverageCode(veh, coverage);
                                dataRows = coverageFactorTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Code = '" + covCode + "'");
                                foreach (DataRow dataRow in dataRows)
                                {
                                    vehRatingFactor *= (Decimal)dataRow["Factor"];
                                }
                            }

                            string symbol = "";

                            //SYMBOL1 = LiabilitySymbolCode
                            //SYMBOL2 = VehicleSymbolCode
                            //SYMBOL3 = PIPMedSymbolCode
                            for (int i = 0; i < 3; i++)
                            {
                                symbol = "";
                                switch (i)
                                {
                                    case 0:
                                        symbol = veh.LiabilitySymbolCode.Trim();
                                        break;
                                    case 1:
                                        symbol = veh.VehicleSymbolCode.Trim();
                                        break;
                                    case 2:
                                        symbol = veh.PIPMedLiabilityCode.Trim();
                                        break;
                                }
                                dataRows = symbolTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Symbol='" + symbol + "' " + " AND MinVehYear <= " + veh.VehicleYear + " AND MaxVehYear >= " + veh.VehicleYear);
                                foreach (DataRow dataRow in dataRows)
                                {
                                    vehRatingFactor *= (Decimal)dataRow["Factor"];
                                }
                            }

                            if (veh.StatedAmt > 0)
                            {
                                dataRows = statedValueTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Coverage='" + coverage + "' AND Description='" + veh.VehicleTypeCode + "' AND MinStatedValue <= " + veh.StatedAmt + "  AND MaxStatedValue > " + veh.StatedAmt);
                                foreach (DataRow dataRow in dataRows)
                                {
                                    vehRatingFactor *= (Decimal)dataRow["Factor"];
                                }
                            }

                            long vehYear = VehicleHelper.GetModelYear(veh, pol, stateInfo, connectionString);
                            dataRows = modelYearTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Coverage='" + coverage + "' AND ModelYear = " + vehYear);
                            foreach (DataRow dataRow in dataRows)
                            {
                                vehRatingFactor *= (Decimal)dataRow["Factor"];
                            }

                            foreach (clsVehicleFactor factor in veh.Factors)
                            {
                                dataRows = vehicleFactorTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Coverage='" + coverage + "' AND FactorCode = '" + factor.FactorCode.Trim() + "' ");
                                foreach (DataRow dataRow in dataRows)
                                {
                                    vehRatingFactor *= (Decimal)dataRow["Factor"];
                                }
                            }

                            totalPremium += vehRatingFactor;
                        }
                    }

                    row["VehFactor"] = totalPremium;
                    if (row != null)
                    {
                        vehDataTable.Rows.Add(row);
                    }
                }

            }
            vehDataSet.Tables.Add(vehDataTable);
            return vehDataSet;
        }
         
    }
}
