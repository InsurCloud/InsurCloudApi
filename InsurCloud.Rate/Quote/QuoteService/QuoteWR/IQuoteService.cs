using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using CorPolicy;
namespace QuoteWR
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IQuoteService
    {

        [OperationContract]
        clsPolicyPPA QuotePersonalAuto(clsPolicyPPA pol);

        [OperationContract]
        clsPolicyPPA EnoughToRate(clsPolicyPPA pol);

        [OperationContract]
        clsPolicyPPA ValidRisk(clsPolicyPPA pol);
        
    }


    
}
