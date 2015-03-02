using CoreCommon.Attributes;
using EdmundsVehicles.Model;
using InsurCloud.Auth.Api.Models;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace InsurCloud.Auth.Api.Controllers
{
    [RequireHttps]
    [RoutePrefix("api/edmunds")]
    public class EdmundsController : ApiController
    {

        private static List<VehicleOption> items = new List<VehicleOption>();

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("v1/vehicles/{query}", Name = "vehicles")]
        public async Task<IHttpActionResult> GetEdmundsVehicleList(string query)
        {
            try
            {
                if (items.Count == 0)
                {
                    LoadItems();
                }

                List<VehicleOption> result = items.OrderBy(u => u.value).Where(c => c.value.StartsWith(query)).Take(100).ToList();

                return Ok(result);

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

        [AllowAnonymous]
        [HttpGet]
        [Route("v1/photo/{styleId}", Name = "getPhoto")]
        public async Task<IHttpActionResult> GetPhoto(string styleId)
        {
            SinglePhoto result = new SinglePhoto();
            result.photoUrl = "img/IC_finalBUG.png";
            if (styleId.Contains('_'))
            {
                styleId = styleId.Split('_')[0];
            }
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://insurcloudauthapi.azurewebsites.net/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "https://api.edmunds.com/v1/api/vehiclephoto/service/findphotosbystyleid?styleId=" + styleId + "&fmt=json&api_key=b8mw4mqz8sskr372pu28gh9k";
                    // HTTP GET
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var mod = await response.Content.ReadAsAsync<List<VehiclePhoto>>();
                            var mod2 = mod.Where(c => c.subType == "exterior" && c.shotTypeAbbreviation == "FQ").ToList();
                            
                            if (mod2.Count > 0 && mod2[0].photoSrcs.Count > 0)
                            {
                                foreach (string src in mod2[0].photoSrcs)
                                {
                                    if (src.EndsWith("4__.jpg") || src.EndsWith("423.jpg") || src.EndsWith("500.jpg"))
                                    {
                                        result.photoUrl = src;
                                        return Ok(result);
                                    }
                                }
                                result.photoUrl = mod2[0].photoSrcs[0];
                                return Ok(result);
                            }
                            return Ok(result);
                        }
                        return Ok(result);
                    }
                    catch
                    {
                        return Ok(result);
                    }
                }
            }
            catch
            {
                return Ok();
            }
        }
    }
}
