using CoreAgency.Model;
using CoreAgency.Repository;
using CoreAuthentication.Model;
using CoreAuthentication.Repository;
using CoreCommon.Attributes;
using InsurCloud.Auth.Api.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace InsurCloud.Auth.Api.Controllers
{
    [RequireHttps]
    [RoutePrefix("api/Agency")]
    public class AgencyController : ApiController
    {
        private AgencyRepository _agencyRepo = null;
        private AuthRepository _authRepo = null;

        public AgencyController()
        {
            _agencyRepo = new AgencyRepository();
            _authRepo = new AuthRepository();
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        [Authorize]
        [HttpGet]
        [Route("v1/users", Name = "agencyusers")]
        public async Task<IHttpActionResult> GetUsers()
        {
            try
            {
                ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());                
                if (user != null)
                {
                    CoreAgency.Model.Agency agency = await _agencyRepo.GetAgencyFromUser(user.Id);
                    if (agency != null)
                    {
                        List<AgencyUser> agencyUsers = await _agencyRepo.GetAgencyUsers(agency);
                        if (agencyUsers != null)
                        {
                            return Ok(await LoadAgencyUserView(agencyUsers));
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    else
                    {
                        return BadRequest("Unable to locate current user's agency");
                    }

                }
                else
                {
                    return BadRequest("Can not identify user");
                }
            }
            catch
            {
                return InternalServerError();
            }
        }

        private async Task<List<AgencyUserView>> LoadAgencyUserView(List<AgencyUser> agencyUsers)
        {
            List<AgencyUserView> view = new List<AgencyUserView>();

            foreach (AgencyUser a in agencyUsers)
            {
                AgencyUserView uv = new AgencyUserView();
                uv = await uv.LoadFromAgencyUser(a, _authRepo);
                if (uv != null)
                {
                    view.Add(uv);
                }
            }
            return view;
        }
    }
}
