using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Authentication.Api.Controllers
{
    [RoutePrefix("api/v1/diary")]
    public class DiaryController : ApiController
    {
        [Authorize]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(DiaryEntry.CreateOrders());
        }
    }



    public class DiaryEntry
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public string ShipperCity { get; set; }
        public Boolean IsShipped { get; set; }

        public static List<DiaryEntry> CreateOrders()
        {
            List<DiaryEntry> OrderList = new List<DiaryEntry>{
                new DiaryEntry {OrderID=10248, CustomerName="Matt Price", ShipperCity = "Amman", IsShipped=true},
                new DiaryEntry {OrderID=10248, CustomerName="Mark Price", ShipperCity = "Dubai", IsShipped=false},
                new DiaryEntry {OrderID=10248, CustomerName="Mike Price", ShipperCity = "Jeddah", IsShipped=false},
                new DiaryEntry {OrderID=10248, CustomerName="Tam Price", ShipperCity = "Abu Dhabi", IsShipped=false},
                new DiaryEntry {OrderID=10248, CustomerName="Tat Price", ShipperCity = "Kuwait", IsShipped=true},
            };

            return OrderList;

        }
    }
}
