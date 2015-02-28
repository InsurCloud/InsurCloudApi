using EdmundsVehicles.Model;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmundsVehicles.Context
{
    public class VehicleContext : DbContext
    {
        public VehicleContext()
            : base("VehicleContext")
        {

        }

        public DbSet<Vehicle> Vehicles { get; set; }
    }
}
