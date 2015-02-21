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
    public static class ProgramSettingHelper
    {
        
        public static string FindSetting(string settingName, clsPolicyPPA pol, string connectionString)
        {
            string SQL = "";

            SQL = " SELECT Top 1 Value ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + "..ProgramSettings with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode in ('B', @AppliesToCode) ";
            SQL += " AND SettingName = @SettingName ";
            SQL += " AND Program in ('PPA', @Program) ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@SettingName", SqlDbType.VarChar, 50, settingName));

            string value = DBHelper.GetScalarValue(SQL, "Value", connectionString, parms);
            return value;
        }
    }
}
