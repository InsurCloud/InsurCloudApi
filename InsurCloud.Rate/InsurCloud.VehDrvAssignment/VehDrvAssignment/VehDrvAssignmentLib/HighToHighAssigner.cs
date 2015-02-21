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
    public class HighToHighAssigner : IAssigner
    {
        public DataRow[] Rows { get; set; }
        public bool NeedsDefaultDriver { get; set; }
        public clsEntityDriver DefaultDriver { get; set; }

        public void Execute(List<string> coverageList, List<DataTable> driverFactorTables, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            Rows = ProcessHighToHigh(pol, stateInfo, connectionString);
        }

        private DataRow[] ProcessHighToHigh(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow[] rows = ShouldCreateDefaultDriver(pol, stateInfo, connectionString);
            if (NeedsDefaultDriver)
            {
                CreateDefaultDriver(pol);
            }
            return rows;
        }

        private DataRow[] ShouldCreateDefaultDriver(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataSet vehDataSet = GetVehicleDataSet(pol, connectionString);
            DataTable vehTable = vehDataSet.Tables[0];
            DataSet drvDataSet = GetDriverDataSet(pol, stateInfo, connectionString);
            DataTable drvTable = drvDataSet.Tables[0];
            DataRow[] drvDataRows = null;

            DataRow[] vehDataRows = vehTable.Select("", "Symbol1 Desc, Symbol2 Desc, Symbol3 Desc");
            foreach (DataRow row in vehDataRows)
            {
                foreach (clsVehicleUnit veh in pol.VehicleUnits)
                {
                    if (!veh.IsMarkedForDelete)
                    {
                        if ((int)row["VehicleNum"] == veh.IndexNum)
                        {
                            veh.AssignedDriverNum = 0;
                            drvDataRows = drvTable.Select("", "RankAmt Desc");
                            foreach (DataRow drvRow in drvDataRows)
                            {
                                veh.AssignedDriverNum = (int)drvRow["DriverNum"];
                                drvRow.Delete();
                                break;
                            }
                            if (veh.AssignedDriverNum == 0)
                            {
                                veh.AssignedDriverNum = 98;
                                NeedsDefaultDriver = true;
                            }
                        }
                    }
                }
            }
            return vehDataRows;
        }

        private void CreateDefaultDriver(clsPolicyPPA pol)
        {
            clsEntityDriver drv = new clsEntityDriver();

            drv.Age = pol.PolicyInsured.Age;
            drv.CreditTier = pol.PolicyInsured.CreditTier;
            drv.DriverStatus = "DEFAULT";
            drv.Gender = pol.PolicyInsured.Gender;
            drv.IndexNum = 98;
            drv.MaritalStatus = pol.PolicyInsured.MaritalStatus;
            drv.Points = 0;
            drv.SR22 = false;
            drv.UWTier = pol.PolicyInsured.UWTier;

            pol.Drivers.Add(drv);
            if (drv != null)
            {
                drv = null;
            }       
        }

        private DataSet GetVehicleDataSet(clsPolicyPPA pol, string connectionString)
        {
            DataSet vehDataSet = new DataSet();
            DataTable vehDataTable = new DataTable("Vehicles");

            //DataColumn col = null;
            DataRow row = null;

            vehDataTable.Columns.Add(AddColumn("VehicleNum"));
            vehDataTable.Columns.Add(AddColumn("Symbol1"));
            vehDataTable.Columns.Add(AddColumn("Symbol2"));
            vehDataTable.Columns.Add(AddColumn("Symbol3"));

            //SYMBOL
            DataRow[] rows = null;
            DataTable symbolTable = LoadFactorSymbolTable(pol, connectionString);

            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                row = vehDataTable.NewRow();
                row["VehicleNum"] = veh.IndexNum;
                string symbol = "";
                string column = "";
                Decimal symbolFactor = 0;

                //SYMBOL1 = LiabilitySymbolCode
                //SYMBOL2 = VehicleSymbolCode
                //SYMBOL3 = PIPMedSymbolCode
                for (int i = 0; i < 3; i++)
                {
                    symbol = "";
                    column = "";
                    symbolFactor = 0;
                    switch (i)
                    {
                        case 0:
                            symbol = veh.LiabilitySymbolCode.Trim();
                            column = "Symbol1";
                            break;
                        case 1:
                            symbol = veh.VehicleSymbolCode.Trim();
                            column = "Symbol2";
                            break;
                        case 2:
                            symbol = veh.PIPMedLiabilityCode.Trim();
                            column = "Symbol3";
                            break;
                    }
                    rows = symbolTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Symbol='" + symbol + "' " + " AND MinVehYear <= '" + veh.VehicleYear + "' " + " AND MaxVehYear >= '" + veh.VehicleYear + "' ");
                    foreach (DataRow dataRow in rows)
                    {
                        symbolFactor += (Decimal)dataRow["Factor"];
                    }
                    row[column] = symbolFactor;
                }
                if (row != null)
                {
                    vehDataTable.Rows.Add(row);
                }
            }
            vehDataSet.Tables.Add(vehDataTable);
            return vehDataSet;
        }

        private DataTable LoadFactorSymbolTable(clsPolicyPPA pol, string connectionString)
        {
            string driverSymbol = GetVehicleSymbols(pol);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string SQL = " SELECT Program, Coverage, Symbol, MinVehYear, MaxVehYear, Factor, FactorType ";
                    SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorSymbol with(nolock)";
                    SQL += " WHERE EffDate <= @RateDate ";
                    SQL += " AND ExpDate > @RateDate ";
                    SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                    SQL += " AND Symbol IN (" + driverSymbol + ") ";
                    SQL += " ORDER BY Program, Symbol, Coverage";


                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL, conn);

                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = pol.RateDate;
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = pol.AppliesToCode;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "FactorSybmol");
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to load factor symbol table for product/statecode " + pol.Product + pol.StateCode + " : " + ex.Message, ex);
            }
        }

        private static string GetVehicleSymbols(clsPolicyPPA pol)
        {
            string driverSymbol = "";
            int driverCount = 0;

            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                string symbol = String.Empty;
                symbol = "'" + veh.LiabilitySymbolCode + "', '" + veh.VehicleSymbolCode + "', '" + veh.PIPMedLiabilityCode + "'";
                if (driverCount == 0)
                {
                    driverSymbol = symbol;
                }
                else
                {
                    driverSymbol = driverSymbol + "," + symbol;
                }
                driverCount++;
            }
            return driverSymbol;
        }

        private static DataColumn AddColumn(string columnName)
        {
            return new DataColumn(columnName);
        }

        private DataSet GetDriverDataSet(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataSet drvDataSet = new DataSet();
            DataTable drvDataTable = new DataTable("Drivers");
            DataRow row = null;
            Decimal rankAmt = 0;

            drvDataTable.Columns.Add(AddColumn("DriverNum"));
            drvDataTable.Columns.Add("RankAmount", System.Type.GetType("System.Decimal"));
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete && DriverHelper.ShouldRateDriver(drv, pol))
                {
                    rankAmt = 0;
                    row = drvDataTable.NewRow();
                    row["DriverNum"] = drv.IndexNum;

                    DataRow[] rows = null;
                    DataTable driverClassTable = LoadFactorDriverClassTable(pol, connectionString);
                    string driverClass = "";

                    driverClass = DriverHelper.GetDriverClassDefinition(drv, pol, stateInfo, connectionString);
                    rows = driverClassTable.Select("Program IN ('PPA', '" + pol.Program + "') AND DriverClass='" + driverClass + "' ");
                    foreach (DataRow r in rows)
                    {
                        rankAmt += (Decimal)r["Factor"];
                    }

                    DataTable drvPointsTable = LoadFactorDriverPointsTable(pol, connectionString);

                    //Only do this if the rate data > a date in the stateinfo table; otherwise there will be rate discrepancies
                    int ratedPoints = 0;
                    foreach (DataRow r in drvPointsTable.Rows)
                    {
                        int points = 0;
                        points = Int32.Parse(r["Points"].ToString());
                        if (points <= drv.Points && points > ratedPoints)
                        {
                            ratedPoints = points;
                        }
                    }

                    DateTime fixDate = GetDriverPointsFixStartDate(pol, stateInfo, connectionString);

                    if (pol.RateDate > fixDate)
                    {
                        rows = drvPointsTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Points='" + ratedPoints + "' ");
                    }
                    else
                    {
                        rows = drvPointsTable.Select("Program IN ('PPA', '" + pol.Program + "') AND Points='" + drv.Points + "' ");
                    }

                    foreach (DataRow r in rows)
                    {
                        rankAmt += (Decimal)r["Factor"];
                    }
                    row["RankAmt"] = rankAmt;
                    if (row != null)
                    {
                        drvDataTable.Rows.Add(row);
                    }
                }
            }
            drvDataSet.Tables.Add(drvDataTable);
            return drvDataSet;
        }

        private DateTime GetDriverPointsFixStartDate(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DateTime fixDate = DateTime.MinValue;
            DataRow[] rows = stateInfo.GetRows(pol, "DRIVER", "ASSIGNMENT", "POINTSOVER15FIX", connectionString); 
            foreach (DataRow row in rows)
            {
                fixDate = Convert.ToDateTime(row["ItemValue"].ToString());
            }
            return fixDate;
        }

        private DataTable LoadFactorDriverPointsTable(clsPolicyPPA pol, string connectionString, string withFilter = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string SQL = " SELECT Program, Coverage, Points, Factor, FactorType";
                    SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverPoints with(nolock)";
                    SQL += " WHERE EffDate <= @RateDate ";
                    SQL += " AND ExpDate > @RateDate ";
                    SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                    if (withFilter != String.Empty)
                    {
                        SQL += " AND Points IN (" + withFilter + ")";
                    }
                    SQL += " ORDER BY Program, Points, Coverage";


                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL, conn);

                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = pol.RateDate;
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = pol.AppliesToCode;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "FactorDriverPoints");
                    return ds.Tables[0];                        
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to load factor driver class table for product/statecode " + pol.Product + pol.StateCode + " : " + ex.Message, ex);
            }
        }

        private DataTable LoadFactorDriverClassTable(clsPolicyPPA pol, string connectionString, string withFilter = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string SQL = " SELECT Program, Coverage, DriverClass, Factor, FactorType";
                    SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverClass with (nolock)";
                    SQL += " WHERE EffDate <= @RateDate ";
                    SQL += " AND ExpDate > @RateDate ";
                    SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                    if (withFilter == string.Empty)
                    {
                        SQL += " AND DriverClass IN (" + withFilter + ")";
                    }
                    SQL += " ORDER BY Program, DriverClass, Coverage";


                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL, conn);

                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = pol.RateDate;
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = pol.AppliesToCode;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds, "FactorDriverClass");
                    return ds.Tables[0];                        
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to load factor driver class table for product/statecode " + pol.Product + pol.StateCode + " : " + ex.Message, ex);
            }
        }
    }
}
