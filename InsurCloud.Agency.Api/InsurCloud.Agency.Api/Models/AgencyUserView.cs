using CoreAgency.Model;
using CoreAgency.Repository;
using CoreAuthentication.Model;
using CoreAuthentication.Repository;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace InsurCloud.Agency.Api.Models
{
    public class AgencyUserView
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string LocationName { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; }
        public IList<string> Roles { get; set; }

        public async Task<AgencyUserView> LoadFromAgencyUser(AgencyUser au, AuthRepository repo)
        {
            UserId = au.UserId;
            ExtendedIdentityUser user = await repo.FindUserByUserId(au.UserId);
            if (user != null)
            {
                FirstName = user.FirstName;
                LastName = user.LastName;
                Email = user.Email;
                PhoneNumber = user.PhoneNumber;
                LocationName = au.Location.Location.Name;
                Status = "Active";
                Roles = await repo.GetUserRolesByUserId(UserId);
                return this;
            }
            else
            {
                return null;
            }
        }
    }

    public class UserRoleView
    {
        public Int64 RoleId { get; set; }
        public string Role { get; set; }
    }
}