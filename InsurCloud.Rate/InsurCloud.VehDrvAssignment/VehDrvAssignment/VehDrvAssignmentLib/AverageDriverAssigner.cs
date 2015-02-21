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
    public class AverageDriverAssigner : IAssigner
    {
        public clsEntityDriver DefaultDriver { get; set; }
        public DataRow[] Rows { get; set; }
        public bool NeedsDefaultDriver { get; set; }
        
        public void Execute(List<string> coverageList, List<DataTable> driverFactorTables, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            CalculateIndividualDriverFactors(pol, driverFactorTables, stateInfo, connectionString);
            CreateAverageDriver(pol, driverFactorTables, coverageList, connectionString);
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete)
                {
                    veh.AssignedDriverNum = 99;
                }
            }
        }
        private void CreateAverageDriver(clsPolicyPPA pol, List<DataTable> driverFactorTables, List<string> coverageList, string connectionString)
        {
            DataTable avgFactorTable = null;
            DataTable allDriversTable = null;
            DataRow driverRow = null;
            DataRow avgFactorRow = null;
            DataRow factorRow = null;
            int activeDriverCount = 0;

            try
            {

                for (int x = pol.Drivers.Count - 1; x > 0; x--)
                {
                    if (pol.Drivers[x].IndexNum == 99)
                    {
                        pol.Drivers.Remove(pol.Drivers[x]);
                    }
                }

                clsEntityDriver combinedDriver = new clsEntityDriver();
                combinedDriver.IndexNum = 99;

                FactorsHelper.AddDriverFactor(combinedDriver, "CombinedDriverFactor", coverageList);

                allDriversTable = FactorsHelper.CreateDataTable("AllDrivers", pol, connectionString);
                avgFactorTable = FactorsHelper.CreateDataTable("Factors", pol, connectionString);

                foreach (DataTable factorTable in driverFactorTables)
                {
                    factorRow = DBHelper.GetRow(factorTable, "IDF");
                    driverRow = allDriversTable.NewRow();
                    for (int z = 0; z < allDriversTable.Columns.Count - 1; z++)
                    {
                        driverRow[z] = factorRow[z];
                    }
                    driverRow["FactorName"] = "Driver" + factorTable.TableName;
                    allDriversTable.Rows.Add(driverRow);
                }

                DataTable sortedAllDriversTable = allDriversTable.Clone();

                DataRow[] rows = allDriversTable.Select("", "BI DESC, PD DESC, OTC DESC, COL DESC");
                foreach (DataRow row in rows)
                {
                    sortedAllDriversTable.ImportRow(row);
                }

                int numToRemove = sortedAllDriversTable.Rows.Count - pol.VehicleCount(true);
                if (numToRemove > 0)
                {
                    for (int i = sortedAllDriversTable.Rows.Count - 1; i > pol.VehicleCount(true); i--)
                    {
                        sortedAllDriversTable.Rows.RemoveAt(i);
                    }
                }

                avgFactorRow = avgFactorTable.NewRow();
                avgFactorRow["FactorName"] = "CombinedDriverFactor";

                for (int i = 1; i < sortedAllDriversTable.Columns.Count - 1; i++)
                {
                    if (avgFactorTable.Columns[i].ColumnName == sortedAllDriversTable.Columns[i].ColumnName)
                    {
                        for (int r = 0; r < sortedAllDriversTable.Rows.Count - 1; r++)
                        {
                            factorRow = sortedAllDriversTable.Rows[r];
                            if (factorRow[i] != System.DBNull.Value)
                            {
                                Double val;
                                if (Double.TryParse(factorRow[i].ToString(), out val))
                                {
                                    if (avgFactorRow[i] == System.DBNull.Value) avgFactorRow[i] = 0;
                                    avgFactorRow[i] = (Double)avgFactorRow[i] + (Double)factorRow[i];
                                }
                            }
                        }
                    }
                }

                activeDriverCount = sortedAllDriversTable.Rows.Count;
                //now we have summed up all of the IDF factors now we need 
                //to divide by the total number of drivers
                //we will start with the 2nd column since we know the 1st is the factor name

                for (int i = 1; i < avgFactorTable.Columns.Count - 1; i++)
                {
                    if (avgFactorRow[i] != System.DBNull.Value)
                    {
                        Double val;
                        if (Double.TryParse(avgFactorRow[i].ToString(), out val))
                        {
                            avgFactorRow[i] = (Double)avgFactorRow[i] / activeDriverCount;
                        }
                    }
                }

                if (avgFactorRow != null)
                {
                    avgFactorTable.Rows.Add(avgFactorRow);
                }

                //set the factor amounts on the combined driver
                foreach (clsBaseFactor factor in combinedDriver.Factors)
                {
                    for (int i = 1; i < avgFactorTable.Columns.Count - 1; i++)
                    {
                        if (avgFactorTable.Columns[i].ColumnName.ToUpper() == factor.CovType.ToUpper())
                        {
                            if (avgFactorRow[i] != System.DBNull.Value)
                            {
                                Double val;
                                if (Double.TryParse(avgFactorRow[i].ToString(), out val))
                                {
                                    factor.FactorAmt = (Decimal)avgFactorRow[i];
                                    factor.FactorType = "MidMult";
                                }
                            }
                        }
                    }
                }
                pol.Drivers.Add(combinedDriver);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error Creating Average Driver", ex);
            }
        }
        private void CalculateIndividualDriverFactors(clsPolicyPPA pol, List<DataTable> driverFactorTables, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable factorTable = null;
            DataRow IDFRow = null;
            Double factor = 0;
            Double total = 0;
            Double newTotal = 0;
            double prevTotal = 0;

            for (int i = 0; i < driverFactorTables.Count - 1; i++)
            {
                factorTable = driverFactorTables[i];
                factorTable.Rows.Add(CreateIDFRow(factorTable));

                for (int x = 0; x < factorTable.Rows.Count - 1; x++)
                {
                    switch (factorTable.Rows[x][0].ToString().ToUpper())
                    {
                        case "DRIVERCLASS":
                        case "DRIVERPOINTS":
                            for (int y = 0; y < factorTable.Columns.Count - 1; y++)
                            {
                                newTotal = 0;
                                if (factorTable.Columns[y].ColumnName.ToUpper() == "FACTORTYPE")
                                {
                                    break;
                                }
                                if (factorTable.Rows[x][y] != System.DBNull.Value)
                                {
                                    factor = (Double)factorTable.Rows[x][y];
                                    IDFRow = DBHelper.GetRow(factorTable, "IDF");
                                    total = (Double)IDFRow[y];
                                    if (newTotal == 0)
                                    {
                                        if (total == 1) prevTotal = 0;
                                        prevTotal = total;
                                    }
                                    else
                                    {
                                        prevTotal = newTotal;
                                    }
                                    newTotal = total + factor;
                                    IDFRow[y] = newTotal;
                                }
                            }
                            break;
                    }
                }


                //subtraction of unity
                for (int y = 0; y < factorTable.Columns.Count - 1; y++)
                {
                    newTotal = 0;
                    if (factorTable.Columns[y].ColumnName.ToUpper() == "FACTORTYPE")
                    {
                        break;
                    }
                    IDFRow = DBHelper.GetRow(factorTable, "IDF");
                    if (IDFRow[y] != System.DBNull.Value)
                    {
                        total = (Double)IDFRow[y];
                        if (total == 0)
                        {
                            newTotal = 1;
                        }
                        else
                        {
                            newTotal = total - 1;
                        }
                        IDFRow[y] = newTotal;
                    }
                }

                //now we will take that number and multiply the other factors by it

                //multiplicative factors (FactorMarketPoints, FactorDriverAgePoints, FactorMerit, FactorDriver-individual driver factors)
                for (int x = 0; x < factorTable.Rows.Count - 1; x++)
                {
                    switch (factorTable.Rows[x][0].ToString().ToUpper())
                    {
                        case "MARKETPOINTS":
                        case "DRIVERAGEPOINTS":
                        case "MERIT":
                        case "MERIT1":
                        case "MERIT2":
                        case "MERIT3":
                        case "EXCL":
                        case "NO_VIOL":
                        case "MILITARY":
                        case "SR22":
                        case "FOREIGN_LICENSE":
                        case "CLN_YOUTH":
                        case "ACC_PREV":
                            bool ignore = false;
                            if (factorTable.Rows[x][0].ToString().ToUpper() == "FOREIGN_LICENSE")
                            {
                                if (stateInfo.Contains(pol, "COMBINEDDRIVER", "VIOLGROUPIGNORE", "UDR", connectionString))
                                {
                                    ignore = true;
                                }
                            }
                            if (pol.StateCode == "42")
                            {
                                if (factorTable.Rows[x][0].ToString().ToUpper() == "SR22")
                                {
                                    if (stateInfo.Contains(pol, "COMBINEDDRIVER", "VIOLGROUPIGNORE", "SR22", connectionString))
                                    {
                                        ignore = true;
                                    }
                                }
                            }

                            if (!ignore)
                            {
                                for (int y = 0; y < factorTable.Columns.Count - 1; y++)
                                {
                                    newTotal = 0;
                                    if (factorTable.Columns[y].ColumnName.ToUpper() == "FACTORTYPE")
                                    {
                                        break;
                                    }
                                    if (factorTable.Rows[x][y] != System.DBNull.Value)
                                    {
                                        factor = (Double)factorTable.Rows[x][y];
                                        IDFRow = DBHelper.GetRow(factorTable, "IDF");
                                        total = (Double)IDFRow[y];
                                        if (total == 0) total = 1;
                                        if (newTotal == 0)
                                        {
                                            if (total == 1) prevTotal = 0;
                                            prevTotal = total;
                                        }
                                        else
                                        {
                                            prevTotal = newTotal;
                                        }
                                        newTotal = total * factor;
                                        IDFRow[y] = newTotal;
                                    }
                                }
                            }
                            break;

                    }
                }
            }
        }
        private DataRow CreateIDFRow(DataTable factorTable)
        {
            //IDF = IndividualDriverFactor
            DataRow row = null;

            try
            {
                row = factorTable.NewRow();
                row["FactorName"] = "IDF";
                for (int i = 1; i < factorTable.Columns.Count - 1; i++)
                {
                    if (factorTable.Columns[i].ColumnName.ToUpper() == "FACTORTYPE")
                    {
                        row[i] = "AvgDriver";
                        break;
                    }
                    row[i] = 0;
                }
                return row;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error Creating IDF Row", ex);
            }
            finally
            {
                if (row != null)
                {
                    row = null;
                }
            }
        }

        
    }
}
