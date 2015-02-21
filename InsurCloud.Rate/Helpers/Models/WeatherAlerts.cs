using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Helpers.Models
{
    public class WeatherAlerts
    {
        public NameValueCollection Alerts { get; set; }
        public NameValueCollection Warnings { get; set; }

        public DateTime ForcaseDate { get; set; }
        public int Tempature { get; set; }
        public int SnowFall { get; set; }
        public int ChanceOfRain { get; set; }        
        public int Humidity { get; set; }

        public void AddAlert(System.Xml.XmlNode xn)
        {
            AddAlert(xn.ChildNodes[0].InnerText, GetMaxWarning(xn).ToString());
        }

        private int GetMaxWarning(XmlNode node)
        {
            int max = 0;
            if (node.ChildNodes[1].InnerText == "")
            {
                return 0;
            }

            max = int.Parse(node.ChildNodes[1].InnerText);
            for (int i = 1; i < node.ChildNodes.Count; i++)
            {
                if (int.Parse(node.ChildNodes[i].InnerText) > max)
                {
                    max = int.Parse(node.ChildNodes[i].InnerText);
                }
            }
            return max;
        }

        public void AddAlert(string name, string value)
        {
            Alerts.Add(name, value);
        }

        public void AddWarning(string name, string value)
        {
            Warnings.Add(name, value);
        }

        public int MildTropicalStormProbability()
        {
            return int.Parse(Alerts.Get("Probability of a Tropical Cyclone Wind Speed above 34 Knots (Cumulative)"));
        }
        public int TropicalStormProbability()
        {
            return int.Parse(Alerts.Get("Probability of a Tropical Cyclone Wind Speed above 50 Knots (Cumulative)"));
        }
        public int ExtremeTropicalStormProbability()
        {
            return int.Parse(Alerts.Get("Probability of a Tropical Cyclone Wind Speed above 64 Knots (Cumulative)"));
        }

        public int TornadoProbability()
        {
            return int.Parse(Alerts.Get("Probability of Tornadoes"));
        }
        public int ExtremeTornadoProbability()
        {
            return int.Parse(Alerts.Get("Probability of Extreme Tornadoes"));
        }

        public int HailProbability()
        {
            return int.Parse(Alerts.Get("Probability of Hail"));
        }
        public int ExtremeHailProbability()
        {
            return int.Parse(Alerts.Get("Probability of Extreme Hail"));
        }

        public int ThunderStormWindsProbability()
        {
            return int.Parse(Alerts.Get("Probability of Damaging Thunderstorm Winds"));
        }
        public int ExtremeThunderStormWindsTornadoProbability()
        {
            return int.Parse(Alerts.Get("Probability of Extreme Thunderstorm Winds"));
        }

        public bool WinterStormWarningExists()
        {
            if (Warnings.Get("WinterStormWarning") != null)
            {
                return true;
            }
            return false;            
        }

        
        public bool TropicalStormsLikely()
        {
            try
            {
                if (MildTropicalStormProbability() > 20 || TropicalStormProbability() > 20 || ExtremeTropicalStormProbability() > 20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            
        }
    }
}
