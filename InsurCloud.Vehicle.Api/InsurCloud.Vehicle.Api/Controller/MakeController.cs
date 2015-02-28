using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Threading;
using InsurCloud.Vehicle.Api.Model;
using EdmundsVehicles.Repository;

namespace InsurCloud.Vehicle.Api.Controller
{
    [RoutePrefix("api/vehicle")]
    public class MakeController : ApiController
    {

        private static List<VehicleOption> items = new List<VehicleOption>();

        [AllowAnonymous]
        [HttpGet]
        [Route("v1/edmunds", Name = "updateEdmunds")]
        public async Task<IHttpActionResult> GetEdmunds()
        {
            try
            {
                if (items.Count == 0)
                {
                    LoadItems();
                }
                return Ok(items);

            }
            catch
            {
                return NotFound();
            }

        }

        private void LoadItems()
        {
            var repo = new EdmundsVehicles.Context.VehicleContext();
            foreach (EdmundsVehicles.Model.Vehicle veh in repo.Vehicles)
            {
                VehicleOption v = new VehicleOption();
                v.id = veh.modelStyleId;
                v.value = String.Concat(veh.modelYear.ToString(), " ", veh.makeName, " ", veh.modelName, " ", veh.modelStyleName);
                items.Add(v);
            }
            

        }
    }
}
