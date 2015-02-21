using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.PolicySetup
{
    public static class VehiclesTrueAgeSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                int vehAge = VehicleHelper.CalculateAge(int.Parse(veh.VehicleYear), pol.EffDate.Year, pol.EffDate.Month);                                
                veh.VehicleAge = vehAge;
            }
            return "";
        }
    }
}
