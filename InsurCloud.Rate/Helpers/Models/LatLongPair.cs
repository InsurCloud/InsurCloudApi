using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Models
{
    public class LatLongPair
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Invalid()
        {
            if (Latitude == 0 || Longitude == 0)
            {
                return true;
            }
            return false;
        }
    }
}
