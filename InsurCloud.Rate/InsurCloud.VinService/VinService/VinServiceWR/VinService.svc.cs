using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using VINServiceLib;

namespace VinServiceWR
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class VinService : IVinService
    {        
        protected static VINService svc;

        public DataSet BridgeVINData(string vin)
        {
            
            return svc.BridgeVINData(vin);

        }
       
        
        public string VerifyCheckDigit(string vin)
        {
            return svc.VerifyCheckDigit(vin);
        }
       
    }

}
