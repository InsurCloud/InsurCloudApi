using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Helpers.Models
{

    public class DLNPattern
    {
        public string State { get; set; }
        public string Number { get; set; }
        public string Pattern { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public bool IsValid { get; set; }
    }

    public class DLNPatterns
    {
        private static List<DLNPattern> patterns = null;
        private static bool loading = false;
        private static DateTime loadDate = DateTime.MinValue;
        
        public DLNPattern FormatDLN(string DLN, string State, string connectionString)
        {            
            DLNPattern dlnFormat;            
            DLN.Replace("-", "").Replace(" ", "").ToUpper();
            dlnFormat = getPattern(State, connectionString);
            dlnFormat.IsValid = false;
            if (dlnFormat != null && dlnFormat.Pattern.Length > 0)
            {
                dlnFormat.IsValid = Regex.IsMatch(DLN, dlnFormat.Pattern);

            }
            else
            {
                dlnFormat = new DLNPattern();
                dlnFormat.State = State;
                dlnFormat.Message = "Pattern Not Found for State: '" + State + "'";                
            }

            dlnFormat.Number = DLN;

            if (!dlnFormat.IsValid)
            {
                dlnFormat.Message = "Invalid Driver's License, Expected Format: '" + dlnFormat.Description + "'";                
            }
            else
            {
                dlnFormat.Message = "Driver's License format is valid";
            }
            return dlnFormat;
        }

        private bool ShouldReloadPatterns()
        {
            if (patterns == null) return true;
            if (loading == true) return false;
            TimeSpan t = DateTime.Now - loadDate;
            if (t.Days > 1) return true;
            return false;
        }
        public DLNPattern getPattern(string StateAbbr, string connectionString)
        {
            if (ShouldReloadPatterns())
            {
                loading = true;
                loadPatterns(connectionString);
                loading = false;
            }            
            return patterns.Where(o => o.State == StateAbbr).FirstOrDefault();
        }

        private void loadPatterns(string connectionString)
        {
            //TODO: Configure Connection String
            DataTable dlPatternsTable = DBHelper.GetDataTable("SELECT * FROM COMMON..DLPatterns", "DLPatterns", connectionString, new List<SqlParameter>(), "common");
            List<DLNPattern> loadPats = new List<DLNPattern>();

            foreach (DataRow row in dlPatternsTable.Rows)
            {
                DLNPattern d = new DLNPattern();
                d.Pattern = row["Pattern"].ToString();
                d.State = row["State"].ToString();
                d.Description = row["Comment"].ToString();
                loadPats.Add(d);
            }            
            patterns = loadPats;
        }
    }
}
