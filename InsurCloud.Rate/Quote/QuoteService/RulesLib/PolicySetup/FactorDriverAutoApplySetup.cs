using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CorPolicy;
using Helpers;
using RulesLib.Rules;

namespace RulesLib.PolicySetup
{
    public static class FactorDriverAutoApplySetup
    {
        private static DataTable LoadFactorTable(clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
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
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            StateRules2 rules = new StateRules2();
            DataTable factorTable = null;
            factorTable = LoadFactorTable(pol, connectionString, stateInfo);
            FactorsHelper.RemoveAutoApplyFactors(pol, factorTable);

            var grouped = from row in factorTable.Select("Program IN ('PPA', '" + pol.Program + "') AND AutoApply = 1 ").CopyToDataTable().AsEnumerable()
                          group row by row.Field<string>("FactorCode") into groupby
                          select new { FactorCode = groupby.Key };
            foreach (var grp in grouped)
            {
                switch (grp.FactorCode.ToString().ToUpper())
                {
                    case "FOREIGN_LICENSE":
                        foreach (clsEntityDriver drv in pol.Drivers)
                        {
                            if (!drv.IsMarkedForDelete)
                            {
                                if (drv.IndexNum < 98)
                                {
                                    if (DriverHelper.HasForeignLicense(drv) && drv.DriverStatus.ToUpper() == "ACTIVE")
                                    {
                                        AddFactor(pol, drv.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                                    }
                                }
                            }
                        }
                        break;
                    case "OTHERSTATE_LICENSE":
                        foreach (clsEntityDriver drv in pol.Drivers)
                        {
                            if (!drv.IsMarkedForDelete)
                            {
                                if (drv.IndexNum < 98)
                                {
                                    if (!DriverHelper.HasForeignLicense(drv) && drv.DriverStatus.ToUpper() == "ACTIVE" && clsCommonFunctions.GetStateCode(drv.DLNState.Trim()) != pol.StateCode.Trim())
                                    {
                                        AddFactor(pol, drv.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                                    }
                                }
                            }
                        }
                        break;
                    case "CLN_YOUTH":
                        bool addGoodDrvDiscount = false;
                        foreach (clsEntityDriver drv in pol.Drivers)
                        {
                            if (!drv.IsMarkedForDelete)
                            {
                                if (drv.IndexNum < 98)
                                {
                                    if (drv.Age <= 18 && drv.DriverStatus.ToUpper() == "ACTIVE")
                                    {
                                        int numOfBadViolsOnThisDriver = 0;
                                        foreach (clsBaseViolation viol in drv.Violations)
                                        {
                                            if (viol.ViolGroup.ToUpper() == "NAF" || viol.ViolGroup.ToUpper() == "OT1" || viol.ViolGroup.ToUpper() == "OTC" || viol.ViolGroup.ToUpper() == "MIN")
                                            {
                                                //These are OK
                                            }
                                            else
                                            {
                                                numOfBadViolsOnThisDriver++;
                                            }
                                        }
                                        if (numOfBadViolsOnThisDriver == 0)
                                        {
                                            addGoodDrvDiscount = true;
                                        }
                                        if (addGoodDrvDiscount)
                                        {
                                            AddFactor(pol, drv.Factors, connectionString, grp.FactorCode.ToString().ToUpper());
                                        }
                                    }                                    
                                }
                            }
                        }
                        break;
                    case "NO_VIOL":
                        int newestViolMonthsOld = 0;
                        foreach (clsEntityDriver drv in pol.Drivers)
                        {
                            if (drv.DriverStatus.ToUpper() == "ACTIVE" && !drv.SR22)
                            {
                                ViolationHelper.CheckViolations(pol, drv, stateInfo, connectionString);
                                newestViolMonthsOld = ViolationHelper.GetNoViolDiscount(drv, pol);

                                if (newestViolMonthsOld >= 36)
                                {
                                    AddFactor(pol, drv.Factors, connectionString, "NO_VIOL");
                                }
                                else
                                {
                                    if (newestViolMonthsOld >= 18)
                                    {
                                        AddFactor(pol, drv.Factors, connectionString, "NO_VIOL_18");
                                    }
                                    else
                                    {
                                        if (newestViolMonthsOld >= 12)
                                        {
                                            AddFactor(pol, drv.Factors, connectionString, "NO_VIOL_12");
                                        }
                                    }
                                }
                            }
                            
                        }
                        break;
                    case "INEXPERIENCED":
                        foreach (clsEntityDriver drv in pol.Drivers)
                        {
                            ViolationHelper.RemoveViolation(pol, drv, "", "99999");
                            if (drv.DriverStatus.ToUpper() != "ACTIVE")
                            {
                                if (drv.LicenseStateDate != DateTime.MinValue)
                                {
                                    int monthDiff = 0;
                                    DateTime addedDate = DateTime.MinValue;
                                    addedDate = pol.EffDate;
                                    if (drv.AddedDate > pol.EffDate)
                                    {
                                        addedDate = drv.AddedDate;
                                    }
                                    monthDiff = DBHelper.DateDiffMonths(drv.LicenseStateDate, addedDate);

                                    if (drv.LicenseStateDate.Month == addedDate.Month)
                                    {
                                        if (drv.LicenseStateDate.Day < addedDate.Day)
                                        {
                                            monthDiff--;
                                        }
                                    }
                                    if (monthDiff < 36)
                                    {
                                        AddFactor(pol, drv.Factors, connectionString, "INEXPERIENCED");
                                        ViolationHelper.AddInexperiencedViolation(pol, drv);
                                    }
                                }
                            }
                        }
                        break;
                }                
            }
            //RemoveNOVIOLIfINEXPFactor(pol); --NOT NEEDED for Texas
            return "";
        }
        private static void AddFactor(clsPolicyPPA pol, List<clsBaseFactor> factors, string connectionString, string factorCode)
        {
            if (!FactorsHelper.FactorOn(pol.PolicyFactors, factorCode))
            {
                FactorsHelper.AddFactor(pol, factors, factorCode, "POLICY", connectionString);
            }
        }
        private static void AddFactor(clsPolicyPPA pol, List<clsBaseFactor> factors, string connectionString, DataRow row)
        {
            AddFactor(pol, factors, connectionString, row["FactorCode"].ToString().ToUpper());
        }
    }
}
