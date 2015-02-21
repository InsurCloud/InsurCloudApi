using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehDrvAssignmentLib
{
    public class HighToHighByAllCoverageAssigner : HighToHighByAssigner
    {
        public override void Execute(List<string> coverageList, List<System.Data.DataTable> driverFactorTables, CorPolicy.clsPolicyPPA pol, Helpers.StateInfoHelper stateInfo, string connectionString)
        {
            this.HighToHighByCoverage(pol, stateInfo, connectionString, true);
        }
    }
}
