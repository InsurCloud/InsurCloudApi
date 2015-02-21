using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace Helpers
{
    public static class FactorsHelper
    {
        public static void RemoveAutoApplyFactors(clsPolicyPPA pol, DataTable factorTable)
        {
            DataRow[] rows = factorTable.Select("Program IN ('PPA', '" + pol.Program + "') AND AutoApply = 1 ");
            switch (factorTable.TableName.ToUpper())
            {
                case "FACTORPOLICY":
                    for (int i = pol.PolicyFactors.Count - 1; i >= 0; i--)
                    {
                        foreach (DataRow row in rows)
                        {
                            if (row["FactorCode"].ToString().ToUpper() == pol.PolicyFactors[i].FactorCode.ToUpper())
                            {
                                pol.PolicyFactors.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    break;
                case "FACTORDRIVER":
                    DriverHelper.RemoveAutoApplyFactors(rows, pol);
                    break;
                case "FACTORVEHICLE":
                    VehicleHelper.RemoveAutoApplyFactors(rows, pol);
                    break;
            }
        }

        public static bool RatedFactorExists(DataTable factorTable, string factorName)
        {
            foreach (DataRow row in factorTable.Rows)
            {
                if (row["FactorName"].ToString().Trim().ToUpper() == factorName.Trim().ToUpper()) return true;
            }
            return false;
        }

        public static bool FactorOn(List<clsBaseFactor> factors, string factorCode)
        {
            foreach (clsBaseFactor factor in factors)
            {
                if (factor.FactorCode.ToString().ToUpper() == factorCode.ToString().ToUpper())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool FactorOn(List<clsVehicleFactor> factors, string factorCode)
        {
            foreach (clsVehicleFactor factor in factors)
            {
                if (factor.FactorCode.ToString().ToUpper() == factorCode.ToString().ToUpper())
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddFactor(clsPolicyPPA pol, List<clsBaseFactor> factors, string factorCode, string factorTypeCode, string connectionString)
        {
            clsBaseFactor factor = new clsBaseFactor();
            factor.FactorCode = factorCode;
            factor.IndexNum = factors.Count + 1;
            factor.SystemCode = factorCode;
            factor.FactorNum = factors.Count + 1;
            factor.FactorAmt = 0;
            factor.FactorDesc = GetFactorDesc(pol, factorCode, factorTypeCode, connectionString);
            factor.FactorName = factor.FactorDesc;
            factors.Add(factor);
        }

        public static void AddFactor(clsPolicyPPA pol, List<clsVehicleFactor> factors, string factorCode, string factorTypeCode, string connectionString)
        {
            clsVehicleFactor factor = new clsVehicleFactor();
            factor.FactorCode = factorCode;
            factor.IndexNum = factors.Count + 1;
            factor.SystemCode = factorCode;
            factor.FactorNum = factors.Count + 1;
            factor.FactorAmt = 0;
            factor.FactorDesc = GetFactorDesc(pol, factorCode, factorTypeCode, connectionString);
            factor.FactorName = factor.FactorDesc;
            factors.Add(factor);
        }

        public static string GetFactorDesc(clsPolicyPPA pol, string factorCode, string factorTypeCode, string connectionString)
        {
            DataRow[] rows;
            string factorDesc = "";
            DataTable factorTable = null;
            switch (factorTypeCode.ToUpper())
            {
                case "POLICY":
                    factorTable = LoadFactorPolicyTable(pol, connectionString);
                    break;
                case "DRIVER":
                    factorTable = LoadFactorDriverTable(pol, connectionString);
                    break;
                case "VEHICLE":
                    factorTable = LoadFactorVehicleTable(pol, connectionString);
                    break;
            }

            rows = factorTable.Select("Program='" + pol.Program + "' AND FactorCode='" + factorCode + "'");

            foreach (DataRow row in rows)
            {
                factorDesc = row["Description"].ToString();
                break;
            }
            return factorDesc;
        }

        private static DataTable LoadFactorVehicleTable(clsPolicyPPA pol, string connectionString)
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

        private static DataTable LoadFactorDriverTable(clsPolicyPPA pol, string connectionString)
        {
            string SQL = "";

            SQL = " SELECT Program, Coverage, FactorCode, Description, AutoApply, Factor, FactorType ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + ".." + "FactorDriver with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " ORDER BY Program, FactorCode, Coverage ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            return DBHelper.GetDataTable(SQL, "FactorDriver", connectionString, parms);
        }

        private static DataTable LoadFactorPolicyTable(clsPolicyPPA pol, string connectionString)
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

        public static bool CheckForPIPXFactor(clsPolicyPPA pol, string pipType)
        {
            bool addFactor = false;
            if (pol.VehicleUnits.Count() > 0)
            {
                foreach (clsPACoverage cov in pol.VehicleUnits[0].Coverages)
                {
                    if (cov.CovGroup == "PIP")
                    {
                        if (cov.UWQuestions.Count > 0)
                        {
                            foreach (clsUWQuestion question in cov.UWQuestions)
                            {
                                if (question.AnswerText.ToUpper().Trim() == "YES")
                                {
                                    if (cov.CovCode.Contains(pipType))
                                    {
                                        addFactor = true;
                                    }
                                }
                                else
                                {
                                    if (question.AnswerText.ToUpper().Trim().Contains(pipType))
                                    {
                                        addFactor = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return addFactor;
        }
        
        public static void RemoveFactor(string factorCode, List<clsBaseFactor> factors)
        {
            for (int i = factors.Count - 1; i >= 0; i--)
            {
                if (factors[i].FactorCode.ToUpper() == factorCode)
                {
                    factors.RemoveAt(i);
                    break;
                }
            }
        }

        public static void GetFactor(string factorName, DataTable dataTable, DataTable factorTable, clsPolicyPPA pol)
        {
            bool factorTypeAdded = false;
            DataRow factorRow = null;
            
            factorRow = null;
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                factorRow = factorTable.NewRow();
                factorRow["FactorName"] = factorName;

                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 1; i < factorTable.Columns.Count; i++)
                    {
                        if (row["Coverage"].ToString() == factorTable.Columns[i].ColumnName)
                        {
                            factorRow[row["Coverage"].ToString()] = row["Factor"];
                            break;
                        }

                    }
                    if (!factorTypeAdded)
                    {
                        factorRow["FactorType"] = row["FactorType"];
                        factorTypeAdded = true;
                    }
                }
                if (factorRow != null)
                {
                    factorTable.Rows.Add(factorRow);
                }
            }
        }

        public static void GetFactorUsingCoverages(clsVehicleUnit veh, string factorName, DataTable dataTable, DataTable factorTable, clsPolicyPPA pol)
        {
            bool factorTypeAdded = false;
            DataRow factorRow = null;

            factorRow = null;
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                factorRow = factorTable.NewRow();
                factorRow["FactorName"] = factorName;

                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 1; i < factorTable.Columns.Count; i++)
                    {
                        foreach (clsPACoverage cov in veh.Coverages)
                        {
                            if (!cov.IsMarkedForDelete)
                            {
                                if (row["Code"].ToString() == cov.CovCode)
                                {
                                    if (factorName.ToUpper() == "COVERAGEADD")
                                    {
                                        factorRow[row["Coverage"].ToString()] = PolicyHelper.UpdateMidAddFactorBasedOnTerm(decimal.Parse(row["Factor"].ToString()), pol);
                                    }
                                    else
                                    {
                                        factorRow[row["Coverage"].ToString()] = row["Factor"];
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    if (!factorTypeAdded)
                    {
                        factorRow["FactorType"] = row["FactorType"];
                        factorTypeAdded = true;
                    }
                }
                if (factorRow != null)
                {
                    factorTable.Rows.Add(factorRow);
                }
            }
        }

        public static void GetFactorUsingCapped(DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsBaseFactor factor, DataTable dataTable)
        {

            bool factorTypeAdded = false;
            DataRow factorRow = null;
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                factorRow = factorTable.NewRow();
                factorRow["FactorName"] = factor.FactorCode;
                DataRow totalsRow = null;
                DataRow maxDiscountRow = null;
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 1; i < factorTable.Columns.Count; i++)
                    {
                        bool isCappedFactor = false;
                        for (int q = 0; q < cappedFactorList.Count; q++)
                        {
                            if (factor.FactorCode.ToUpper() == cappedFactorList[q].ToUpper())
                            {
                                isCappedFactor = true;
                                break;
                            }
                        }
                        if (isCappedFactor)
                        {
                            totalsRow = DBHelper.GetRow(cappedFactors, "Totals");
                            maxDiscountRow = DBHelper.GetRow(cappedFactors, "MaxDiscountAmt");
                            if (decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString()) != 0 && decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString()) <= decimal.Parse(maxDiscountRow[row["Coverage"].ToString()].ToString()))
                            {
                                //no more discounts, set this to 1.0 and add it to the data row
                                factorRow[row["Coverage"].ToString()] = 1;
                                break;
                            }
                            else if (decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString()) != 0 && decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString()) * decimal.Parse(row["Factor"].ToString()) <= decimal.Parse(maxDiscountRow[row["Coverage"].ToString()].ToString()))
                            {
                                //set the factor to the difference between the MaxAmount and the current total
                                decimal discount = 0;
                                discount = decimal.Parse(maxDiscountRow[row["Coverage"].ToString()].ToString()) / decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString());
                                factorRow[row["Coverage"].ToString()] = discount;
                                totalsRow[row["Coverage"].ToString()] = decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString()) * discount;
                            }
                            else
                            {
                                //add it to the data row
                                factorRow[row["Coverage"].ToString()] = decimal.Parse(row["Factor"].ToString());
                                decimal multiple = 0;
                                multiple = (decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString()) == 0) ? 1 : decimal.Parse(totalsRow[row["Coverage"].ToString()].ToString());
                                totalsRow[row["Coverage"].ToString()] = decimal.Parse(row["Factor"].ToString()) * multiple;
                            }
                        }
                        else
                        {
                            //add it to the data row
                            factorRow[row["Coverage"].ToString()] = decimal.Parse(row["Factor"].ToString());
                            break;
                        }
                    }
                    if (!factorTypeAdded)
                    {
                        factorRow["FactorType"] = row["FactorType"];
                        factorTypeAdded = true;
                    }
                }
                if (factorRow != null)
                {
                    factorTable.Rows.Add(factorRow);
                }
            }
        }

        public static void GetCombinedDriverFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (veh.AssignedDriverNum == 99)
            {
                //average driver, don't need to lookup driver factors
                //add combined driver factors to data table
                DataRow factorRow = null;
                factorRow = factorTable.NewRow();
                factorRow["FactorName"] = "CombinedDriver";
                clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);
                foreach (clsBaseFactor factor in drv.Factors)
                {
                    for (int i = 1; i < factorTable.Columns.Count; i++)
                    {
                        if (factor.CovType.ToUpper() == factorTable.Columns[i].ColumnName.ToUpper())
                        {
                            factorRow[factorTable.Columns[i].ColumnName] = factor.FactorAmt;
                            break;
                        }
                    }
                    factorRow["FactorType"] = factor.FactorType;
                }

                if (factorRow != null)
                {
                    factorTable.Rows.Add(factorRow);
                }

            }
        }

        public static void GetDriverAdjustmentFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);
            string driverClass = DriverHelper.GetDriverClassDefinition(drv, pol, stateInfo, connectionString);
            DataTable dataTable = GetDriverAdjustmentTable(pol, drv, driverClass, connectionString);
            GetFactor("DriverAdjust", dataTable, factorTable, pol);
        }
        public static DataTable GetDriverAdjustmentTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverClass = DriverHelper.GetDriverClassDefinitions(pol, stateInfo, connectionString);
            string driverPoints = DriverHelper.GetDriverPoints(pol);
            string SQL = " SELECT Program, Coverage, Points, DriverClass, Factor, FactorType ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverAdjustment with(nolock)";
            SQL += GetBasicWhereClause();
            if (driverClass != string.Empty)
            {
                SQL += " AND DriverClass IN (" + driverClass + ")";
            }
            if (driverPoints != string.Empty)
            {
                SQL += " AND Points IN (" + driverPoints + ")";
            }
            SQL += " ORDER BY Program, Coverage ";

            List<SqlParameter> parms = GetBasicParms(pol);
            return DBHelper.GetDataTable(SQL, "FactorDriverAdjustment", connectionString, parms);
        }

        private static DataTable GetDriverAdjustmentTable(clsPolicyPPA pol, clsEntityDriver drv, string driverClass, string connectionString)
        {
            string SQL = " SELECT Coverage, Points, DriverClass, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverAdjustment with(nolock)";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Points = (SELECT TOP 1 Points ";
            SQL += "                FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverAdjustment with(nolock) ";
            SQL += "                WHERE Cast(Points As Int) <= Cast(@Points As Int) ";
            SQL += "                ORDER BY Cast(Points As Int) Desc) ";
            SQL += " AND DriverClass = @DriverClass ";
            SQL += " ORDER BY Coverage Asc ";


            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@Points", SqlDbType.VarChar, 2, drv.Points));
            parms.Add(DBHelper.AddParm("@DriverClass", SqlDbType.VarChar, 8, driverClass));
            return DBHelper.GetDataTable(SQL, "DriverAdjustmentFactor", connectionString, parms);
        }

        public static void GetDriverFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);
            if (drv != null)
            {
                foreach (clsBaseFactor factor in drv.Factors)
                {
                    DataTable dataTable = GetDriverFactorTable(pol, drv, stateInfo, connectionString, factor);
                    GetFactorUsingCapped(factorTable, cappedFactors, cappedFactorList, factor, dataTable);
                }
            }
        }
        public static DataTable GetDriverFactorTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            return GetBasicFactorTable("FactorDriver", pol, connectionString, " SELECT Program, Coverage, Description, FactorCode, Factor, FactorType FROM ");
        }
        //public static delegate DataTable MyFunction(clsPolicyPPA pol, clsEntityDriver drv, string connectionString, clsBaseFactor factor);
        public static DataTable GetDriverFactorTable(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString, clsBaseFactor factor)
        {
            //Set driver class not available on object 
            string SQL = "SELECT Coverage, Description, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorDriver with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND FactorCode = @FactorCode ";
            SQL += " ORDER BY Coverage Asc ";


            List<SqlParameter> parms = GetBasicParms(pol);
            parms.Add(DBHelper.AddParm("@FactorCode", SqlDbType.VarChar, 20, factor.FactorCode));

            DataTable dataTable = DBHelper.GetDataTable(SQL, "FactorDriver", connectionString, parms);
            return dataTable;
        }
        
        public static void GetDriverAgePointsFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);
            if (drv != null)
            {
                string SQL = string.Empty;
                List<SqlParameter> parms = new List<SqlParameter>();
                clsBaseFactor factor = new clsBaseFactor();
                DataTable dataTable = GetDriverAgePointsTable(pol, drv, stateInfo, connectionString, factor);
                GetFactor("DriverAgePoint", dataTable, factorTable, pol);
            }
        }
        public static DataTable GetDriverAgePointsTable(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString, clsBaseFactor factor)
        {
            drv.Points = DriverHelper.CleanViolationPoints(pol, drv, stateInfo, connectionString, false);
            string SQL = string.Empty;
            List<SqlParameter> parms = new List<SqlParameter>();
            GetDriverAgePointsTable(pol, drv, out SQL, out parms);
            return DBHelper.GetDataTable(SQL, "DriverAgePointsFactor", connectionString, parms);
        }
        private static void GetDriverAgePointsTable(clsPolicyPPA pol, clsEntityDriver drv, out string SQL, out List<SqlParameter> parms)
        {
            //Set driver class not available on object 
            SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverAgePoints with(nolock) ";
            SQL += GetBasicWhereClause();
            SQL += " AND MinAge <= @InsuredAge ";
            SQL += " AND MaxAge > @InsuredAge ";
            SQL += " AND MinPoints <= @Points ";
            SQL += " AND MaxPoints > @Points ";
            SQL += " ORDER BY Coverage Asc ";


            parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@InsuredAge", SqlDbType.Int, 22, drv.Age));
            parms.Add(DBHelper.AddParm("@Points", SqlDbType.Int, 22, drv.Points));
        }

        public static void GetDriverClassFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);                        
            clsBaseFactor factor = new clsBaseFactor();
            DataTable dataTable = GetDriverClassTable(pol, drv, stateInfo, connectionString, factor);
            GetFactor("DriverClass", dataTable, factorTable, pol);
        }
        public static DataTable GetDriverClassTable(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString, clsBaseFactor factor)
        {
            drv.Age = DriverHelper.GetDriverAge(drv, pol, stateInfo, connectionString);
            string driverClass = DriverHelper.GetDriverClassDefinition(drv, pol, stateInfo, connectionString);
            //Set driver class not available on object 
            string SQL = string.Empty;
            List<SqlParameter> parms = new List<SqlParameter>();
            GetDriverClassFactorTable(pol, driverClass, out SQL, out parms);
            return DBHelper.GetDataTable(SQL, "DriverClassFactor", connectionString, parms);
        }
        public static DataTable GetDriverClassFactorTableFilterByDriverClass(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString){
            string driverClass = "";
            driverClass = DriverHelper.GetDriverClassDefinitions(pol, stateInfo, connectionString);
            return FactorsHelper.GetDriverClassFactorTable(pol, connectionString, driverClass);
        }
        public static DataTable GetDriverClassFactorTable(clsPolicyPPA pol, string connectionString, string withFilter = "")
        {
            string SQL = " SELECT Program, Coverage, DriverClass, Factor, FactorType";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverClass with (nolock)";
            SQL += GetBasicWhereClause();
            if (withFilter == string.Empty)
            {
                SQL += " AND DriverClass IN (" + withFilter + ")";
            }
            SQL += " ORDER BY Program, DriverClass, Coverage";
            
            List<SqlParameter> parms = GetBasicParms(pol);
            return DBHelper.GetDataTable(SQL, "FactorDriverClass", connectionString, parms);

        }
        private static void GetDriverClassFactorTable(clsPolicyPPA pol, string driverClass, out string SQL, out List<SqlParameter> parms)
        {
            SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverClass with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND DriverClass = @DriverClass ";
            SQL += " ORDER BY Coverage Asc ";


            parms = GetBasicParms(pol);
            parms.Add(DBHelper.AddParm("@DriverClass", SqlDbType.VarChar, 8, driverClass));
        }

        public static void GetDriverPointsFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);            
            clsBaseFactor factor = new clsBaseFactor();
            DataTable dataTable = GetDriverPointsTable(pol, drv, stateInfo, connectionString, factor);
            GetFactor("DriverPoints", dataTable, factorTable, pol);
        }
        public static DataTable GetDriverPointsTable(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString, clsBaseFactor factor)
        {
            drv.Points = DriverHelper.CleanViolationPoints(pol, drv, stateInfo, connectionString, true);
            string SQL = string.Empty;
            List<SqlParameter> parms = new List<SqlParameter>();
            GetDriverPointsTable(pol, drv, out SQL, out parms);
            return DBHelper.GetDataTable(SQL, "FactorDriverPoints", connectionString, parms);
        }
        public static DataTable GetDriverPointsTableFilterByPolicyPoints(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverPoints = DriverHelper.GetDriverPoints(pol);
            return GetDriverPointsTable(pol, stateInfo, connectionString, driverPoints);
        }
        public static DataTable GetDriverPointsTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, string withFilter = ""){
            string SQL = " SELECT Program, Coverage, Points, Factor, FactorType";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverPoints with(nolock)";
            SQL += GetBasicWhereClause();
            if (withFilter != String.Empty)
            {
                SQL += " AND Points IN (" + withFilter + ")";
            }
            SQL += " ORDER BY Program, Points, Coverage";
            List<SqlParameter> parms = GetBasicParms(pol);
            return DBHelper.GetDataTable(SQL, "FactorDriverPoints", connectionString, parms);
        }
        private static void GetDriverPointsTable(clsPolicyPPA pol, clsEntityDriver drv, out string SQL, out List<SqlParameter> parms)
        {
            SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverPoints with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += "  AND Points = (SELECT TOP 1 Points ";
            SQL += "                 FROM pgm" + pol.Product + pol.StateCode + "..FactorDriverPoints  with(nolock) ";
            SQL += "                 WHERE Cast(Points As Int) <= Cast(@Points As Int) ";
            SQL += "                  AND EffDate <= @RateDate ";
            SQL += "                  AND ExpDate > @RateDate ";
            SQL += "                  AND Program = @Program ";
            SQL += "                  AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += "                 ORDER BY Cast(Points As Int) Desc) ";
            SQL += " ORDER BY Coverage Asc ";


            parms = GetBasicParms(pol);
            parms.Add(DBHelper.AddParm("@Points", SqlDbType.VarChar, 8, (drv.Points > 30) ? 30 : drv.Points));
        }

        public static void GetMarketPointsFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            clsEntityDriver drv = DriverHelper.FindDriverByAssignment(pol, veh.AssignedDriverNum);
            clsBaseFactor factor = new clsBaseFactor();       
            DataTable dataTable = GetMarketPointsTable(pol, drv, stateInfo, connectionString, factor);
            GetFactor("MarketPoints", dataTable, factorTable, pol);

        }
        public static DataTable GetMarketPointsTable(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString, clsBaseFactor factor)
        {
            string driverClass = DriverHelper.GetDriverClassDefinition(drv, pol, stateInfo, connectionString);
            string SQL = string.Empty;
            List<SqlParameter> parms = new List<SqlParameter>();
            GetMarketPointsTable(pol, drv, out SQL, out parms);
            return DBHelper.GetDataTable(SQL, "FactorMarketPoints", connectionString, parms);
        }
        private static void GetMarketPointsTable(clsPolicyPPA pol, clsEntityDriver drv, out string SQL, out List<SqlParameter> parms)
        {
            SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorMarketPoints with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND UWTier = @UWTier ";
            SQL += " AND MinPoints <= @Points ";
            SQL += " AND MaxPoints > @Points ";
            SQL += " ORDER BY Coverage Asc ";


            parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@UWTier", SqlDbType.VarChar, 3, pol.PolicyInsured.UWTier));
            parms.Add(DBHelper.AddParm("@Points", SqlDbType.VarChar, 8, (drv.Points > 30) ? 30 : drv.Points));
        }

        public static void GetBaseRateFactor(DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable dataTable = GetBaseRateTable(pol, connectionString);
            GetFactor("BaseRate", dataTable, factorTable, pol);
        }

        public static DataTable GetBaseRateTable(clsPolicyPPA pol, string connectionString)
        {
            return GetBasicFactorTable("FactorBaseRate", pol, connectionString);
        }

        public static DataTable GetBasicFactorTable(string factorTableName, clsPolicyPPA pol, string connectionString, string selectClause = " SELECT Program, Coverage, Factor, FactorType FROM ")
        {
            //Set driver class not available on object 
            string SQL = selectClause;
            SQL += "pgm" + pol.Product + pol.StateCode + ".." + factorTableName + " with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " ORDER BY Coverage Asc ";

            return DBHelper.GetDataTable(SQL, factorTableName, connectionString, GetBasicParms(pol));
        }        
        private static string GetBasicWhereClause()
        {
            string SQL = " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            return SQL;
        }
        private static List<SqlParameter> GetBasicParms(clsPolicyPPA pol)
        {
            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            return parms;
        }

        public static void GetHouseholdStructureFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable dataTable = GetHouseholdStructureTable(pol, stateInfo, connectionString);
            GetFactor("HouseholdStructure", dataTable, factorTable, pol);
        }
        private static DataTable GetHouseholdStructureTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Set driver class not available on object 
            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorHouseholdStructure with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND MultiCar = @MultiCar";
            SQL += " AND MaritalStatus = @MaritalStatus ";
            SQL += " AND Youthful = @Youthful ";
            SQL += " AND HomeOwner = @HomeOwner ";
            SQL += " AND PCRelationship = @PCRelationship ";
            SQL += " ORDER BY Coverage Asc ";


            List<SqlParameter> parms = new List<SqlParameter>();
            parms = GetBasicParms(pol);
            parms.Add(DBHelper.AddParm("@MultiCar", SqlDbType.VarChar, 1, (VehicleHelper.VehicleCount(pol) > 1) ? "Y" : "N"));
            parms.Add(DBHelper.AddParm("@MaritalStatus", SqlDbType.VarChar, 1, DriverHelper.GetMaritalStatus(pol, stateInfo, connectionString)));
            parms.Add(DBHelper.AddParm("@Youthful", SqlDbType.VarChar, 1, (pol.PolicyInsured.Age < 21) ? "Y" : "N"));
            parms.Add(DBHelper.AddParm("@HomeOwner", SqlDbType.VarChar, 1, (pol.PolicyInsured.OccupancyType.ToUpper() == "HOMEOWNER") ? "Y" : "N"));
            parms.Add(DBHelper.AddParm("@PCRelationship", SqlDbType.VarChar, 1, (DriverHelper.GetParentChildRelationshipIndicator(pol, stateInfo, connectionString)) ? "Y" : "N"));
            return DBHelper.GetDataTable(SQL, "FactorHouseholdStructure", connectionString, parms);
        }

        public static void GetModelYearFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable dataTable = GetModelYearTable(pol, veh, stateInfo, connectionString);
            GetFactor("ModelYear", dataTable, factorTable, pol);
        }
        public static DataTable GetModelYearTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string modelYears = getModelYears(pol);

            string SQL = " SELECT Program, Coverage, ModelYear, Factor, FactorType";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorModelYear with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND ModelYear IN(" + modelYears + ")";
            SQL += " ORDER BY Program, ModelYear, Coverage";

            List<SqlParameter> parms = GetBasicParms(pol);

            return DBHelper.GetDataTable(SQL, "FactorModelYear", connectionString, parms);
        }
        private static string getModelYears(clsPolicyPPA pol)
        {
            string modelYears = "";
            int vehCount = 0;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (vehCount > 0) modelYears += ", ";
                modelYears += getModelYear(pol, veh);
                vehCount++;
            }
            return modelYears;
        }
        private static long getModelYear(clsPolicyPPA pol, clsVehicleUnit veh)
        {
            long vehYear = 0;
            if (pol.Program.ToUpper() == "MONTHLY")
            {
                vehYear = veh.VehicleAge;
            }
            else
            {
                if (Int32.Parse(veh.VehicleYear) < 1980 && Int32.Parse(veh.VehicleYear) > 1)
                {
                    vehYear = 1980;
                }
                else if (Int32.Parse(veh.VehicleYear) > DateTime.Now.Year)
                {
                    vehYear = DateTime.Now.Year;
                }
                else
                {
                    vehYear = Int32.Parse(veh.VehicleYear);
                }
            }
            return vehYear;
        }
        public static DataTable GetModelYearTable(clsPolicyPPA pol, clsVehicleUnit veh, StateInfoHelper stateInfo, string connectionString){
            //Set driver class not available on object 
            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorModelYear with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND VehicleCountType IN ('A', @VehicleCountType) ";
            SQL += " AND ModelYear = (Select Max( Cast(ModelYear As Int)) ";
            SQL += "                     FROM pgm" + pol.Product + pol.StateCode + "..FactorModelYear with(nolock)";
            SQL += "                     WHERE(ModelYear <= @ModelYear)";
            SQL += "                         AND Program = @Program ";
            SQL += "                         AND EffDate <= @RateDate ";
            SQL += "                         AND ExpDate > @RateDate ";
            SQL += " 						 AND VehicleCountType IN ('A', @VehicleCountType ) ";
            SQL += "                         AND AppliesToCode IN ('B',  @AppliesToCode ) )";
            SQL += " ORDER BY Coverage Asc ";


            List<SqlParameter> parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@ModelYear", SqlDbType.VarChar, 4, VehicleHelper.GetModelYear(veh, pol, stateInfo, connectionString).ToString()));
            parms.Add(DBHelper.AddParm("@VehicleCountType", SqlDbType.VarChar, 1, VehicleHelper.GetVehicleCountType(pol, stateInfo, connectionString)));
            return DBHelper.GetDataTable(SQL, "FactorModelYear", connectionString, parms);
        }

        public static void GetPolicyFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsBaseFactor factor in pol.PolicyFactors)
            {
                DataTable dataTable = GetPolicyFactorTable(factor, pol, stateInfo, connectionString);
                GetFactorUsingCapped(factorTable, cappedFactors, cappedFactorList, factor, dataTable);
            }
        }
        private static DataTable GetPolicyFactorTable(clsBaseFactor factor, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Set driver class not available on object 
            string SQL = " SELECT Coverage, Description, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorPolicy with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND FactorCode = @FactorCode ";
            SQL += " ORDER BY Coverage Asc ";


            List<SqlParameter> parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@FactorCode", SqlDbType.VarChar, 20, factor.FactorCode));
            return DBHelper.GetDataTable(SQL, "FactorPolicyDiscountMatrix", connectionString, parms);
        }

        public static void GetPolicyDiscountMatrixFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable dataTable = GetPolicyDiscountMatrixTable(pol, stateInfo, connectionString);
            GetFactor("PolicyDiscountMatrix", dataTable, factorTable, pol);
        }
        private static DataTable GetPolicyDiscountMatrixTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Set driver class not available on object 
            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorPolicyDiscountMatrix with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND UWTier = @UWTier ";
            SQL += " AND MultiCar <= @MultiCar ";
            SQL += " AND PaidInFull = @PaidInFull ";
            SQL += " AND HomeOwner = @HomeOwner ";
            SQL += " ORDER BY Coverage Asc ";


            List<SqlParameter> parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@UWTier", SqlDbType.VarChar, 3, pol.PolicyInsured.UWTier));
            parms.Add(DBHelper.AddParm("@MultiCar", SqlDbType.VarChar, 1, VehicleHelper.VehicleCount(pol) > 1 ? "Y" : "N"));
            parms.Add(DBHelper.AddParm("@PaidInFull", SqlDbType.VarChar, 1, PolicyHelper.PayPlanIsPaidInFull(pol, stateInfo, connectionString) ? "Y" : "N"));
            parms.Add(DBHelper.AddParm("@HomeOwner", SqlDbType.VarChar, 1, (pol.PolicyInsured.OccupancyType.ToUpper() == "HOMEOWNER") ? "Y" : "N"));
            return DBHelper.GetDataTable(SQL, "FactorPolicyDiscountMatrix", connectionString, parms);
        }

        public static void GetMidAddCoverageFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string factorName = "";
            factorName = "MidAdd";
            GetCoverageFactor(factorName, veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
        }

        public static void GetMidMultCoverageFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string factorName = "";
            factorName = "MidMult";
            GetCoverageFactor(factorName, veh, factorTable, cappedFactors, cappedFactorList, pol, stateInfo, connectionString);
        }

        public static void GetCoverageFactor(string factorType, clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string factorName = GetCoverageFactorName(factorType);
            VehicleHelper.CheckCoverageDeductiblesAndLimits(veh);
            DataTable dataTable = GetCoverageFactorTable(pol, stateInfo, connectionString, factorType, factorName);
            GetFactorUsingCoverages(veh, factorName, dataTable, factorTable, pol);
        }
        private static string GetCoverageFactorName(string factorType)
        {
            string factorName = "";
            factorName = factorType.Trim().ToUpper() == "MIDADD" ? "CoverageAdd" : "Coverage";
            return factorName;
        }
        public static DataTable GetCoverageFactorTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            return GetBasicFactorTable("FactorCoverage", pol, connectionString, " SELECT Coverage, Code, Description, Factor, FactorType, Program FROM ");
        }
        private static DataTable GetCoverageFactorTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, string factorType, string factorName)
        {
            
            //Set driver class not available on object 
            string SQL = " SELECT Coverage, Code, Description, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorCoverage with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND UWTier = @UWTier ";
            SQL += " AND FactorType = @Type ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@UWTier", SqlDbType.VarChar, 3, PolicyHelper.GetUWTierForCoverageFactors(pol)));
            parms.Add(DBHelper.AddParm("@Type", SqlDbType.VarChar, 10, factorType.Trim()));
            return DBHelper.GetDataTable(SQL, factorName, connectionString, parms);
        }

        public static void GetStatedValueFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (VehicleHelper.VehicleSymbolIsStatedAmountSymbol(veh.VehicleSymbolCode, int.Parse(veh.VehicleYear)))
            {                
                DataTable dataTable = GetStatedValueFactorTable(veh, pol, connectionString);
                GetFactor("StatedValue", dataTable, factorTable, pol);
            }
        }
        public static DataTable GetStatedValueFactorTable(clsPolicyPPA pol, string connectionString)
        {
            return GetBasicFactorTable("FactorStatedValue", pol, connectionString, " SELECT Program, Coverage, MinStatedValue, MaxStatedValue, Description, Factor, FactorType FROM ");
        }
        public static DataTable GetStatedValueFactorTable(clsVehicleUnit veh, clsPolicyPPA pol, string connectionString)
        {
            //Set driver class not available on object 
            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorStatedValue with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND MinStatedValue <= @StatedValue ";
            SQL += " AND MaxStatedValue >= @StatedValue ";
            SQL += " AND MinVehYear <= @VehicleYear ";
            SQL += " AND MaxVehYear >= @VehicleYear ";
            SQL += " AND Description >= @Description ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@StatedValue", SqlDbType.Int, 22, veh.StatedAmt.ToString()));
            parms.Add(DBHelper.AddParm("@VehicleYear", SqlDbType.Int, 22, veh.VehicleYear));
            parms.Add(DBHelper.AddParm("@Description", SqlDbType.VarChar, 75, veh.VehicleTypeCode));
            return DBHelper.GetDataTable(SQL, "FactorStatedValue",connectionString, parms);
        }

        public static void GetSymbolFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsPACoverage cov in veh.Coverages)
            {
                if (!cov.IsMarkedForDelete)
                {                                     
                    DataTable dataTable = GetSymbolFactorTable(veh, pol, cov, stateInfo, connectionString);
                    GetFactor("Symbol", dataTable, factorTable, pol);
                }
            }
        }
        public static DataTable GetSymbolFactorTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverSymbol = GetVehicleSymbols(pol);

            string SQL = " SELECT Program, Coverage, Symbol, MinVehYear, MaxVehYear, Factor, FactorType ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorSymbol with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Symbol IN (" + driverSymbol + ") ";
            SQL += " ORDER BY Program, Symbol, Coverage";

            List<SqlParameter> parms = GetBasicParms(pol);

            return DBHelper.GetDataTable(SQL, "FactorSymbol", connectionString, parms);
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
        public static DataTable GetSymbolFactorTable(clsVehicleUnit veh, clsPolicyPPA pol, clsPACoverage cov, StateInfoHelper stateInfo, string connectionString)
        {
            string symbol = VehicleHelper.GetSymbolForFactorLookup(cov, veh, pol, stateInfo, connectionString);

            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorSymbol with(nolock)";
            SQL += GetBasicWhereClause();
            SQL += " AND Coverage = @Coverage ";
            SQL += " AND Symbol = @Symbol ";
            SQL += " AND MinVehYear <= @VehicleYear ";
            SQL += " AND MaxVehYear >= @VehicleYear ";
            SQL += " ORDER BY Coverage Asc ";


            List<SqlParameter> parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@Coverage", SqlDbType.VarChar, 11, cov.CovGroup));
            parms.Add(DBHelper.AddParm("@VehicleYear", SqlDbType.VarChar, 5, VehicleHelper.GetVehicleYearForSymbolFactorLookup(veh)));
            parms.Add(DBHelper.AddParm("@Symbol", SqlDbType.VarChar, 4, symbol));
            return DBHelper.GetDataTable(SQL, "FactorSymbol", connectionString, parms);
        }

        public static void GetDriverMeritFactor(clsPolicyPPA pol, clsEntityDriver drv, DataTable factorTable, StateInfoHelper stateInfo, string connectionString)
        {            
            List<string> meritFactorList = GetMeritFactorCodeList(pol, connectionString);
            foreach (string factor in meritFactorList)
            {
                List<int> numInRange = GetNumsInRange(pol, drv, stateInfo, connectionString, factor);
                DataTable dataTable = GetDriverMeritFactorTable(drv, pol, factor, numInRange, stateInfo, connectionString);
                GetFactor(factor, dataTable, factorTable, pol);
            }
        }
        private static DataTable GetDriverMeritFactorTable(clsEntityDriver drv, clsPolicyPPA pol, string factor, List<int> numsInRanges, StateInfoHelper stateInfo, string connectionString)
        {
            string SQL = " SELECT Coverage, Factor, FactorType ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorMerit with(nolock) ";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND MinDriverAge <= @InsuredAge ";
            SQL += " AND MaxDriverAge > @InsuredAge ";
            SQL += " AND NumberInRange1 = @NumberInRange1 ";
            SQL += " AND NumberInRange2 = @NumberInRange2 ";
            SQL += " AND NumberInRange3 = @NumberInRange3 ";
            SQL += " AND Code = @Code ";
            SQL += " ORDER BY Coverage Asc ";                       

            List<SqlParameter> parms = GetBasicParms(pol);

            parms.Add(DBHelper.AddParm("@InsuredAge", SqlDbType.Int, 22, drv.Age));
            parms.Add(DBHelper.AddParm("@NumberInRange1", SqlDbType.Int, 22, numsInRanges[0]));
            parms.Add(DBHelper.AddParm("@NumberInRange2", SqlDbType.Int, 22, numsInRanges[1]));
            parms.Add(DBHelper.AddParm("@NumberInRange3", SqlDbType.Int, 22, numsInRanges[2]));
            parms.Add(DBHelper.AddParm("@Code", SqlDbType.VarChar, 10, factor));

            return DBHelper.GetDataTable(SQL, "FactorMerit", connectionString, parms);

        }
        private static List<string> GetMeritFactorCodeList(clsPolicyPPA pol, string connectionString)
        {
            List<string> meritFactorList = new List<string>();
            DataTable dataTable = GetBasicFactorTable("FactorMerit", pol, connectionString, " SELECT Distinct(Code) ");
            if (dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    meritFactorList.Add(row["Code"].ToString());
                }
            }
            return meritFactorList;
        }        
        private static List<int> GetNumsInRange(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString, string factor)
        {
            List<int> nums = new List<int>();
            nums.Add(0);
            nums.Add(0);
            nums.Add(0);

            DataRow[] rows;            
            rows = stateInfo.GetRows(pol, "MERIT", "GROUP", factor, connectionString);

            DataRow[] stateInfoRows;
            stateInfoRows = stateInfo.GetRows(pol, "MERIT", "IGNORE", "ADMINMSG", connectionString);

            DateTime ignoreAdminStartDate = DateTime.MinValue;
            foreach (DataRow row in stateInfoRows)
            {
                ignoreAdminStartDate = (DateTime)row["ItemValue"];
            }

            foreach (DataRow row in rows)
            {
                foreach (clsBaseViolation viol in drv.Violations)
                {
                    bool ignoreViol = false;
                    if (pol.Program.ToUpper() == "SUMMIT" && viol.ViolGroup.ToUpper() == "UDR" && (drv.Age <= 18 || DriverHelper.HasForeignLicense(drv)))
                    {
                        ignoreViol = true;
                    }
                    // If this is an administration message, check the state info row to see when to start
                    // ignoring this violation, if no row then always ignore

                    if (!ignoreViol && viol.ViolDesc.ToUpper().Trim() == "ADMINISTRATION MESSAGE")
                    {
                        if (ignoreAdminStartDate == DateTime.MinValue || pol.RateDate >= ignoreAdminStartDate)
                        {
                            ignoreViol = true;
                        }
                    }
                    if (!ignoreViol && stateInfo.Contains(pol, "COMBINEDRIVER", "VIOLIGNORE", viol.ViolTypeCode, connectionString))
                    {
                        ignoreViol = true;
                    }
                    if (!ignoreViol && stateInfo.Contains(pol, "COMBINEDRIVER", "VIOLGROUPIGNORE", viol.ViolGroup, connectionString))
                    {
                        ignoreViol = true;
                    }

                    if (!ignoreViol)
                    {
                        if (viol.ViolGroup == row["ItemSubCode"].ToString().ToUpper())
                        {
                            int violAge = ViolationHelper.CalculateViolAgeInMonths(viol.ViolDate, pol.EffDate);
                            if (violAge <= 11)
                            {
                                nums[0] = (nums[0] < 3 ? nums[0] + 1 : nums[0]);
                            }
                            else if (violAge <= 23)
                            {
                                nums[1] = (nums[1] < 3 ? nums[1] + 1 : nums[1]);
                            }
                            else if (violAge <= 35)
                            {
                                nums[2] = (nums[2] < 3 ? nums[2] + 1 : nums[2]);
                            }
                        }
                    }
                }
            }
            return nums;
        }


        public static void GetTerritoryFactor_Summit(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsPACoverage cov in veh.Coverages)
            {
                if (!cov.IsMarkedForDelete)
                {
                    DataTable dataTable = GetTerritoryFactorTable(pol, stateInfo, connectionString, cov);
                    GetFactor("Territory", dataTable, factorTable, pol);
                }
            }
        }
        public static DataTable GetTerritoryCodeTable(clsPolicyPPA pol, string connectionString)
        {
            string zipCodes = getTerritoryZipCodes(pol);

            string SQL = " SELECT Program, Coverage, Zip, County, City, State, Territory, Region, Disabled";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..CodeTerritoryDefinitions with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Zip IN (" + zipCodes + ")";
            SQL += " ORDER BY Program, Coverage";

            List<SqlParameter> parms = GetBasicParms(pol);
            return DBHelper.GetDataTable(SQL, "CodeTerritoryDefinitions", connectionString, parms);

        }
        private static string getTerritoryZipCodes(clsPolicyPPA pol)
        {
            string zipCodes = "";
            int vehCount = 0;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (vehCount > 0) zipCodes += ", ";
                zipCodes += veh.Zip.Trim();
                vehCount++;
            }
            return zipCodes;
        }
        private static string getTerritories(clsPolicyPPA pol)
        {
            string territories = "";
            int vehCount = 0;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (vehCount > 0) territories += ", ";
                if (veh.Territory.Length > 0)
                {
                    territories += "'" + veh.Territory + "'";
                    vehCount++;
                }
            }
            return territories;
        }

        public static DataTable GetTerritoryFactorTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string territories = getTerritories(pol);
            string SQL = " SELECT Coverage, Factor, FactorType, Territory, Program ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorTerritory with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Program = @Program";
            if (territories != string.Empty)
            {
                SQL += " AND Territory IN (" + territories + ")";
            }
            SQL += " AND VehicleCountType IN ('A', @VehicleCountType ) ";
            SQL += " ORDER BY Coverage Asc";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@VehicleCountType", SqlDbType.VarChar, 1, VehicleHelper.GetVehicleCountType(pol, stateInfo, connectionString)));

            return DBHelper.GetDataTable(SQL, "TerritoryFactor", connectionString, parms);
        }
        public static DataTable GetTerritoryFactorTable(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, clsPACoverage cov)
        {
            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorTerritory with(nolock)";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Coverage = @Coverage ";
            SQL += " AND Territory = @Territory ";
            SQL += " AND VehicleCountType IN ('A', @VehicleCountType) ";
            //SQL += " AND MaxVehYear >= @VehicleYear ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@Coverage", SqlDbType.VarChar, 11, cov.CovGroup));
            parms.Add(DBHelper.AddParm("@Territory", SqlDbType.VarChar, 5, cov.Territory.Trim()));
            parms.Add(DBHelper.AddParm("@VehicleCountType", SqlDbType.VarChar, 1, VehicleHelper.GetVehicleCountType(pol, stateInfo, connectionString)));

            return DBHelper.GetDataTable(SQL, "TerritoryFactor", connectionString, parms);
        }

        public static void GetMarketAdjustmentFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable dataTable = GetMarketAdjustmentTable(veh, pol, connectionString);
            GetFactor("MarketAdjustFactor", dataTable, factorTable, pol);
        }

        private static DataTable GetMarketAdjustmentTable(clsVehicleUnit veh, clsPolicyPPA pol, string connectionString)
        {
            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorMarketAdjustment with(nolock)";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND Zip = @Zip ";
            SQL += " AND County = @County ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@Zip", SqlDbType.VarChar, 5, veh.Zip));
            parms.Add(DBHelper.AddParm("@County", SqlDbType.VarChar, 30, veh.County));

            return DBHelper.GetDataTable(SQL, "MarketAdjustment", connectionString, parms);
        }

        public static void GetTierMatrixFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable dataTable = GetTierMatrixTable(pol, connectionString);
            GetFactor("TierMatrix", dataTable, factorTable, pol);
        }

        private static DataTable GetTierMatrixTable(clsPolicyPPA pol, string connectionString)
        {
            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorTierMatrix with(nolock)";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND CreditTier = @CreditTier ";
            SQL += " AND UWTier = @UWTier ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@CreditTier", SqlDbType.VarChar, 3, pol.PolicyInsured.CreditTier));
            parms.Add(DBHelper.AddParm("@UWTier", SqlDbType.VarChar, 30, pol.PolicyInsured.UWTier));

            return DBHelper.GetDataTable(SQL, "TierMatrix", connectionString, parms);
        }

        public static DataTable GetVehicleFactorTable(clsPolicyPPA pol, string connectionString)
        {
            return GetBasicFactorTable("FactorVehicle", pol, connectionString, " SELECT Program, Coverage, Description, FactorCode, Factor, FactorType FROM ");
        }
        public static void GetVehicleFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsBaseFactor factor in veh.Factors)
            {
                //Set driver class not available on object 
                string SQL = " SELECT Coverage, Description, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorVehicle with(nolock)";
                SQL += " WHERE Program = @Program ";
                SQL += " AND EffDate <= @RateDate ";
                SQL += " AND ExpDate > @RateDate ";
                SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                SQL += " AND FactorCode = @FactorCode ";
                SQL += " ORDER BY Coverage Asc ";


                List<SqlParameter> parms = new List<SqlParameter>();

                parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
                parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
                parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
                parms.Add(DBHelper.AddParm("@FactorCode", SqlDbType.VarChar, 20, factor.FactorCode));
                DataTable dataTable = DBHelper.GetDataTable(SQL, "VehicleFactor", connectionString, parms);
                GetFactorUsingCapped(factorTable, cappedFactors, cappedFactorList, factor, dataTable);
            }
        }

        public static void GetDiscountFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow factorRow = null;
            DataRow discountNoMaxRow = null;
            DataRow discountMaxRow = null;
            DataRow surchargeRow = null;

            List<string> maxDiscountFactorList = new List<string>();
            DataRow[] rows = stateInfo.GetRows(pol, "MAXDISCOUNT", "FACTOR", "", connectionString);
            foreach (DataRow row in rows)
            {
                maxDiscountFactorList.Add(row["ItemValue"].ToString());
            }

            string SQL = " SELECT Coverage, Factor, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorDiscount with(nolock)";
            SQL += " WHERE Program = @Program ";
            SQL += " AND EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " ORDER BY Coverage Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            DataTable factorDiscountTable = DBHelper.GetDataTable(SQL, "FactorDiscountTable", connectionString, parms);
            if (factorDiscountTable != null)
            {
                DataRow[] discountRows = factorDiscountTable.Select();

                factorRow = factorTable.NewRow();
                factorRow["FactorName"] = "Discount";

                discountNoMaxRow = factorTable.NewRow();
                discountNoMaxRow["FactorName"] = "DiscountNoMax";

                discountMaxRow = factorTable.NewRow();
                discountMaxRow["FactorName"] = "DiscountMax";

                surchargeRow = factorTable.NewRow();
                surchargeRow["FactorName"] = "Surcharge";

                bool addSR22 = DriverHelper.HasSR22Drivers(pol);
                bool combinedDriverState = FactorsHelper.RatedFactorExists(factorTable, "COMBINEDDRIVER");
                if (addSR22 && combinedDriverState)
                {
                    DataRow sr22Row = null;
                    sr22Row = factorTable.NewRow();
                    sr22Row["FactorName"] = "SR22";
                    factorTable.Rows.Add(sr22Row);
                }

                foreach (DataRow row in discountRows)
                {
                    if (factorRow[row["Coverage"].ToString()] == DBNull.Value)
                    {
                        factorRow[row["Coverage"].ToString()] = 1.0D;
                    }
                    if (discountMaxRow[row["Coverage"].ToString()] == DBNull.Value)
                    {
                        discountMaxRow[row["Coverage"].ToString()] = 1.0D;
                    }
                    if (discountNoMaxRow[row["Coverage"].ToString()] == DBNull.Value)
                    {
                        discountNoMaxRow[row["Coverage"].ToString()] = 1.0D;
                    }
                    if (surchargeRow[row["Coverage"].ToString()] == DBNull.Value)
                    {
                        surchargeRow[row["Coverage"].ToString()] = 1.0D;
                    }

                    foreach (DataRow factorInRow in factorTable.Rows)
                    {
                        if (factorInRow["FactorName"].ToString().Trim().ToUpper() == row["FactorCode"].ToString().Trim().ToUpper())
                        {
                            for (int i = 1; i < factorTable.Columns.Count; i++)
                            {
                                if (row["Coverage"].ToString() == factorTable.Columns[i].ColumnName)
                                {
                                    //'*************************************
                                    //'If we have .85 in the Discount factor already (then we're applying a 15% discount)
                                    //'If we want to add another 5% discount, the factor row for the new discount has .95
                                    //'So, the new factor on the Discount row needs to be .80
                                    //' What we do is (1 - ((1 - .85) + (1 - .95)))
                                    //' Step 1 is... (1 - (.15 + .05))
                                    //' Step 2 is... (1 - .20)
                                    //' Giving us... .80 to put in the Discount record for rating
                                    //'*************************************
                                    if (decimal.Parse(row["Factor"].ToString()) > 1)
                                    {
                                        surchargeRow[row["Coverage"].ToString()] = (decimal)(1 + (((decimal)((decimal)surchargeRow[row["Coverage"].ToString()] - 1)) + ((decimal)((decimal)row["Factor"] - 1))));
                                    }
                                    else
                                    {
                                        if (maxDiscountFactorList.Contains(row["FactorCode"].ToString()))
                                        {
                                            discountMaxRow[row["Coverage"].ToString()] = (decimal)(1 - (((decimal)(1 - (decimal)discountMaxRow[row["Coverage"].ToString()])) + ((decimal)(1 - (decimal)row["Factor"]))));
                                        }
                                        else
                                        {
                                            discountNoMaxRow[row["Coverage"].ToString()] = (decimal)(1 - (((decimal)(1 - (decimal)discountNoMaxRow[row["Coverage"].ToString()])) + ((decimal)(1 - (decimal)row["Factor"]))));
                                        }
                                        break;
                                    }
                                }
                            }

                        }
                    }

                    if (discountMaxRow != null)
                    {
                        DataRow[] maxRows = stateInfo.GetRows(pol, "MAXDISCOUNT", "PERCENT", "", connectionString);
                        foreach (DataRow maxRow in rows)
                        {
                            if ((decimal)discountMaxRow[maxRow["ItemSubCode"].ToString()] < (decimal)maxRow["ItemValue"])
                            {
                                discountMaxRow[maxRow["ItemSubCode"].ToString()] = (decimal)maxRow["ItemValue"];
                            }

                        }

                        SQL = " SELECT DISTINCT Coverage From pgm" + pol.Product + pol.StateCode + "..FactorDiscount with(nolock) ";
                        SQL += " WHERE Program = @Program ";
                        SQL += "  AND EffDate <= @RateDate ";
                        SQL += "  AND ExpDate > @RateDate ";
                        SQL += "  AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                        SQL += "  ORDER BY Coverage Asc ";

                        DataTable discountCovTable = DBHelper.GetDataTable(SQL, "CovDiscountTable", connectionString, parms);
                        foreach (DataRow discountCovRow in discountCovTable.Rows)
                        {
                            factorRow[discountCovRow["Coverage"].ToString()] = (decimal)(1 - (((decimal)(1 - (decimal)discountNoMaxRow[row["Coverage"].ToString()])) + ((decimal)(1 - (decimal)discountMaxRow[row["Coverage"].ToString()]))));
                        }

                    }
                }
            }

            if (factorRow != null)
            {
                factorTable.Rows.Add(factorRow);
            }
            if (surchargeRow != null)
            {
                factorTable.Rows.Add(surchargeRow);
            }
        }

        public static void GetRatedFactor(clsVehicleUnit veh, DataTable factorTable, DataTable cappedFactors, List<string> cappedFactorList, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataRow factorRow = null;

            foreach (clsBaseFactor factor in pol.PolicyFactors)
            {
                factorRow = factorTable.NewRow();
                factorRow["FactorName"] = factor.FactorCode;
                for (int i = 1; i < factorTable.Columns.Count - 1; i++)
                {
                    factorRow[factorTable.Columns[i].ColumnName] = factor.RatedFactor;
                }
                if (factor.FactorType != string.Empty)
                {
                    factorRow["FactorType"] = factor.FactorType.Trim();
                }
                else
                {
                    factorRow["FactorType"] = "MidMult";
                }

                if (factorRow != null)
                {
                    decimal ratedFactor = 0;
                    Decimal.TryParse(factor.RatedFactor, out ratedFactor);
                    if (ratedFactor > 0)
                    {
                        factorTable.Rows.Add(factorRow);
                    }
                }
            }
        }

        public static List<string> GetCappedFactors(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            List<string> result = new List<string>();
            DataRow[] rows = stateInfo.GetRows(pol, "MAXDISCOUNT", "FACTOR", "", connectionString);
            foreach (DataRow row in rows)
            {
                result.Add(row["ItemValue"].ToString());
            }
            return result;
        }

        public static DataTable GetRateOrderTable(clsPolicyPPA pol, string connectionString)
        {
            string SQL = " SELECT Program, FactorType, FactorName, FactorOrder, RateOrder FROM pgm" + pol.Product + pol.StateCode + "..RateOrder with(nolock)";
            SQL += "  WHERE Program = @Program ";
            SQL += "  AND EffDate <= @RateDate ";
            SQL += "  AND ExpDate > @RateDate ";
            SQL += "  AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += "  ORDER BY FactorType, RateOrder Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            DataTable rateOrderTable = DBHelper.GetDataTable(SQL, "RateOrder", connectionString, parms);
            return rateOrderTable;
        }

        public static void GetDriverFactorRows(clsPolicyPPA pol, clsEntityDriver drv, DataTable factorTable, StateInfoHelper stateInfo, string connectionString)
        {
            try
            {
                foreach (clsBaseFactor factor in drv.Factors)
                {
                    GetFactorRow(factor, pol, drv, factorTable, stateInfo, connectionString, FactorsHelper.GetDriverFactorTable);
                }
                GetFactorRow("DriverAgePoints", pol, drv, factorTable, stateInfo, connectionString, GetDriverAgePointsTable);
                GetFactorRow("DriverClass", pol, drv, factorTable, stateInfo, connectionString, GetDriverClassTable);
                GetFactorRow("DriverPoints", pol, drv, factorTable, stateInfo, connectionString, GetDriverPointsTable);
                GetFactorRow("MarketPoints", pol, drv, factorTable, stateInfo, connectionString, GetMarketPointsTable);
                GetDriverMeritFactor(pol, drv, factorTable, stateInfo, connectionString);
            }
            catch
            {
                //Do Nothing yet;
            }
        }

        public static void GetFactorRow(string factorCode, clsPolicyPPA pol, clsEntityDriver drv,
            DataTable factorTable, StateInfoHelper stateInfo, string connectionString,
            Func<clsPolicyPPA, clsEntityDriver, StateInfoHelper, string, clsBaseFactor, DataTable> tableMethod)
        {
            clsBaseFactor factor = new clsBaseFactor();
            factor.FactorCode = factorCode;
            GetFactorRow(factor, pol, drv, factorTable, stateInfo, connectionString, tableMethod);
        }

        public static void GetFactorRow(clsBaseFactor factor, clsPolicyPPA pol, clsEntityDriver drv,
            DataTable factorTable, StateInfoHelper stateInfo, string connectionString,
            Func<clsPolicyPPA, clsEntityDriver, StateInfoHelper, string, clsBaseFactor, DataTable> tableMethod)
        {

            try
            {
                DataTable factors = tableMethod(pol, drv, stateInfo, connectionString, factor);
                DataRow rowFactor = AddFactorRow(factor, factorTable, factors);
                if (rowFactor != null)
                {
                    factorTable.Rows.Add(rowFactor);
                }
            }
            catch
            {
                //Do Nothing yet
            }

        }

        //public static DataRow AddFactorRow(string factorCode, DataTable factorTable, DataTable reader)
        //{
        //    clsBaseFactor factor = new clsBaseFactor();
        //    factor.FactorCode = factorCode;
        //    return AddFactorRow(factor, factorTable, reader, false);
        //}

        public static DataRow AddFactorRow(clsBaseFactor factor, DataTable factorTable, DataTable reader, bool AddDescription = false)
        {
            DataRow rowFactor = null;
            bool haveFactorType = false;

            if (reader != null)
            {
                rowFactor = factorTable.NewRow();
                rowFactor["FactorName"] = factor.FactorCode;

                foreach (DataRow row in reader.Rows)
                {
                    if (AddDescription)
                    {
                        factor.FactorDesc = row["Description"].ToString();
                    }
                    for (int i = 0; i < factorTable.Columns.Count - 1; i++)
                    {
                        if (row["Coverage"].ToString() == factorTable.Columns[i].ColumnName)
                        {
                            rowFactor[row["Coverage"].ToString()] = row["Factor"].ToString();
                            break;
                        }
                    }
                    if (!haveFactorType)
                    {
                        rowFactor["FactorType"] = row["FactorType"].ToString();
                        haveFactorType = true;
                    }
                }
            }
            return rowFactor;
        }

        public static void AddDriverFactor(clsEntityDriver driver, string factorCode, List<string> coverageList)
        {
            clsBaseFactor factor = null;
            for (int i = 0; i < coverageList.Count - 1; i++)
            {
                if (factor != null)
                {
                    factor = null;
                }
                factor = new clsBaseFactor();
                factor.FactorCode = factorCode;
                factor.IndexNum = driver.Factors.Count + 1;
                factor.SystemCode = factorCode;
                factor.FactorNum = driver.Factors.Count + 1;
                factor.FactorAmt = 0;
                factor.CovType = coverageList[i];
                driver.Factors.Add(factor);
            }
        }

        public static List<DataTable> CreateDataTables(clsPolicyPPA pol, string connectionString)
        {
            List<DataTable> driverFactorTables = new List<DataTable>();
            DataTable factorTable;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete && DriverHelper.ShouldRateDriver(drv, pol))
                {
                    factorTable = CreateDataTable("Factors", pol, connectionString);
                    factorTable.TableName = drv.IndexNum.ToString();

                    if (driverFactorTables == null)
                    {
                        driverFactorTables = new List<DataTable>();
                    }
                    driverFactorTables.Add(factorTable);
                }
            }
            return driverFactorTables;
        }

        public static DataTable CreateDataTable(string tableName, clsPolicyPPA pol, string connectionString)
        {
            DataTable factorTable = null;
            SqlDataReader reader = null;
            DataColumn colFactorName = null;
            DataColumn colFactorType = null;
            string typeName = "";

            try
            {
                factorTable = new DataTable(tableName);
                colFactorName = new DataColumn("FactorName");
                factorTable.Columns.Add(colFactorName);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string SQL = " SELECT DISTINCT(Coverage) ";
                    SQL += " FROM pgm" + pol.Product + pol.StateCode + "..FactorBaseRate with(nolock) ";
                    SQL += " WHERE Program = @Program ";
                    SQL += " AND EffDate <= @RateDate ";
                    SQL += " AND ExpDate > @RateDate ";
                    SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                    SQL += " ORDER BY Coverage Asc";


                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL, conn);

                    cmd.Parameters.Add("@Program", SqlDbType.VarChar, 10).Value = pol.Program;
                    cmd.Parameters.Add("@RateDate", SqlDbType.DateTime, 8).Value = pol.RateDate;
                    cmd.Parameters.Add("@AppliesToCode", SqlDbType.VarChar, 1).Value = pol.AppliesToCode;

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        switch (pol.Product)
                        {
                            case "1":
                                for (int i = 0; i < 3; i++)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            typeName = "D";
                                            break;
                                        case 1:
                                            typeName = "C";
                                            break;
                                        case 2:
                                            typeName = "N";
                                            break;
                                        default:
                                            typeName = "";
                                            break;
                                    }
                                    DataColumn colCov = new DataColumn(String.Concat(reader["Coverage"].ToString(), "_", typeName));
                                    factorTable.Columns.Add(colCov);
                                    if (colCov != null)
                                    {
                                        colCov.Dispose();
                                        colCov = null;
                                    }
                                }
                                break;
                            case "2":
                                DataColumn colCov2 = new DataColumn(reader["Coverage"].ToString());
                                factorTable.Columns.Add(colCov2);
                                if (colCov2 != null)
                                {
                                    colCov2.Dispose();
                                    colCov2 = null;
                                }
                                break;

                        }
                    }

                }
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
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
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
        }

    }
}
