using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.Rules
{
    public interface IPPARule
    {
        bool CheckNEI(CorPolicy.clsPolicyPPA pol, Helpers.StateInfoHelper stateInfo, string connectionString, bool includeSymbol = false);
        void SetupPolicyData(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo);
        bool CheckIER(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        bool CheckUWW(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        bool CheckWRN(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        bool CheckRES(string ruleLevel, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void Finish(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo);
    }
}
