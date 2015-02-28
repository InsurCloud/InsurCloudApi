using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmundsVehicles.Model
{
    public class EdmundMakes
    {
        public List<Make> makes { get; set; }
        public int makesCount { get; set; }
    }

    public class Make
    {
        public string id { get; set; }
        public string name { get; set; }
        public string niceName { get; set; }
        public virtual List<Model> models { get; set; }
    }

    public class VehicleMake
    {
        public string id { get; set; }
        public string name { get; set; }
        public string niceName { get; set; }
    }

    public class Model
    {
        public string id { get; set; }
        public string name { get; set; }
        public string niceName { get; set; }
        public List<VehicleModelYear> years { get; set; }
    }

    public class VehicleModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string niceName { get; set; }
    }

    public class VehicleModelYear
    {
        public string id { get; set; }
        public int year { get; set; }
        public List<ModelStyle> styles { get; set; }
    }

    public class ModelStyle
    {
        public string id { get; set; }
        public string name { get; set; }
        public string trim { get; set; }
        public virtual SubModel submodel { get; set; }
    }

    public class SubModel
    {
        public Int64 VehicleModelYearStyleSubModelId { get; set; }
        public string body { get; set; }
        public string fuel { get; set; }
        public string tuner { get; set; }
        public string modelName { get; set; }
        public string niceName { get; set; }
    }

    public class ExtendedModelStyle
    {
        public string id { get; set; }
        public string name { get; set; }
        public VehicleMake make { get; set; }
        public VehicleModel model { get; set; }
        public ModelYear year { get; set; }
        public SubModel submodel { get; set; }
        public string trim { get; set; }
        public VehicleEngine engine { get; set; }
        public VehicleTransmission transmission { get; set; }
        public List<Options> options { get; set; }
        public List<ColorOptions> colors { get; set; }
        public string drivenWheels { get; set; }
        public string numOfDoors { get; set; }
        public List<string> squishVins { get; set; }
        public object MPG { get; set; }
        public string manufacturerCode { get; set; }
        public object price { get; set; }



    }

    public class VehicleMPG
    {
        public int highway { get; set; }
        public int city { get; set; }
    }

    public class VehiclePrice
    {
        public float baseMSRP { get; set; }
        public float baseInvoice { get; set; }
        public float deliveryCharges { get; set; }
        public float tmv { get; set; }
        public float usedTmvRetail { get; set; }
        public float usedPrivateParty { get; set; }
        public float usedTradeIn { get; set; }
        public float estimateTmv { get; set; }
    }

    public class VehicleEngine
    {
        public string id { get; set; }
        public string name { get; set; }
        public float compressionRatio { get; set; }
        public int cylinder { get; set; }
        public float size { get; set; }
        public float displacement { get; set; }
        public string configuration { get; set; }
        public string fuelType { get; set; }
        public int horsepower { get; set; }
        public int torque { get; set; }
        public int totalValves { get; set; }
        public string manufacturerEngineCode { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string compressorType { get; set; }
    }

    public class VehicleTransmission
    {
        public string id { get; set; }
        public string name { get; set; }
        public string automaticType { get; set; }
        public string transmissionType { get; set; }
        public string numberOfSpeeds { get; set; }
    }

    public class Options
    {
        public string category { get; set; }
        public List<Option> options { get; set; }
    }

    public class ColorOptions
    {
        public string category { get; set; }
        public List<ColorOption> options { get; set; }
    }

    public class Option
    {
        public string id { get; set; }
        public string name { get; set; }
        public string equipmentType { get; set; }
        public string manufactureOptionName { get; set; }
        public string manufactureOptionCode { get; set; }
        public string description { get; set; }
        public string category { get; set; }
    }

    public class ColorOption : Option
    {
        public VehicleOptionColorChips colorChips { get; set; }
        public List<FabricType> fabricTypes { get; set; }
    }

    public class FabricType
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class VehicleOptionColorChips
    {
        public ColorChipPrimary primary { get; set; }
        public ColorChipPrimary secondary { get; set; }
    }

    public class ColorChipPrimary
    {
        public string r { get; set; }
        public string g { get; set; }
        public string b { get; set; }
        public string hex { get; set; }
    }

    public class ModelYear
    {
        public string id { get; set; }
        public int year { get; set; }
    }

    public class VehiclePhoto
    {
        public List<string> authorNames { get; set; }
        public string captionTranscript { get; set; }
        public string type { get; set; }
        public string subType { get; set; }
        public string shortTypeAbbreviation { get; set; }
        public List<string> photoSrcs { get; set; }
    }
}
