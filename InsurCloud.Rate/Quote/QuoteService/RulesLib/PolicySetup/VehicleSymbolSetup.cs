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

namespace RulesLib.PolicySetup
{
    public static class VehicleSymbolSetup
    {
        private static VINServiceLib.VINService vinSvc;

        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            if (vinSvc == null)
            {
                vinSvc = new VINServiceLib.VINService();
            }
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete && (veh.IsNew || veh.IsModified || veh.PIPMedLiabilityCode == string.Empty || veh.VehicleSymbolCode == string.Empty || veh.LiabilitySymbolCode == string.Empty))
                {
                    //VINService.VinServiceClient client = new VINService.VinServiceClient();
                    DataSet ds = vinSvc.BridgeVINData(veh.VinNo);
                    if (ds != null && ds.Tables.Count >= 1)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows[0] != null && dt.Rows[0]["VINCode"].ToString() != "")
                        {
                            DataRow dr = dt.Rows[0];
                            veh.ValidVIN = true;
                            veh.VehicleYear = dr["ModelYear"].ToString();
                            veh.VehicleMakeCode = dr["VehicleMakeCode"].ToString();
                            veh.VehicleModelCode = dr["VehicleModelCode"].ToString();
                            veh.VehicleRestraintTypeCode = dr["VehicleRestraintTypeCode"].ToString();
                            veh.VehiclePerformanceCode = dr["CountryVehiclePerformanceCode"].ToString();
                            veh.VehicleCylinderCode = dr["VehicleCylinderCode"].ToString();
                            veh.VehicleBodyStyleCode = dr["VehicleBodyStyleCode"].ToString();
                            veh.VehicleAntiTheftCode = dr["VehicleAntiTheftCode"].ToString();
                            veh.VehicleABSCode = dr["VehicleABSCode"].ToString();
                            veh.VehicleClassCode = dr["VehicleClassCode"].ToString();
                            veh.VehicleDaytimeLightCode = dr["VehicleDaytimeLightCode"].ToString();
                            veh.VehicleEngineTypeCode = dr["VehicleEngineTypeCode"].ToString();
                            veh.LiabilitySymbolCode = dr["LiabilitySymbolCode"].ToString();
                            veh.PIPMedLiabilityCode = dr["PIPMedPaySymbolCode"].ToString();
                            veh.CollSymbolCode = dr["CollSymbolCode"].ToString();
                            veh.CompSymbolCode = dr["CompSymbolCode"].ToString();
                            veh.PriceNewSymbolCode = dr["VSRVehicleSymbolCode"].ToString();
                            veh.VehicleSymbolCode = dr["NonVSRVehicleSymbolCode"].ToString();
                        }
                        else
                        {
                            veh.ValidVIN = false;
                        }
                    }
                }

            }
            return "";
        }
    }
}
