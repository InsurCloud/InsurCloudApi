using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace InsurCloud.Diary.Api.Controllers
{
    
    [RoutePrefix("api/diaryitem")]
    public class DiaryItemController : ApiController
    {
        [Authorize]
        [Route("v1/items")]
        public IHttpActionResult Get()
        {
            var identity = User.Identity as ClaimsIdentity;

            return Ok(identity.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }));
        }

        [AllowAnonymous]
        [Route("v1/test/{id}")]
        public IHttpActionResult Get(string id)
        {
            Message msg = new Message();
            msg.Value = "Hello Tester";
            return Ok(msg);
        }

        
    }

    public class Message
    {
        public string Value = "";
    }


}
