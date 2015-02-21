using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using CorPolicy;

namespace VehDrvAssignmentWR
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IVehDrvAssignmentService
    {

        [OperationContract]
        clsPolicyPPA SetVehDrvAssignments(clsPolicyPPA pol);

        [OperationContract]
        clsPolicyPPA SetVehDrvAssignmentsSpecify(clsPolicyPPA pol, string assignmentType);

      
    }


}
