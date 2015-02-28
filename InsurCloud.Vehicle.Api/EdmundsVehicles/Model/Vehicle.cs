using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdmundsVehicles.Model
{
    [Table(name: "Vehicles", Schema = "vehicles")]
    public class Vehicle
    {
        [Key]
        [Column(name: "StyleId")]
        public string modelStyleId { get; set; }
        [Column(name: "StyleName")]
        public string modelStyleName { get; set; }
        [Column(name: "Trim")]
        public string trim { get; set; }
        [Column(name: "SubModelBodyType")]
        public string submodelBody { get; set; }
        [Column(name: "SubModelName")]
        public string submodelName { get; set; }
        [Column(name: "SubModelNiceName")]
        public string submodelNiceName { get; set; }
        [Column(name: "ModelYear")]
        public int modelYear { get; set; }
        [Column(name: "MakeId")]
        public string makeId { get; set; }
        [Column(name: "MakeName")]
        public string makeName { get; set; }
        [Column(name: "MakeNiceName")]
        public string makeNiceName { get; set; }
        [Column(name: "ModelId")]
        public string modelId { get; set; }
        [Column(name: "ModelName")]
        public string modelName { get; set; }
        [Column(name: "ModelNiceName")]
        public string modelNiceName { get; set; }



        
    }

    
}
