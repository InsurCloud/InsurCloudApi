using InsurCloud.Vehicle.Api.Model.EdmundsMake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace InsurCloud.Vehicle.Api.Controller
{
    [RoutePrefix("api/vehicle")]
    public class MakeController : ApiController
    {

        public List<ExtendedModelStyle> styles = new List<ExtendedModelStyle>();

        [AllowAnonymous]
        [HttpGet]
        [Route("v1/edmunds", Name = "updateEdmunds")]
        public async Task<IHttpActionResult> GetEdmunds()
        {
            try
            {
                EdmundMakes makes = await GetMakes();
                makes = await GetModelStyles(makes);
                return Ok(makes);

            }
            catch
            {
                return NotFound();
            }

        }

        private async Task<EdmundMakes> GetMakes()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://insurcloudauthapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("https://api.edmunds.com/api/vehicle/v2/makes?view=basic&fmt=json&api_key=b8mw4mqz8sskr372pu28gh9k");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<EdmundMakes>();
                }
            }
            return null;
        }

        private async Task<EdmundMakes> GetModelStyles(EdmundMakes makes)
        {

            foreach (Make m in makes.makes)
            {
                string urlStub = "https://api.edmunds.com/api/vehicle/v2/" + m.niceName + "/";
                for (int i = 0; i < m.models.Count; i++)
                {
                    string url = urlStub + m.models[i].niceName + "?fmt=json&api_key=b8mw4mqz8sskr372pu28gh9k";
                    try
                    {
                        m.models[i] = await GetModelStyle(m.models[i], url);
                    }
                    catch
                    {
                        return null;
                    }
                    
                }  
            }

            return null;

        }

        private async Task<Model.EdmundsMake.Model> GetModelStyle(Model.EdmundsMake.Model t, string url)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://insurcloudauthapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    t = await response.Content.ReadAsAsync<Model.EdmundsMake.Model>();
                    foreach (VehicleModelYear y in t.years)
                    {
                        for(int a = 0; a < y.styles.Count; a++)
                        {
                            ExtendedModelStyle newStyle = await GetStyleDetails(y.styles[a]);
                            styles.Add(newStyle);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<ExtendedModelStyle> GetStyleDetails(ModelStyle s)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://insurcloudauthapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string url = "https://api.edmunds.com/api/vehicle/v2/styles/" + s.id + "?view=full&fmt=json&api_key=b8mw4mqz8sskr372pu28gh9k";
                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    ExtendedModelStyle result = await response.Content.ReadAsAsync<ExtendedModelStyle>();
                    return result;
                }
            }
            return null;
        }
    }
}
