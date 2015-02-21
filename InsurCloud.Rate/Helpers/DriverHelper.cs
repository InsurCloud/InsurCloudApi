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
using Helpers.Models;

namespace Helpers
{
    public static class DriverHelper
    {
        public static bool HasForeignLicense(clsEntityDriver drv)
        {
            switch (drv.DLNState.ToUpper().Trim())
            {
                case "FN":
                case "IT":
                case "VI":
                case "AS":
                case "FM":
                case "GU":
                case "MH":
                case "MP":
                case "PR":
                case "PW":
                case "ON":
                case "AE":
                case "AP":
                case "AA":
                case "JZ":
                    return true;
                default:
                    return false;
            }
        }

        public static bool ShouldIgnoreViolationPoints(string violGroup, clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString)
        {
            bool ignoreViol = false;

            if (violGroup.ToUpper().Trim() == "UDR")
            {
                bool bypassUDR = false;
                bypassUDR = stateInfo.Contains(pol, "IGNOREVIOL", "UDR", "UDR", connectionString);
                if ((pol.Program.ToUpper() == "SUMMIT" || bypassUDR) && drv.Age <= 18)
                {
                    ignoreViol = true;
                }
                if ((pol.Program.ToUpper() == "SUMMIT" || bypassUDR) && DriverHelper.HasForeignLicense(drv))
                {
                    ignoreViol = true;
                }
            }
            return ignoreViol;
        }

