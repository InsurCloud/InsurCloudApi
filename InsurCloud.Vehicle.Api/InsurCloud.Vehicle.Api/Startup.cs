using InsurCloud.Vehicle.Api.App_Start;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace InsurCloud.Vehicle.Api
{
    [assembly: OwinStartup(typeof(InsurCloud.Vehicle.Api.Startup))]
    public class Startup
    {        

        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            ConfigureOAuth(app);

            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);

        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            
        }

    }
}