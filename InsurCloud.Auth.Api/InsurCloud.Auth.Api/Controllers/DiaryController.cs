using CoreCommon.Attributes;
using InsurCloud.Auth.Api.Models;
using Microsoft.Owin.Security;
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
    [RoutePrefix("api/Diary")]
    public class DiaryController : ApiController
    {


        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }


        [Authorize]
        [HttpGet]
        [Route("v1/notifications", Name = "notifications")]
        public async Task<IHttpActionResult> GetUserNotifications()
        {
            try
            {
                List<DiaryNotifcationView> items = new List<DiaryNotifcationView>();
                items.Add(new DiaryNotifcationView { classType = "comment", diaryItemId = "2q14314", hoursAgo = 0, minutesAgo = 32, Title = "Somebody commented to you about..." });
                items.Add(new DiaryNotifcationView { classType = "envelope", diaryItemId = "2q14514", hoursAgo = 0, minutesAgo = 15, Title = "Message from @gharrison" });
                items.Add(new DiaryNotifcationView { classType = "envelope", diaryItemId = "2q14514", hoursAgo = 0, minutesAgo = 15, Title = "Message from @gharrison" });
                return Ok(items);                
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
