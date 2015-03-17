using CoreCommon.Attributes;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using USPSAddress;

namespace InsurCloud.Auth.Api.Controllers
{
    [RequireHttps]
    [RoutePrefix("api/Address")]
    public class AddressController : ApiController
    {
        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        [Authorize]
        [HttpGet]
        [Route("v1/citystatebyzip/{zipCode}", Name = "CityStateByZip")]
        public async Task<IHttpActionResult> GetCityStateByZip(int zipCode)
        {
            if (zipCode < 10000)
            {
                return Ok();
            }

            
            CityStateLookup lookup = new CityStateLookup();
            ZipCode resp = await lookup.LookupCityStateByZipCode(zipCode);
            if (resp != null)
            {
                return Ok(resp);
            }
            return InternalServerError();
        }
    }
}
