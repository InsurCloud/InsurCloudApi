using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Helpers
{
    public static class DBHelper
    {
        public static DataColumn AddColumn(string columnName)
        {
            return new DataColumn(columnName);
        }
        public static string GetScalarValue(string sql, string columnName, string connectionString, List<SqlParameter> parms)
        {
            DataSet ds = new DataSet();
            string value = string.Empty;

            try
            {
                //TODO: Replace connection string with parameter
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    foreach (SqlParameter parm in parms)
                    {
                        cmd.Parameters.Add(parm);
                    }

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds, "StateInfo");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        value = ds.Tables[0].Rows[0][columnName].ToString();
                    }
                }

            }
            catch
            {
                //Nothing Yet
            }
            finally
            {
                ds.Dispose();
                ds = null;
            }
            return value;
        }

        public static DataTable GetDataTable(string sql, string tableName, string connectionString, List<SqlParameter> parms, string DatabaseNameOverride = "")
        {
            DataSet ds = new DataSet();
            DataTable dt = null;            

            if (DatabaseNameOverride != string.Empty)
            {
                connectionString = connectionString.Replace("pgm242", DatabaseNameOverride);
            }

            try
            {
                //TODO: Replace connection string with parameter
                using (SqlConnection conn = new SqlConnection(connectionString))
                {                    
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        foreach (SqlParameter parm in parms)
                        {
                            cmd.Parameters.Add(CloneParm(parm));
                        }

                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = cmd;
                        da.Fill(ds, tableName);
                        if (ds.Tables.Count > 0)
                        {
                            dt = ds.Tables[0];
                        }
                    }                    
                }

            }
            catch
            {
                //Nothing Yet
            }
            finally
            {                
                ds.Dispose();
                ds = null;
            }
            return dt;
        }

        private static SqlParameter CloneParm(SqlParameter parm)
        {
            SqlParameter newParm = new SqlParameter(parm.ParameterName, parm.SqlDbType, parm.Size);
            newParm.Value = parm.Value;
            return newParm;
        }
        public static int DateDiffMonths(DateTime firstDate, DateTime secondDate)
        {
            return ((firstDate.Year - secondDate.Year) * 12) + firstDate.Month - secondDate.Month;
        }
        public static SqlParameter AddParm(string parameterName, SqlDbType dbType, int size, string value)
        {
            SqlParameter parm = new SqlParameter(parameterName, dbType, size);
            parm.Value = value;
            return parm;
        }
        public static SqlParameter AddParm(string parameterName, SqlDbType dbType, int size, DateTime value)
        {
            SqlParameter parm = new SqlParameter(parameterName, dbType, size);
            parm.Value = value;
            return parm;
        }
        public static SqlParameter AddParm(string parameterName, SqlDbType dbType, int size, int value)
        {
            SqlParameter parm = new SqlParameter(parameterName, dbType, size);
            parm.Value = value;
            return parm;
        }

        public static DataTable SelectDistinct(DataTable sourceTable, string fieldName){
            var lastValue = new Object();
            DataTable newTable;
            if(fieldName == null || fieldName.Length == 0){
                throw new ArgumentException("FieldNames");
            }
            newTable = new DataTable();
            newTable.Columns.Add(fieldName, sourceTable.Columns[fieldName].DataType);

            foreach(DataRow row in sourceTable.Select("", fieldName)){
                if(!lastValue.Equals(row[fieldName])){
                    newTable.Rows.Add(row[fieldName]);
                    lastValue = row[fieldName];
                }
            }
            return newTable;
        }


        public static void ExecuteNonQuery(string sql, string connectionString, List<SqlParameter> parms = null)
        {
            if (parms == null)
            {
                parms = new List<SqlParameter>();
            }
            try
            {
                //TODO: Replace connection string with parameter
                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        foreach (SqlParameter parm in parms)
                        {
                            cmd.Parameters.Add(parm);
                        }

                        cmd.ExecuteNonQuery();
                    }                    
                }

            }
            catch
            {
                //Nothing Yet
            }              
        }

        public static DataRow CreateTotalsRow(DataTable factorTable)
        {
            DataRow factorRow = null;
            factorRow = factorTable.NewRow();
            factorRow["FactorName"] = "Totals";
            for (int i = 1; i < factorTable.Columns.Count; i++)
            {
                if (factorTable.Columns[i].ColumnName.ToUpper() == "FACTORTYPE")
                {
                    factorRow[i] = "Premium";
                    break;
                }
                factorRow[i] = 0;
            }
            return factorRow;                        
        }

        public static DataRow GetRow(DataTable table, string factorName)
        {
            DataRow[] rows = table.Select("FactorName='" + factorName + "'");
            if (rows.Length > 0)
            {
                return rows[0];
            }
            return null;
        }

        public static decimal RoundStandard(decimal num, int precision){
            decimal factor = Convert.ToDecimal(Math.Pow(10, precision));
            int sign = Math.Sign(num);
            return decimal.Truncate(num * factor + (decimal)0.5 * sign) / factor;
        }
    }
}
