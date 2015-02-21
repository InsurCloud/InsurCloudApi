using InsurCloud.Auth.Api.Models;
using CoreAuthentication.Model;
using CoreAuthentication.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace InsurCloud.Auth.Api.Controllers
{
    [RoutePrefix("api/v1/users")]
    public class UserManagerController : ApiController
    {
        private AuthRepository _repo = null;

        public UserManagerController()
        {
            _repo = new AuthRepository();
        }

        // GET api/v1/UserManager
        [AllowAnonymous]
        [Route("")]
        public async Task<IHttpActionResult> Get()
        {            

            List<ExtendedIdentityUser> users = await _repo.FindUsers();
            if (users == null)
            {
                return NotFound();
            }
            return Ok(users);
            

        }
    }
}
