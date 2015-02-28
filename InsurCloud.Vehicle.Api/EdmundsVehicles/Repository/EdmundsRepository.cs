using EdmundsVehicles.Context;
using EdmundsVehicles.Model;
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

namespace EdmundsVehicles.Repository
{
    public class EdmundsRepository : IDisposable
    {
        private VehicleContext _ctx;

        public EdmundsRepository()
        {
            _ctx = new VehicleContext();
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }

        public async Task<bool> Reload()
        {
            try
            {
                EdmundMakes makes = await LoadEdmundsData();
                CreateVehicleList(makes);
                return true;
            }
            catch
            {
                return false;
            }
                        
        }


        public async Task<EdmundMakes> LoadEdmundsData()
        {
            EdmundMakes makes = await GetMakes();
            makes = await GetModelStyles(makes);
            return makes;
        }

        private async Task<EdmundMakes> GetMakes()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://insurcloudauthapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                try
                {
                    
                    HttpResponseMessage response = await client.GetAsync("https://api.edmunds.com/api/vehicle/v2/makes?view=basic&fmt=json&api_key=b8mw4mqz8sskr372pu28gh9k");
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsAsync<EdmundMakes>();
                    }
                }
                catch
                {
                    return null;
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
                    Thread.Sleep(200);
                    m.models[i] = await GetModelStyle(m.models[i], url);
                }

            }

            return makes;

        }

        private async Task<Model.Model> GetModelStyle(Model.Model t, string url)
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
                    try
                    {
                        var mod = await response.Content.ReadAsAsync<Model.Model>();
                        return mod;
                    }
                    catch
                    {
                        return null;
                    }

                    //foreach (VehicleModelYear y in t.years)
                    //{
                    //    for(int a = 0; a < y.styles.Count; a++)
                    //    {
                    //        ExtendedModelStyle newStyle = await GetStyleDetails(y.styles[a]);
                    //        styles.Add(newStyle);
                    //    }
                    //}
                }
            }
            return null;
        }

        //public async Task<ExtendedModelStyle> GetStyleDetails(ModelStyle s)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri("https://insurcloudauthapi.azurewebsites.net/");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        string url = "https://api.edmunds.com/api/vehicle/v2/styles/" + s.id + "?view=full&fmt=json&api_key=b8mw4mqz8sskr372pu28gh9k";
        //        // HTTP GET
        //        HttpResponseMessage response = await client.GetAsync(url);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            //JObject test = await response.Content.ReadAsAsync<JObject>();
        //            //ExtendedModelStyle result = await response.Content.ReadAsAsync<ExtendedModelStyle>();
        //            //ExtendedModelStyle result = new ExtendedModelStyle();
        //            //return result;
        //            return await response.Content.ReadAsAsync<ExtendedModelStyle>();

        //        }
        //    }
        //    return null;
        //}

        private void CreateVehicleList(EdmundMakes makes)
        {
            _ctx.Dispose();
            _ctx = new VehicleContext();
            foreach (Make m in makes.makes)
            {
                foreach (Model.Model t in m.models)
                {
                    foreach (VehicleModelYear y in t.years)
                    {
                        foreach (ModelStyle s in y.styles)
                        {
                            var veh = new Vehicle();
                            veh.modelYear = y.year;
                            veh.modelStyleId = s.id + "_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond;
                            veh.modelStyleName = s.name;
                            veh.trim = s.trim;
                            veh.submodelBody = s.submodel.body;
                            veh.submodelName = s.submodel.modelName;
                            veh.submodelNiceName = s.submodel.niceName;
                            veh.makeId = m.id;
                            veh.makeName = m.name;
                            veh.makeNiceName = m.niceName;
                            veh.modelId = t.id;
                            veh.modelName = t.name;
                            veh.modelNiceName = t.niceName;
                            _ctx.Vehicles.AddOrUpdate(veh);
                        }
                        _ctx.SaveChanges();
                    }
                    //_ctx.SaveChanges();
                }
                //_ctx.SaveChanges();
            }            
            _ctx.SaveChanges();
        }
    }
}
