using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using CorPolicy;

namespace Helpers
{
    public static class ViolationHelper
    {
        public static void AddSR22Violation(clsPolicyPPA pol, clsEntityDriver drv)
        {
            clsBaseViolation viol = GetViolation("SR22", "59998", drv);
            if (viol == null && pol.StateCode == "17" && pol.Program.ToUpper() == "MONTHLY")
            {
                viol = GetViolation("XX2", "59998", drv);
            }

            if (viol == null)
            {
                viol = new clsBaseViolation();
                viol.ViolTypeCode = "59998";
                viol.ViolDesc = "SR-22 FILING";
                viol.ViolTypeIndicator = "V";
                viol.ViolGroup = "S22";
                viol.ViolSourceCode = "M";
                viol.AtFault = false;
                viol.ViolDate = drv.SR22Date;
                viol.ConvictionDate = viol.ViolDate;
                viol.Chargeable = true;
                viol.IndexNum = drv.Violations.Count + 1;
                viol.AddToXML = true;
                if (pol.StateCode != "09")
                {
                    drv.Violations.Add(viol);
                }
            }

        }
        public static void AddInexperiencedViolation(clsPolicyPPA pol, clsEntityDriver drv)
        {
            clsBaseViolation viol = GetViolation("MED", "99999", drv);
            
            if (viol == null)
            {
                viol = new clsBaseViolation();
                viol.ViolTypeCode = "99999";
                viol.ViolDesc = "Operators licensed for less than 3 years (" + drv.LicenseStateDate + ")";
                viol.ViolTypeIndicator = "V";
                viol.ViolGroup = "MED";
                viol.ViolSourceCode = "M";
                viol.AtFault = false;
                viol.ViolDate = DateTime.Now;
                viol.ConvictionDate = viol.ViolDate;
                viol.Chargeable = true;
                viol.IndexNum = drv.Violations.Count + 1;
                viol.AddToXML = true;
                drv.Violations.Add(viol);                
            }
        }
        public static void RemoveViolation(clsPolicyPPA pol, clsEntityDriver drv, string violGroup = "", string violTypeCode = "")
        {
            foreach (clsBaseViolation viol in drv.Violations)
            {
                if (violGroup != string.Empty)
                {
                    if (viol.ViolGroup.ToUpper() == violGroup)
                    {
                        drv.Violations.Remove(viol);
                        break;
                    }
                }
                else if (violTypeCode != string.Empty)
                {
                    if (viol.ViolTypeCode == violTypeCode)
                    {
                        drv.Violations.Remove(viol);
                        break;
                    }
                }
                
            }
        }
        public static void RemoveSR22Violation(clsPolicyPPA pol, clsEntityDriver drv)
        {
            RemoveViolation(pol, drv, "S22");            
        }
        public static clsBaseViolation GetViolation(string violGroup, string violTypeCode, clsEntityDriver drv)
        {
            clsBaseViolation retViol = null;
            foreach (clsBaseViolation viol in drv.Violations)
            {
                if (viol.ViolGroup.ToUpper() == viol.ViolGroup.ToUpper() && viol.ViolTypeCode.ToUpper() == violTypeCode.ToUpper())
                {
                    retViol = viol;
                    break;
                }
            }
            return retViol;
        }
        public static void CheckViolations(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString)
        {
            
            DataTable violGroupsTable = LoadCodeViolGroupsTable(pol, connectionString);
            DataTable violations = new DataTable("Violations");
            violations.Columns.Add(new DataColumn("ViolationIndex", Type.GetType("System.Int32")));
            violations.Columns.Add(new DataColumn("ViolTypeCode", Type.GetType("System.String")));
            violations.Columns.Add(new DataColumn("ViolGroup", Type.GetType("System.String")));
            violations.Columns.Add(new DataColumn("ViolDate", Type.GetType("System.DateTime")));
            violations.Columns.Add(new DataColumn("Chargeable", Type.GetType("System.Boolean")));
            violations.Columns.Add(new DataColumn("IsFirst", Type.GetType("System.Boolean")));
            violations.Columns.Add(new DataColumn("IsSecond", Type.GetType("System.Boolean")));
            violations.Columns.Add(new DataColumn("Points", Type.GetType("System.Int32")));

            drv.Points = 0;
            violations.Rows.Clear();
            bool ignoreOutOfAgeRange = false;
            ignoreOutOfAgeRange = stateInfo.Contains(pol, "VIOLATION", "OCCURRENCE", "IGNOREOUTOFAGERANGE", connectionString);
            if (ignoreOutOfAgeRange)
            {
                drv.Violations.Sort(CompareViolationsByDate);
            }

            for (int i = 0; i < drv.Violations.Count - 1; i++)
            {
                clsBaseViolation viol = drv.Violations[i];
                DateTime newDate = pol.EffDate;

                if (stateInfo.Contains(pol, "ALLOW", "RECALC", "POINTS", connectionString))
                {
                    string violGroup = GetViolCodeGroup(viol.ViolTypeCode, pol, connectionString);
                    if (violGroup == null)
                    {
                        violGroup = string.Empty;
                    }
                    if (violGroup.Trim().Length > 0)
                    {
                        viol.ViolGroup = violGroup;
                    }
                }

                SetupUDRViolDate(pol, viol);
                viol.ConvictionDate = viol.ViolDate;

                DataRow row = violations.NewRow();
                row["ViolationIndex"] = viol.IndexNum;
                row["ViolTypeCode"] = viol.ViolTypeCode.Trim();
                row["ViolGroup"] = viol.ViolGroup.Trim();
                row["ViolDate"] = viol.ViolDate;
                row["Chargeable"] = true; //oViol.Chargeable
                int monthsOld = CalculateViolAgeInMonths(viol.ViolDate, pol.EffDate);
                if (monthsOld < 0) monthsOld = 0;

                DataRow[] dataRows = violGroupsTable.Select("Program = '" + pol.Program + "' AND ViolGroup = '" + viol.ViolGroup.Trim() + "' AND MinAgeViol <= " + monthsOld + " AND MaxAgeViol > " + monthsOld);

                if (dataRows.Length == 0)
                {
                    row["Chargeable"] = false;
                    row["IsFirst"] = false;
                    row["IsSecond"] = false;
                    row["Points"] = 0;
                }
                else
                {
                    if ((bool)row["Chargeable"])
                    {
                        foreach (DataRow checkRow in dataRows)
                        {
                            int occurrence = GetOccurrence(drv.Violations, viol.ViolGroup.Trim(), i, (int)checkRow["MinAgeViol"], (int)checkRow["MaxAgeViol"], monthsOld, pol.EffDate, ignoreOutOfAgeRange);
                            switch(occurrence){
                                case 1:
                                    row["IsFirst"] = true;
                                    row["IsSecond"] = false;
                                    row["Points"] = (int)checkRow["FirstOccurrence"];                                        
                                    break;
                                case 2:
                                    row["IsFirst"] = false;
                                    row["IsSecond"] = true;
                                    row["Points"] = (int)checkRow["SecondOccurrence"];
                                    break;
                                default:
                                    row["IsFirst"] = false;
                                    row["IsSecond"] = false;
                                    row["Points"] = (int)checkRow["AddlOccurrence"];
                                    break;

                            }
                            if(DriverHelper.ShouldIgnoreViolationPoints(row["ViolGroup"].ToString(), pol, drv, stateInfo, connectionString)){
                                row["Points"] = 0;
                            }
                        }
                    }
                }
                violations.Rows.Add(row);
            }

            //Same Day Violation Handling
            DataTable violDates = DBHelper.SelectDistinct(violations, "ViolDate");
            foreach(DataRow row in violDates.Rows){
                DateTime violDate = (DateTime)row["ViolDate"];

                DataRow[] viols = violations.Select("Chargeable = 'True' and ViolDate = '" + violDate + "'", "Points DESC");
                if(viols.Length == 1){
                    //Don't have to change anything. There is only one violation for that date
                }else{
                    for(int i = 0; i < viols.Length - 1; i++){
                        DataRow viol = viols[i];
                        if(i == 0){
                            //Don't have to change anything. This should be the violation with the highest point value for that date
                        }else{
                            foreach(DataRow drViol in violations.Rows){
                                if(drViol.Equals(viol)){
                                    drViol["Chargeable"] = false;
                                    drViol["Points"] = 0;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach(DataRow row in violations.Rows){
                foreach(clsBaseViolation viol in drv.Violations){
                    if(!DriverHelper.ShouldIgnoreViolationPoints(viol.ViolGroup.ToUpper(), pol, drv, stateInfo, connectionString)){
                        if((int)row["ViolationIndex"] == viol.IndexNum){
                            viol.Points = (int)row["Points"];
                            viol.Chargeable = (bool)row["Chargeable"];
                            drv.Points += viol.Points;
                            break;
                        }
                    }else{
                        if((int)row["ViolationIndex"] == viol.IndexNum){                            
                            viol.Chargeable = (bool)row["Chargeable"];                            
                            break;
                        }
                    }
                }
            }

        }
        private static int GetOccurrence(List<clsBaseViolation> violations, string violGroup, int violNum, int minAgeViol, int maxAgeViol, int monthsOld, DateTime effDate, bool ignoreOutOfAgeRange)
        {
            int occurrence = 0;
            for (int i = 0; i <= violNum; i++)
            {
                if (violations[i].ViolGroup.ToUpper() == violGroup.ToUpper())
                {
                    int violAge = CalculateViolAgeInMonths(violations[i].ViolDate, effDate);
                    if (violAge < 0)
                    {
                        violAge = 0;
                    }
                    if ((minAgeViol <= monthsOld && maxAgeViol > monthsOld) && (!ignoreOutOfAgeRange || (minAgeViol <= violAge && maxAgeViol > violAge)))
                    {
                        occurrence++;
                    }
                }
            }
            return occurrence;
        }
        private static DataTable LoadCodeViolGroupsTable(clsPolicyPPA pol, string connectionString)
        {
            string SQL = "";

            SQL = " SELECT Program, ViolGroup, FirstOccurrence, SecondOccurrence, AddlOccurrence, MinAgeViol, MaxAgeViol ";
            SQL += " FROM pgm" + pol.Product + pol.StateCode + ".." + "CodeViolGroups with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " ORDER BY Program, ViolGroup ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            return DBHelper.GetDataTable(SQL, "CodeViolGroups", connectionString, parms);
        }    
        public static int CalculateViolAgeInMonths(DateTime violDate, DateTime effDate)
        {
            double violAge = 0;
            violAge = ((effDate.Year - violDate.Year) * 12) + effDate.Month - violDate.Month;
            if(violDate.Day > effDate.Day){
                violAge--;
            }
            if(violAge < 0){
                violAge++;
            }
            return (int)violAge;
        }
        private static void SetupUDRViolDate(clsPolicyPPA pol, clsBaseViolation viol)
        {
            DateTime newDate = DateTime.MinValue;
            if (viol.ViolGroup.ToUpper() == "UDR" && viol.ViolTypeCode == "55559")
            {
                if (pol.CallingSystem == "WEBRATER" || pol.CallingSystem == "BRIDGE")
                {
                    newDate = pol.EffDate;
                    viol.ViolDate = newDate.AddDays(-1);
                }
            }            
        }
        public static string GetViolCodeGroup(string violCode, clsPolicyPPA pol, string connectionString)
        {
            string SQL = "";

            SQL = " SELECT ViolGroup FROM pgm" + pol.Product + pol.StateCode + ".." + "CodeViolCodes with(nolock)";
            SQL += " WHERE EffDate <= @RateDate ";
            SQL += " AND ExpDate > @RateDate ";
            SQL += " AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += " AND ViolCode = @ViolCode ";
            SQL += " AND Program = @Program ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));
            parms.Add(DBHelper.AddParm("@ViolCode", SqlDbType.VarChar, 10, violCode));

            return DBHelper.GetScalarValue(SQL, "ViolGroup", connectionString, parms);

        }
        private static int CompareViolationsByDate(clsBaseViolation x, clsBaseViolation y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    return x.ViolDate.CompareTo(y.ViolDate);
                }
            }
        }
        public static int GetNoViolDiscount(clsEntityDriver drv, clsPolicyPPA pol)
        {
            int discountToAdd = -1;
            int pts36 = 0;
            int pts18 = 0;
            int pts12 = 0;

            foreach (clsBaseViolation viol in drv.Violations)
            {
                int tempMonthsOld = 0;
                tempMonthsOld = CalculateViolAgeInMonths(viol.ViolDate, pol.EffDate);
                if (tempMonthsOld < 0)
                {
                    tempMonthsOld = 0;
                }

                if (tempMonthsOld < 12)
                {
                    pts12 += viol.Points;
                }
                else if (tempMonthsOld < 18)
                {
                    pts18 += viol.Points;
                }
                else if (tempMonthsOld < 36)
                {
                    pts36 += viol.Points;
                }
                   
            }

            bool discountAdded = false;
            if (pts36 + pts18 + pts12 < 2 && !discountAdded)
            {
                discountToAdd = 36;
                discountAdded = true;
            }
            if (pts18 + pts12 < 2 && !discountAdded)
            {
                discountToAdd = 18;
                discountAdded = true;
            }
            if (pts12 < 2 && !discountAdded)
            {
                discountToAdd = 12;
                discountAdded = true;
            }
            return discountToAdd;
        }

        public static int ChargeableViolationCount(clsEntityDriver drv)
        {
            int violCount = 0;
            foreach (clsBaseViolation viol in drv.Violations)
            {
                if (viol.Chargeable)
                {
                    violCount++;
                }
            }
            return violCount;
        }
    }
}
