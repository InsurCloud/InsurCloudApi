using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;


namespace RulesLib.PolicySetup
{
    public interface ISetupDataDB
    {
        string Execute(ref clsPolicyPPA pol, string connectionString, Helpers.StateInfoHelper stateInfoHelper = null);
    }
}
