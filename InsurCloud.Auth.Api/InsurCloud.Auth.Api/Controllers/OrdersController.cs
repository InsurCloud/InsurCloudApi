using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Authentication.Api.Controllers
{
    [RoutePrefix("api/v1/Orders")]
    public class OrdersController : ApiController
    {
        [Authorize]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(Order.CreateOrders());
        }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public string ShipperCity { get; set; }
        public Boolean IsShipped { get; set; }

        public static List<Order> CreateOrders()
        {
            List<Order> OrderList = new List<Order>{
                new Order {OrderID=10248, CustomerName="Matt Price", ShipperCity = "Amman", IsShipped=true},
                new Order {OrderID=10248, CustomerName="Mark Price", ShipperCity = "Dubai", IsShipped=false},
                new Order {OrderID=10248, CustomerName="Mike Price", ShipperCity = "Jeddah", IsShipped=false},
                new Order {OrderID=10248, CustomerName="Tam Price", ShipperCity = "Abu Dhabi", IsShipped=false},
                new Order {OrderID=10248, CustomerName="Tat Price", ShipperCity = "Kuwait", IsShipped=true},
            };

            return OrderList;

        }
    }
}
