using CoreAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InsurCloud.Auth.Api.Models
{
    public class UserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastInitial { get; set; }
        public UserType UserType { get; set; }
        public string EmailAddress { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockedOut { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime LastLogin { get; set; }
        public string ImageURL { get; set; }
        public bool HasProfileImage { get; set; }

        public UserViewModel(ExtendedIdentityUser user)
        {
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.LockedOut = user.LockoutEnabled;
            this.LastInitial = user.LastName.Substring(0, 1) + ".";
            //rUser.LastLogin = user.
            this.TwoFactorEnabled = user.TwoFactorEnabled;
            this.UserType = user.UserType;
            this.EmailConfirmed = user.EmailConfirmed;
            this.EmailAddress = user.Email;
            this.ImageURL = user.ProfileImageURL;
            this.HasProfileImage = !string.IsNullOrEmpty(this.ImageURL);
        }

    }   
}