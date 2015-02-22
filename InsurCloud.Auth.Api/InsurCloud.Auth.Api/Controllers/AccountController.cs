using CoreAuthentication.Helpers;
using CoreAuthentication.Model;
using CoreAuthentication.Repository;
using CoreAuthentication.Services;
using InsurCloud.Auth.Api.Attributes;
using InsurCloud.Auth.Api.Models;
using InsurCloud.Auth.Api.Results;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace InsurCloud.Auth.Api.Controllers
{    
    [RequireHttps]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private AuthRepository _repo = null;

        public AccountController()
        {
            _repo = new AuthRepository();
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        
        [Authorize]
        [HttpGet]
        [Route("v1/users", Name = "users")]
        public async Task<IHttpActionResult> GetUsers()
        {
            try
            {
                List<UserViewModel> rUsers = new List<UserViewModel>();

                List<ExtendedIdentityUser> users = await _repo.FindUsers();
                foreach (ExtendedIdentityUser user in users)
                {
                    UserViewModel rUser = new UserViewModel(user);
                    rUsers.Add(rUser);
                }

                return Ok(rUsers);
            }
            catch
            {
                return NotFound();
            }

        }

        [Authorize]
        [Route("v1/user", Name = "user")]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                ExtendedIdentityUser user = await _repo.FindCurrentUser(User.Identity.GetUserName());
                UserViewModel rUser = new UserViewModel(user);
                return Ok(rUser);
            }
            catch
            {
                return NotFound();
            }

        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        //[VersionedRoute("externallogin", 1)]
        [Route("v1/externallogin", Name = "externallogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            string redirectUri = string.Empty;

            if (error != null)
            {
                return BadRequest(Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            var redirectUriValidationResult = ValidateClientAndRedirectUri(this.Request, ref redirectUri);

            if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
            {
                return BadRequest(redirectUriValidationResult);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ExtendedIdentityUser user = await _repo.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}&external_email=={5}",
                                            redirectUri,
                                            externalLogin.ExternalAccessToken,
                                            externalLogin.LoginProvider,
                                            hasRegistered.ToString(),
                                            externalLogin.UserName, 
                                            externalLogin.EmailAddress);

            return Redirect(redirectUri);

        }

        // POST api/Account/Register
        [AllowAnonymous]
        //[VersionedRoute("register", 1)]
        [Route("v1/register")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!userModel.AcceptTerms)
            {
                ModelState.AddModelError("AcceptTerms", new ArgumentException("You must accept the Terms & Conditions to sign-up!"));
                return BadRequest(ModelState);
            }

            IdentityResult result = null;
            try
            {
                result = await _repo.RegisterUser(userModel);
            }
            catch
            {
                //Do Nothing
            }
            
            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            SendEmailInfo emailInfo = new SendEmailInfo();
            emailInfo.EmailType = EmailType.NewAccountCreated;
            emailInfo.EmailAddress = userModel.EmailAddress;
            emailInfo.HostName = ConfigurationManager.AppSettings["HostName"];

            AccountEmailService svc = new AccountEmailService();
            if (await svc.SendEmail(emailInfo))
            {
                return Ok();
            }
            else
            {
                //TODO: Add Logging of failed events
            }

            return Ok();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        //[VersionedRoute("register", 1)]
        [Route("v1/forgot")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ExtendedIdentityUser result = null;
            try
            {
                result = await _repo.FindCurrentUser(userModel.EmailAddress);   
            }
            catch
            {
                //Do Nothing
                return BadRequest("Unable to locate user");
            }
            if (result == null)
            {
                return BadRequest("User not found");
            }

            RefreshToken token = new RefreshToken();
            try
            {
                token.Id = Guid.NewGuid().ToString();
                token.Subject = userModel.EmailAddress;
                token.ClientId = userModel.ClientId;
                token.IssuedUtc = DateTime.UtcNow;
                token.ExpiresUtc = DateTime.UtcNow.AddMinutes(20);
                token.ProtectedTicket = Helper.GetHash(string.Concat(token.Id, "|", token.Subject, "|", token.ClientId, "|", token.IssuedUtc.ToString(), "|", token.ExpiresUtc.ToString()));

                var tokenResult = await _repo.AddRefreshToken(token);

                if (!tokenResult)
                {
                    return BadRequest("Unable to create unique email");
                }
            }
            catch
            {
                return BadRequest("Unable to create unique email");
            }
            
            SendEmailInfo emailInfo = new SendEmailInfo();
            emailInfo.EmailAddress = token.Subject;
            emailInfo.Token = token.Id;
            emailInfo.EmailType = EmailType.ForgotPasswordToken;
            emailInfo.HostName = ConfigurationManager.AppSettings["HostName"];

            AccountEmailService svc = new AccountEmailService();
            if (await svc.SendEmail(emailInfo))
            {
                return Ok();
            }
            return BadRequest("Unable to send email");
            
            
        }

        // POST api/Account/Register
        [AllowAnonymous]
        //[VersionedRoute("register", 1)]
        [Route("v1/reset")]
        public async Task<IHttpActionResult> ResetPassword(ResetModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (userModel.Password != userModel.ConfirmPassword)
            {
                return BadRequest("Password's do not match");
            }

            RefreshToken token = null;
            try
            {
                token = await _repo.FindRefreshToken(userModel.Token);                
            }
            catch
            {
                //Do Nothing
                return BadRequest("Invalid Reset Token");
            }
            if (token == null || token.ExpiresUtc < DateTime.UtcNow)
            {
                return BadRequest("Invalid Reset Token");
            }


            IdentityResult result = null;
            try
            {
                result = await _repo.ResetPassword(userModel, token);
            }
            catch
            {
                //Do Nothing
            }

            if (result.Errors.Count() != 0)
            {
                return BadRequest("Unable to reset password");
            }
            
            try
            {
                SendEmailInfo emailInfo = new SendEmailInfo();
                emailInfo.EmailAddress = token.Subject;
                emailInfo.Token = token.ProtectedTicket;
                emailInfo.EmailType = EmailType.PasswordChanged;

                AccountEmailService svc = new AccountEmailService();
                await svc.SendEmail(emailInfo);
            }
            catch
            {
                //TODO: Add logging/alert
            }

            return Ok();

        }


        // POST api/Account/RegisterExternal
        [AllowAnonymous]
        //[VersionedRoute("registerexternal", 1)]
        [Route("v1/registerexternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!model.TermsAccepted)
            {
                return BadRequest("You must accept the terms to register.");
            }

            var verifiedAccessToken = await VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            ExtendedIdentityUser user = await _repo.FindAsync(new UserLoginInfo(model.Provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                return BadRequest("External user is already registered");
            }

            IdentityResult result;
            user = new ExtendedIdentityUser() { UserName = model.UserName, Email = model.UserName, FirstName = model.FirstName, LastName = model.LastName };
            try
            {
                 result = await _repo.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
            }
            catch
            {
                return InternalServerError();
            }
            

            var info = new ExternalLoginInfo()
            {
                DefaultUserName = model.UserName,
                Login = new UserLoginInfo(model.Provider, verifiedAccessToken.user_id)
            };

            result = await _repo.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            //generate access token response
            var accessTokenResponse = GenerateLocalAccessTokenResponse(model.UserName);

            return Ok(accessTokenResponse);
        }

        // GET api/Account/obtainlocalaccesstoken
        [AllowAnonymous]
        [HttpGet]
        //[VersionedRoute("obtainlocalaccesstoken", 1)]
        [Route("v1/obtainlocalaccesstoken")]
        public async Task<IHttpActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken)
        {

            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                return BadRequest("Provider or external access token is not sent");
            }

            var verifiedAccessToken = await VerifyExternalAccessToken(provider, externalAccessToken);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            ExtendedIdentityUser user = await _repo.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            if (!hasRegistered)
            {
                return BadRequest("External user is not registered");
            }

            //generate access token response
            var accessTokenResponse = GenerateLocalAccessTokenResponse(user.UserName);

            return Ok(accessTokenResponse);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        private string ValidateClientAndRedirectUri(HttpRequestMessage request, ref string redirectUriOutput)
        {

            Uri redirectUri;

            var redirectUriString = GetQueryString(Request, "redirect_uri");

            if (string.IsNullOrWhiteSpace(redirectUriString))
            {
                return "redirect_uri is required";
            }

            bool validUri = Uri.TryCreate(redirectUriString, UriKind.Absolute, out redirectUri);

            if (!validUri)
            {
                return "redirect_uri is invalid";
            }

            var clientId = GetQueryString(Request, "client_id");

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return "client_Id is required";
            }

            var client = _repo.FindClient(clientId);

            if (client == null)
            {
                return string.Format("Client_id '{0}' is not registered in the system.", clientId);
            }

            if (!string.Equals(client.AllowedOrigin, redirectUri.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("The given URL is not allowed by Client_id '{0}' configuration.", clientId);
            }

            redirectUriOutput = redirectUri.AbsoluteUri;

            return string.Empty;

        }

        private string GetQueryString(HttpRequestMessage request, string key)
        {
            var queryStrings = request.GetQueryNameValuePairs();

            if (queryStrings == null) return null;

            var match = queryStrings.FirstOrDefault(keyValue => string.Compare(keyValue.Key, key, true) == 0);

            if (string.IsNullOrEmpty(match.Value)) return null;

            return match.Value;
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            var verifyTokenEndPoint = "";

            if (provider == "Facebook")
            {
                //You can get it from here: https://developers.facebook.com/tools/accesstoken/
                //More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook

                var appToken = "1638934106334873|0ia8RyFtsTIFwI09kLdlCCzebTg";
                verifyTokenEndPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", accessToken, appToken);
            }
            else if (provider == "Google")
            {
                verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}", accessToken);
            }
            else
            {
                return null;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint);
            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.user_id = jObj["data"]["user_id"];
                    parsedToken.app_id = jObj["data"]["app_id"];

                    if (!string.Equals(Startup.facebookAuthOptions.AppId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (provider == "Google")
                {
                    parsedToken.user_id = jObj["user_id"];
                    parsedToken.app_id = jObj["audience"];

                    if (!string.Equals(Startup.googleAuthOptions.ClientId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                }

            }

            return parsedToken;
        }

        private JObject GenerateLocalAccessTokenResponse(string userName)
        {

            var tokenExpiration = TimeSpan.FromDays(1);

            ClaimsIdentity identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);

            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            identity.AddClaim(new Claim("role", "user"));

            var props = new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };

            var ticket = new AuthenticationTicket(identity, props);

            string accessToken = "";
            try
            {
                accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
            }
            catch
            {
                //TODO: log failures
            }

            
            JObject tokenResponse = new JObject(
                                        new JProperty("userName", userName),
                                        new JProperty("access_token", accessToken),
                                        new JProperty("token_type", "bearer"),
                                        new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
                                        new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
                                        new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
        );

            return tokenResponse;
        }

        

    }
}