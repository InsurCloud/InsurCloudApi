using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace InsurCloud.Vehicle.Api.Model
{
    [Table(name: "VehicleMakes", Schema = "vins")]
    public class VehicleMake
    {
        [Key]
        public Int64 VehicleMakeId { get; set; }
        public Int64 EdmundsId { get; set; }
        public string DisplayName { get; set; }
        public string NiceName { get; set; }
        


    }

    [Table(name: "VehicleModels", Schema = "vins")]
    public class VehicleModel
    {
        [Key]
        public Int64 VehicleModelId { get; set; }
        public Int64 EdmundsId { get; set; }
        public string DisplayName { get; set; }
        public string NiceName { get; set; }
    }

    [Table(name: "VehicleModelYears", Schema = "vins")]
    public class VehicleModelYear
    {
        [Key]
        public Int64 VehicleModelId { get; set; }
        public Int64 EdmundsId { get; set; }
        public int Year { get; set; }
    }
}