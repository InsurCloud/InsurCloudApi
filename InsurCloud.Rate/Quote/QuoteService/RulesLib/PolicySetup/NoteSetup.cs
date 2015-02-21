using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using CorPolicy;

namespace RulesLib.PolicySetup
{
    public static class NoteSetup
    {
        public static string Execute(ref clsPolicyPPA pol, string connectionString, StateInfoHelper stateInfo)
        {
            NotesHelper.RemoveNotes(pol.Notes, "AAF");
            return "";
        }
        
    }
}
