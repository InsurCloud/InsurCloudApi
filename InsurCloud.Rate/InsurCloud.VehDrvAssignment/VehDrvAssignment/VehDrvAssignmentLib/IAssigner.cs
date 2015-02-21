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

namespace VehDrvAssignmentLib
{
    public interface IAssigner
    {
        clsEntityDriver DefaultDriver { get; set; }
        DataRow[] Rows { get; set; }
        bool NeedsDefaultDriver { get; set; }
        void Execute(List<string> coverageList, List<DataTable> driverFactorTables, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);

    }
}