        public static bool HasActiveDrivers(clsPolicyPPA pol)
        {
            bool hasActiveDrivers = false;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.DriverStatus.ToUpper().Trim() == "ACTIVE")
                {
                    hasActiveDrivers = true;
                    break;
                }
            }
            return hasActiveDrivers;
        }

        public static void RemoveAutoApplyFactors(DataRow[] rows, clsPolicyPPA pol)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete)
                {
                    for (int i = drv.Factors.Count - 1; i >= 0; i--)
                    {
                        foreach (DataRow row in rows)
                        {
                            if (row["FactorCode"].ToString().ToUpper() == drv.Factors[i].FactorCode.ToUpper())
                            {
                                drv.Factors.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static bool HasExcludedDrivers(clsPolicyPPA pol)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete)
                {
                    if (drv.IndexNum < 98)
                    {
                        if (drv.RelationToInsured.ToUpper() == "SPOUSE" || drv.RelationToInsured.ToUpper() == "PARENT")
                        {
                            if (drv.DriverStatus.ToUpper() == "EXCLUDED")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool DriverApplies(clsEntityDriver drv, clsPolicyPPA pol)
        {
            if (pol.CallingSystem.ToUpper().Contains("OLE") || pol.CallingSystem.ToUpper().Contains("UWC"))
            {
                if (drv.IsNew) return true;
                if (drv.IsModified) return true;
                return false;
            }
            return true;
        }

        public static bool IsActiveOrExcludedDriver(clsEntityDriver drv)
        {
            if (drv.DriverStatus.ToUpper().Trim() != string.Empty)
            {
                if (drv.DriverStatus.ToUpper().Trim() == "PERMITTED" || drv.DriverStatus.ToUpper() == "NHH") return false;
                return true;
            }
            return true;
        }

        public static void CleanDriverName(clsEntityDriver drv)
        {
            if (drv.EntityName1.Contains(" "))
            {
                if (drv.EntityName2.Length > 1)
                {
                    drv.EntityName2 = drv.EntityName1.Substring(drv.EntityName1.IndexOf(" ") + 1) + " " + drv.EntityName2;
                }
                else
                {
                    drv.EntityName2 = drv.EntityName1.Substring(drv.EntityName1.IndexOf(" ") + 1);
                }
                drv.EntityName1 = drv.EntityName1.Split(' ')[0];
            }
        }

        public static bool NameMissing(clsEntityDriver drv)
        {
            if (drv.EntityName1.Trim() == string.Empty ||
                drv.EntityName2.Trim() == string.Empty ||
                drv.EntityName1.Contains("undefined") ||
                drv.EntityName2.Contains("undefined"))
            {
                return true;
            }
            return false;
        }

        public static clsEntityDriver FindDriverByAssignment(clsPolicyPPA pol, int assignedDriverNum)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.IndexNum == assignedDriverNum)
                {
                    return drv;
                }
            }
            return null;
        }

        public static DateTime GetNextBirthDay(int year, int month, int day)
        {
            DateTime nextBDay = DateTime.MinValue;
            try
            {
                nextBDay = new DateTime(year, month, day);
            }
            catch
            {
                if (month == 2 && day == 29)
                {
                    nextBDay = new DateTime(year, 3, 1);
                }
            }
            return nextBDay;
        }
        public static string GetDriverPoints(clsPolicyPPA pol)
        {
            string driverPoints = "";
            int driverCount = 0;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (driverCount > 0) driverPoints += ", ";
                driverPoints += drv.Points;
                driverCount++;
            }
            return driverPoints;
        }
        public static string GetDriverClassDefinitions(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverClass = "";
            int driverCount = 0;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (driverCount > 0) driverClass += ", ";
                driverClass += "'" + GetDriverClassDefinition(drv, pol, stateInfo, connectionString) + "'";
                driverCount++;
            }
            return driverClass;
        }
        public static string GetDriverClassDefinition(clsEntityDriver drv, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {

            int driverAge = GetDriverAge(drv, pol, stateInfo, connectionString);

            string driverClass = "";
            driverClass = GetMaritalStatus(pol, stateInfo, connectionString, drv);

            driverClass = string.Concat(driverClass, (drv.Gender.Trim().ToUpper().StartsWith("M")) ? "M" : "F");
            if (drv.Age > 99)
            {
                driverClass += "99";
            }
            else
            {
                driverClass += driverAge.ToString();
            }
            return driverClass;
        }

        public static string GetMaritalStatus(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString, clsEntityDriver drv = null)
        {
            string maritalStatus = "";
            if (drv == null)
            {
                maritalStatus = pol.PolicyInsured.MaritalStatus;
            }
            else
            {
                maritalStatus = drv.MaritalStatus;
            }

            if (stateInfo.Contains(pol, "RATE", "WIDOW", "MARRIED", connectionString))
            {
                maritalStatus = (maritalStatus.Trim().ToUpper() == "MARRIED" || maritalStatus.Trim().ToUpper() == "WIDOWED") ? "M" : "S";
            }
            else
            {
                maritalStatus = (maritalStatus.Trim().ToUpper() == "MARRIED") ? "M" : "S";
            }
            return maritalStatus;
        }

        public static int GetDriverAge(clsEntityDriver drv, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int driverAge = drv.Age;
            if (UseDriverAgeBumping(pol, stateInfo, connectionString))
            {
                if (drv.Age <= 24 && drv.DOB > DateTime.MinValue)
                {
                    DateTime nextBDay = GetNextBirthDay(pol.EffDate.Year, drv.DOB.Month, drv.DOB.Day);

                    if (nextBDay < pol.EffDate)
                    {
                        nextBDay = GetNextBirthDay(pol.EffDate.Year + 1, drv.DOB.Month, drv.DOB.Day);
                    }

                    TimeSpan t = pol.EffDate - nextBDay;
                    if (t.Days < 30 && t.Days > 0)
                    {
                        driverAge++;
                    }
                }
            }
            return driverAge;
        }

        public static bool UseDriverAgeBumping(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (pol.Program.ToUpper() == "MONTHLY") return true;
            if (pol.Program.ToUpper() == "CLASSIC" || pol.Program.ToUpper() == "DIRECT")
            {
                if (pol.StateCode == "17" || pol.StateCode == "42" || pol.StateCode == "03" || pol.StateCode == "35")
                {
                    if (stateInfo.Contains(pol, "RATE", "DRIVER", "AGEBUMP", connectionString))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool GetParentChildRelationshipIndicator(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.DriverStatus.ToUpper() == "ACTIVE" && !drv.IsMarkedForDelete)
                {
                    if (drv.RelationToInsured.ToUpper() == "CHILD" || drv.RelationToInsured.ToUpper() == "PARENT")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasSR22Drivers(clsPolicyPPA pol)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete)
                {
                    foreach (clsBaseFactor factor in drv.Factors)
                    {
                        if (factor.FactorCode.ToUpper().Trim() == "SR22")
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void RemoveDefaultAndCombinedAverageDrivers(clsPolicyPPA pol)
        {
            for (int x = pol.Drivers.Count - 1; x >= 0; x--)
            {
                if (pol.Drivers[x].IndexNum == 99 || pol.Drivers[x].IndexNum == 98)
                {
                    pol.Drivers.Remove(pol.Drivers[x]);
                }
            }
        }

        public static bool DriversHaveValidDLN(clsPolicyPPA pol, string connectionString)
        {
            bool allValid = false;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete)
                {
                    DLNPattern pat = DLNFormat(drv, connectionString);
                    if (!pat.IsValid)
                    {
                        allValid = false;
                        NotesHelper.AddNote(pol, pat.Message, "InvalidDLNFormat", "RULES");
                    }
                }
            }
            return allValid;
        }

        public static DLNPattern DLNFormat(clsEntityDriver drv, string connectionString)
        {
            DLNPatterns pats = new DLNPatterns();
            DLNPattern pat = pats.FormatDLN(drv.DLN, drv.DLNState, connectionString);
            return pat;
        }

        public static void CheckForDriverRestrictions(clsPolicyPPA pol, string connectionString)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverIsRestricted(drv, connectionString))
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: Driver is an unacceptable risk.  Driver- " + drv.IndexNum + ".", "RestrictedDLN", "IER");
                }
            }
        }

        public static bool DriverIsRestricted(clsEntityDriver drv, string connectionString)
        {
            if ((drv.DriverStatus.ToUpper() == "ACTIVE" || drv.DriverStatus.ToUpper() == "PERMITTED") && !drv.IsMarkedForDelete)
            {
                DLNPattern pat = DLNFormat(drv, connectionString);
                if (pat.IsValid)
                {
                    string SQL = " SELECT * FROM Common..DriverRestriction ";
                    SQL += " WHERE DLN = @DLN AND ";
                    SQL += " DLNState = @DLNState AND ";
                    SQL += " IsNull(UnrestrictredByUser, '') = '' ";

                    List<SqlParameter> parms = new List<SqlParameter>();

                    parms.Add(DBHelper.AddParm("@DLN", SqlDbType.VarChar, 50, pat.Number));
                    parms.Add(DBHelper.AddParm("@DLNState", SqlDbType.VarChar, 2, pat.State));

                    DataTable restrictions = DBHelper.GetDataTable(SQL, "DriverRestrictions", connectionString, parms, "Common");
                    if (restrictions != null && restrictions.Rows.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            
        }

        public static int CleanViolationPoints(clsPolicyPPA pol, clsEntityDriver drv, StateInfoHelper stateInfo, string connectionString, bool resetOver30)
        {
            int points = (drv.Points > 30 && resetOver30) ? 30 : drv.Points;

            foreach (clsBaseViolation viol in drv.Violations)
            {
                bool removedItem = false;
                if (!removedItem && stateInfo.Contains(pol, "COMBINEDDRIVER", "VIOLIGNORE", viol.ViolTypeCode, connectionString))
                {
                    points = points - viol.Points;
                    removedItem = true;
                }
                if (!removedItem && stateInfo.Contains(pol, "COMBINEDDRIVER", "VIOLGROUPIGNORE", viol.ViolTypeCode, connectionString))
                {
                    points = points - viol.Points;
                    removedItem = true;
                }
                if (!removedItem && stateInfo.Contains(pol, "VIOLDATE", "PASCALC", viol.ViolTypeCode, connectionString))
                {
                    if (viol.IgnoredForRating || CalculateViolAge(viol.ViolDate, pol.EffDate, pol, stateInfo, connectionString) >= 36)
                    {
                        points = points - viol.Points;
                        removedItem = true;
                    }
                }
            }
            return points;
        }

        private static int CalculateViolAge(DateTime violDate, DateTime effDate, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            Double violAge = 0;
            violAge = ((effDate.Year - violDate.Year) * 12) + effDate.Month - violDate.Month;
            if (stateInfo.Contains(pol, "VIOLDATE", "PASCALC", "", connectionString))
            {
                int years;
                TimeSpan ts = effDate - violDate;
                years = (int)Math.Floor((double)(ts.Days / 365));
                switch (years)
                {
                    case 0:
                        violAge = 11;
                        break;
                    case 1:
                        violAge = 23;
                        break;
                    case 2:
                        violAge = 35;
                        break;
                }

                if (pol.StateCode != "42")
                {
                    if (((effDate.Year - violDate.Year) * 12) + effDate.Month - violDate.Month == 35)
                    {
                        if (effDate.Day <= violDate.Day)
                        {
                            violAge = 36;
                        }
                        else
                        {
                            violAge = 35;
                        }
                    }
                    else if (((effDate.Year - violDate.Year) * 12) + effDate.Month - violDate.Month > 35)
                    {
                        violAge = 36;
                    }
                }
            }
            else
            {
                if (violDate.Day > effDate.Day)
                {
                    violAge = violAge - 1;
                }
                if (violAge < 0)
                {
                    violAge = violAge + 1;
                }
            }
            return (int)violAge;

        }

        public static bool ShouldRateDriver(clsEntityDriver drv, clsPolicyPPA pol)
        {
            if (drv.DriverStatus.ToUpper().Trim() == "ACTIVE")
            {
                return true;
            }

            if (pol.StateCode == "17" && pol.Program.ToUpper().Trim() == "MONTHLY")
            {
                if (drv.DriverStatus.ToUpper().Trim() == "PERMITTED")
                {
                    return true;
                }
            }
            return false;
        }
    }

}
