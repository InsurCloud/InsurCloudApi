using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace USPSAddress
{
    public class CityStateLookup
    {

        public async Task<ZipCode> LookupCityStateByZipCode(int zipCode)
        {
            CityStateLookupRequest request = new CityStateLookupRequest();
            request.USERID = "529INSUR1742";
            request.ZipCodes.Add(new ZipCode {ID=0, Zip5=zipCode});

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://insurcloudauthapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                // HTTP POST
                try
                {
                    var content = new FormUrlEncodedContent(new[] 
                    {
                        new KeyValuePair<string, string>("API", "CityStateLookup"), 
                        new KeyValuePair<string, string>("XML", request.Serialize())
                    });

                    HttpResponseMessage response = await client.PostAsync("http://production.shippingapis.com/ShippingAPI.dll", content);
                    if (response.IsSuccessStatusCode)
                    {
                        //Error in line 1 position 26. Expecting element 'CityStateLookupResponse' from namespace 'http://schemas.datacontract.org/2004/07/USPSAddress'.. Encountered 'Element'  with name 'CityStateLookupResponse', namespace ''. 

                        string respXml = await response.Content.ReadAsStringAsync();
                        ZipCode resp = ZipCode.FromXml(respXml);
                        return resp;
                        
                    }
                }
                catch
                {
                    return null;
                }

            }
            return null;

        }

    }
}
