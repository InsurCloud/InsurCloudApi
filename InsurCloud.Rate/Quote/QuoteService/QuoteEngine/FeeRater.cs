using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace QuoteEngine
{
    public static class FeeRater
    {
        public static void CalculateFees(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            DataTable feesTable = LoadFees(pol, connectionString);
            DataTable policyFeeTable = CreateFeesTable(pol);
            decimal totalFees = 0;
            if (pol.CallingSystem.ToUpper() != "PAS")
            {
                pol.Billing.Fees.Clear();
            }

            DataRow feeRow = null;
            foreach(clsBaseFee fee in pol.Fees)
            {
                DataRow[] rows = feesTable.Select("FeeCode = '" + fee.FeeCode + "'");
                if (rows != null && rows.Count() > 0)
                {
                    feeRow = policyFeeTable.NewRow();
                    feeRow["FeeCode"] = fee.FeeCode;
                }

                foreach (DataRow row in rows)
                {
                    feeRow["Factor"] = row["Factor"];
                    feeRow["FactorType"] = row["FactorType"];
                    feeRow["FeeApplicationType"] = row["FeeApplicationType"];
                    fee.FeeAmt = (decimal)row["Factor"];

                    if (fee.FeeType != "P")
                    {
                        fee.FeeType = row["FactorType"].ToString();
                    }
                    else
                    {
                        if (pol.CallingSystem.ToUpper() != "PAS")
                        {
                            pol.Billing.Fees.Add(fee);
                        }
                    }
                    fee.FeeApplicationType = row["FeeApplicationType"].ToString();
                        
                }
                if (feeRow != null)
                {
                    policyFeeTable.Rows.Add(feeRow);
                    feeRow = null;
                }
                totalFees += fee.FeeAmt;
            }
            pol.TotalFees = totalFees;

        }

        private static DataTable LoadFees(clsPolicyPPA pol, string connectionString)
        {
            clsBaseFee fee = null;            

            pol.Fees.Clear();

            DataTable feeTable = GetFeeTable(pol, connectionString);

            if (feeTable != null)
            {
                foreach (DataRow row in feeTable.Rows)
                {
                    switch (row["FeeCode"].ToString().ToUpper())
                    {
                        case "POLICY":
                            fee = CreateFee(pol.Fees.Count + 1, row["FeeCode"].ToString(), row["Description"].ToString(), row["Description"].ToString(), row["FeeApplicationType"].ToString(), "P");
                            break;
                        case "MVR":

                            clsBaseNote mvrNote = NotesHelper.FindNoteByDescriptionOnly(pol, "Rater MVR Order");
                            if (mvrNote == null)
                            {
                                foreach (clsEntityDriver drv in pol.Drivers)
                                {
                                    if (drv.MVROrderStatus.ToUpper().Trim() == "")
                                    {
                                        if (drv.DriverStatus.ToUpper().Trim() == "PERMITTED" || drv.DriverStatus.ToUpper().Trim() == "ACTIVE")
                                        {
                                            fee = CreateFee(pol.Fees.Count + 1, row["FeeCode"].ToString(), row["Description"].ToString(), string.Concat(row["Description"].ToString(), " for Driver #", drv.IndexNum), row["FeeApplicationType"].ToString(), "P");
                                            pol.Fees.Add(fee);
                                        }
                                    }
                                    else if (drv.MVROrderStatus.ToUpper().Trim() != "NEO" && drv.MVROrderStatus.ToUpper().Trim() != "ERROR")
                                    {
                                        if (drv.DriverStatus.ToUpper().Trim() == "PERMITTED" || drv.DriverStatus.ToUpper().Trim() == "ACTIVE")
                                        {
                                            fee = CreateFee(pol.Fees.Count + 1, row["FeeCode"].ToString(), row["Description"].ToString(), string.Concat(row["Description"].ToString(), " for Driver #", drv.IndexNum), row["FeeApplicationType"].ToString(), "P");
                                            pol.Fees.Add(fee);

                                        }
                                    }
                                }
                            }
                            fee = null;
                            break;
                        case "SR22":
                            if (DriverHelper.HasSR22Drivers(pol))
                            {
                                fee = CreateFee(pol.Fees.Count + 1, row["FeeCode"].ToString(), row["Description"].ToString(), row["Description"].ToString(), row["FeeApplicationType"].ToString(), "P");
                            }
                            break;
                        case "THEFT":
                            fee = CreateFee(pol.Fees.Count + 1, row["FeeCode"].ToString(), row["Description"].ToString(), row["Description"].ToString(), row["FeeApplicationType"].ToString(), "P");
                            break;
                    }
                    if (fee != null)
                    {
                        pol.Fees.Add(fee);
                        fee = null;
                    }
                }
            }
            return feeTable;
        }

        private static clsBaseFee CreateFee(int feeNum, string feeCode, string feeDescription, string feeName, string feeApplication, string feeLevel)
        {
            clsBaseFee fee = new clsBaseFee();
            fee.FeeCode = feeCode;
            fee.FeeDesc = feeDescription;
            fee.FeeName = feeName;
            fee.FeeType = feeLevel;
            fee.FeeApplicationType = feeApplication;
            fee.FeeNum = feeNum;
            fee.IndexNum = fee.FeeNum;
            return fee;
        }

        private static DataTable GetFeeTable(clsPolicyPPA pol, string connectionString)
        {
            string SQL = " SELECT Factor, FeeCode, Description, FeeApplicationType, FactorType FROM pgm" + pol.Product + pol.StateCode + "..FactorFee with(nolock)";
            SQL += "  WHERE Program = @Program ";
            SQL += "  AND EffDate <= @RateDate ";
            SQL += "  AND ExpDate > @RateDate ";
            SQL += "  AND AppliesToCode IN ('B',  @AppliesToCode ) ";
            SQL += "  ORDER BY FeeCode Asc ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@Program", SqlDbType.VarChar, 10, pol.Program));
            parms.Add(DBHelper.AddParm("@RateDate", SqlDbType.DateTime, 8, pol.RateDate));
            parms.Add(DBHelper.AddParm("@AppliesToCode", SqlDbType.VarChar, 1, pol.AppliesToCode));

            return DBHelper.GetDataTable(SQL, "FeeTable", connectionString, parms);            
        }

        private static DataTable CreateFeesTable(clsPolicyPPA pol)
        {
            DataTable feeTable = null;
            DataColumn feeAppType = null;
            DataColumn factorType = null;
            DataColumn feeCode = null;
            DataColumn factor = null;

            feeTable = new DataTable("Fees");
            feeCode = new DataColumn("FeeCode");
            feeTable.Columns.Add(feeCode);
            feeAppType = new DataColumn("FeeApplicationType");
            feeTable.Columns.Add(feeAppType);
            factor = new DataColumn("Factor");
            feeTable.Columns.Add(factor);
            factorType = new DataColumn("FactorType");
            feeTable.Columns.Add(factorType);

            return feeTable;
        }
    }
}
