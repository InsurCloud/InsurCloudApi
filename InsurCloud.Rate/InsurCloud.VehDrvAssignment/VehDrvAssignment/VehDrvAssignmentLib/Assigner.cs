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
using Helpers;

namespace VehDrvAssignmentLib
{
    public class Assigner
    {
        
        string _driverAssignmentType = string.Empty;

        public clsPolicyPPA SetVehDrvAssignmentsSpecify(clsPolicyPPA pol, string assignmentType)
        {
            if (assignmentType.ToUpper().Contains("AVERAGEDRIVER"))
            {
                _driverAssignmentType = "HIGHTOHIGH";
            }
            else
            {
                _driverAssignmentType = assignmentType;
            }
            return SetVehDrvAssignments(pol, assignmentType);
        }

        public clsPolicyPPA SetVehDrvAssignments(clsPolicyPPA pol, string assignmentType = "")
        {            
            string connectionString = "Server=tcp:emuxtovazm.database.windows.net,1433;Database=pgm242;User ID=AppUser@emuxtovazm;Password=AppU$er!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
            StateInfoHelper stateInfo = new StateInfoHelper(pol.Product, pol.StateCode, pol.RateDate, pol.AppliesToCode, pol.ProgramInfo.CompanyCode, connectionString);
            
            try
            {
                List<string> coverageList = GetCoverageList(pol, connectionString);
                List<DataTable> driverFactorTables = FactorsHelper.CreateDataTables(pol, connectionString);
                setDriverFactors(pol, driverFactorTables, coverageList, stateInfo, connectionString);                
                DataRow[] vehDataRows = null;
                IAssigner assigner = null;
                switch (PolicyHelper.GetDriverAssignmentType(pol, stateInfo, connectionString, assignmentType).ToUpper())
                {
                    case "AVERAGEDRIVER":
                        assigner = new AverageDriverAssigner();
                        assigner.Execute(coverageList, driverFactorTables, pol, stateInfo, connectionString);
                        break;
                    case "HIGHTOHIGH":
                        assigner = new HighToHighAssigner();
                        assigner.Execute(coverageList, driverFactorTables, pol, stateInfo, connectionString);
                        vehDataRows = assigner.Rows;
                        break;
                    case "HIGHTOHIGHVEHCOV":
                        assigner = new HighToHighByVehCoverageAssigner();
                        assigner.Execute(coverageList, driverFactorTables, pol, stateInfo, connectionString);
                        break;

                    case "HIGHTOHIGHALLCOV":
                        assigner = new HighToHighByAllCoverageAssigner();
                        assigner.Execute(coverageList, driverFactorTables, pol, stateInfo, connectionString);
                        break;
                }

                if (stateInfo.Contains(pol, "RATE", "EXCL", "SINGLE", connectionString))
                {
                    int exclCount = 0;
                    foreach (clsEntityDriver drv in pol.Drivers)
                    {
                        if (drv.DriverStatus.ToUpper() == "EXCLUDED")
                        {
                            exclCount++;
                        }
                    }

                    if (vehDataRows != null)
                    {
                        foreach (DataRow row in vehDataRows)
                        {
                            foreach (clsVehicleUnit veh in pol.VehicleUnits)
                            {
                                if (!veh.IsMarkedForDelete)
                                {
                                    if ((int)row["VehicleNum"] == veh.IndexNum)
                                    {
                                        if (exclCount == 0)
                                        {
                                            for (int i = 0; i < veh.Factors.Count - 1; i++)
                                            {
                                                if (veh.Factors[i].FactorCode.ToUpper() == "EXCL")
                                                {
                                                    veh.Factors.Remove(veh.Factors[i]);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            exclCount--;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error in Assigning Drivers to Vehicles (PolicyID " + pol.PolicyID + "): " + ex.Message, ex);
            }
            return pol;
        }
        
        private void setDriverFactors(clsPolicyPPA pol, List<DataTable> driverFactorTables, List<string> coverageList, StateInfoHelper stateInfo, string connectionString)
        {

            DataTable factorTable = null;

            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.IndexNum == 99 || drv.IndexNum == 98)
                {
                    //Do Nothing
                }
                else
                {
                    if (!drv.IsMarkedForDelete && DriverHelper.ShouldRateDriver(drv, pol))
                    {
                        foreach (DataTable drvFacTable in driverFactorTables)
                        {
                            if (drvFacTable.TableName == drv.IndexNum.ToString())
                            {
                                factorTable = drvFacTable;
                                break;
                            }
                        }
                        FactorsHelper.GetDriverFactorRows(pol, drv, factorTable, stateInfo, connectionString);                        
                    }
                }
            }
        }
        
        private List<string> GetCoverageList(clsPolicyPPA pol, string connectionString)
        {
            DataRow[] dataRows;
            DataTable baseRateTable = FactorsHelper.GetBaseRateTable(pol, connectionString);

            dataRows = baseRateTable.Select();

            List<string> coverageList = new List<string>();
            coverageList.Add("Initialize");

            foreach (DataRow row in dataRows)
            {
                coverageList.Add(row["Coverage"].ToString());
            }
            return coverageList;


        }
    }
}
