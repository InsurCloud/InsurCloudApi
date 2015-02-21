using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using CorPolicy;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
//using Microsoft.VisualBasic;


namespace VehDrvAssignmentWR
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class VehDrvAssignmentService : IVehDrvAssignmentService
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
            return SetVehDrvAssignments(pol);
        }       

        public clsPolicyPPA SetVehDrvAssignments(clsPolicyPPA pol)
        {
            try
            {
                VehDrvAssignmentLib.Assigner assigner = new VehDrvAssignmentLib.Assigner();
                assigner.SetVehDrvAssignments(pol, _driverAssignmentType);                
            }
            catch(Exception ex)
            {
                throw new ArgumentException("Error in Assigning Drivers to Vehicles (PolicyID " + pol.PolicyID + "): " + ex.Message, ex);
            }
            return pol;
        }

       
    }
}
