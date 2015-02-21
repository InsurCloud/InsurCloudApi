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

namespace Helpers
{
    public class StateInfoHelper
    {
        private DataTable _stateInfoTable = null;        

        public StateInfoHelper(string product, string stateCode, DateTime rateDate, string appliesToCode, string companyCode, string connectionString)
        {
            Load(product, stateCode, rateDate, appliesToCode, companyCode, connectionString);
        }

        public DateTime GetDateTimeValue(clsPolicyPPA pol, string itemGroup, string itemCode, string itemSubCode, string connectionString)
        {
            string val = GetStringValue(pol, itemGroup, itemCode, itemSubCode, connectionString);
            DateTime retVal = DateTime.MinValue;
            DateTime.TryParse(val, out retVal);
            return retVal;
        }

        public string GetStringValue(clsPolicyPPA pol, string itemGroup, string itemCode, string itemSubCode, string connectionString)
        {
            DataRow[] rows = GetRows(pol, itemGroup, itemCode, itemSubCode, connectionString);
            string rowValue = "";
            foreach (DataRow row in rows)
            {
                rowValue = row["ItemValue"].ToString();
            }
            return rowValue;
                
        }

        public DataRow[] GetRows(clsPolicyPPA pol, string itemGroup, string itemCode, string itemSubCode, string connectionString)
        {
            Load(pol.Product, pol.StateCode, pol.RateDate, pol.AppliesToCode, pol.ProgramInfo.CompanyCode, connectionString);
            string whereClause = "";
            whereClause += "Program IN ('PPA', '" + pol.Program + "') ";
            if (itemGroup != "")
            {
                whereClause += "AND ItemGroup='" + itemGroup + "' ";
            }
            if (itemCode != "")
            {
                whereClause += "AND ItemCode='" + itemCode + "' ";
            }
            if (itemSubCode != "")
            {
                whereClause += "AND ItemSubCode='" + itemSubCode + "' ";
            }

            DataRow[] rows = _stateInfoTable.Select(whereClause);
            return rows;
        }

        public bool Contains(clsPolicyPPA pol, string itemGroup, string itemCode, string itemSubCode, string connectionString)
        {
            string value = GetStringValue(pol, itemGroup, itemCode, itemSubCode, connectionString);
            return value == "" ? false : true;
        }

        public void Load(string product, string stateCode, DateTime rateDate, string appliesToCode, string companyCode, string connectionString)
        {

            if (_stateInfoTable == null)
            {
                string SQL = "";
                SQL = " SELECT Program, ItemGroup, ItemCode, ItemSubCode, ItemValue ";
                if (product == string.Empty)
                {
                    SQL += " FROM Common..StateInfo with(nolock)";
                }
                else
                {
                    SQL += " FROM pgm" + product + stateCode + "..StateInfo with(nolock)";
                }
                
                SQL +=  " WHERE EffDate <= @RateDate ";
                SQL +=  " AND ExpDate > @RateDate ";
                if (product != string.Empty)
                {
                    SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
                }                
                SQL +=  " ORDER BY Program, ItemGroup, ItemCode ";

                List<SqlParameter> parms = new List<SqlParameter>();

                parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, rateDate));
                parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, appliesToCode));
                _stateInfoTable = DBHelper.GetDataTable(SQL, "STATEINFO", connectionString, parms);
            }
        }
    }
}
