using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;
using RulesLib.VehicleDriverAssignmentService;


namespace RulesLib.PolicySetup
{
    public static class VehicleDriverAssignmentSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            VehDrvAssignmentLib.Assigner assigner = new VehDrvAssignmentLib.Assigner();
            //VehDrvAssignmentServiceClient client = new VehDrvAssignmentServiceClient();
            //pol = client.SetVehDrvAssignments(pol);
            pol = assigner.SetVehDrvAssignments(pol);
            return "";
        }

    }
}
