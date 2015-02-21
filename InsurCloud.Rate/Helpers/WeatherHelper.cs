using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Helpers.gov.weather.www.ndfdXML;
using Helpers.Models;
using System.Xml;
using System.Configuration;

namespace Helpers
{
    public static class WeatherHelper
    {
        
        public static WeatherAlerts CheckWeather(string zipCode, string connectionString){
            string SQL = string.Empty;
            WeatherAlerts wa = new WeatherAlerts();
            SQL = "SELECT LookupResult FROM Common..WeatherLookup (nolock) WHERE ZipCode = @ZipCode AND LastLookupDate > DateAdd(hh, -2, GetDate())";
            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@ZipCode", SqlDbType.VarChar, 50, zipCode));

            connectionString = connectionString.Replace("pgm242", "common");

            string result = DBHelper.GetScalarValue(SQL, "LookupResult", connectionString, parms);

            if (result == string.Empty)
            {
                parms[0].Value = "-1";
                result = DBHelper.GetScalarValue(SQL, "LookupResult", connectionString, parms);

                if (result == string.Empty)
                {
                    try
                    {
                        LatLongPair latLongPair = LookupLatitudeLongitudeInDB(zipCode, connectionString);
                        using (ndfdXMLPortTypeClient weatherSvc = new ndfdXMLPortTypeClient())
                        {
                            weatherParametersType weatherParms = setWeatherParms();
                            string xmlResult;
                            if (latLongPair.Invalid())
                            {
                                xmlResult = weatherSvc.LatLonListZipCode(zipCode);
                                XmlDocument xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(xmlResult);
                                string[] latLongList = xmlDoc.InnerText.Split(',');
                                latLongPair.Latitude = double.Parse(latLongList[0]);
                                latLongPair.Longitude = double.Parse(latLongList[1]);
                                UpdateLatitudeLongitudeInDB(zipCode, latLongPair, connectionString);

                            }

                            xmlResult = weatherSvc.NDFDgen((decimal)latLongPair.Latitude, (decimal)latLongPair.Longitude, "timeseries", DateTime.Now, DateTime.Now.AddDays(5), "e", weatherParms);
                            SQL = "exec Common..UpdateWeatherLookup '" + zipCode + "', '" + xmlResult.Replace("'", "''") + "'";
                            DBHelper.ExecuteNonQuery(SQL, connectionString);
                            wa = loadXML(xmlResult);
                        }
                    }
                    catch
                    {
                        //Do nothing
                    }
                }
                else
                {
                    //WeatherLookup error in past
                }
            }
            else
            {
                wa = loadXML(result);
            }
            return wa;
        }

        private static WeatherAlerts loadXML(string xmlResult)
        {
            WeatherAlerts resp = new WeatherAlerts();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlResult);

            XmlNode xNode = null;

            try
            {
                xNode = xmlDoc.SelectSingleNode("//dwml/head/product/creation-date");
                resp.ForcaseDate = DateTime.Parse(xNode.InnerText);
            }
            catch
            {
                resp.ForcaseDate = DateTime.Now;
            }

            xNode = xmlDoc.SelectSingleNode("//dwml/data/parameters");
            foreach(XmlNode xn in xNode.ChildNodes){
                try
                {
                    switch (xn.Attributes.GetNamedItem("type").Value)
                    {
                        case "apparent": //temp
                            resp.Tempature = Int32.Parse(xn.ChildNodes[1].InnerText);
                            break;
                        case "snow": //inches of snowfall
                            resp.SnowFall = Int32.Parse(xn.ChildNodes[1].InnerText);
                            break;
                        case "12 hour": //chance of rain
                            resp.ChanceOfRain = Int32.Parse(xn.ChildNodes[1].InnerText);
                            break;
                        case "cumulative34":
                        case "cumulative50":
                        case "cumulative64":
                            resp.AddAlert(xn);
                            break;
                        case "relative":
                            resp.Humidity = Int32.Parse(xn.ChildNodes[1].InnerText);
                            break;
                    }
                }
                catch
                {
                    switch (xn.ChildNodes[0].Attributes.GetNamedItem("type").Value)
                    {
                        case "tornados":
                        case "hail": 
                        case "damaging thunderstorm winds":
                        case "extreme tornadoes": //tropical storm
                        case "extreme hail":
                        case "extreme thunderstorm winds":
                            resp.AddAlert(xn.ChildNodes[0].ChildNodes[0].InnerText, xn.ChildNodes[0].ChildNodes[1].InnerText);
                            break;                        
                    }
                }
            }

            XmlNodeList nodes = xmlDoc.SelectNodes("//dwml/data/parameters/hazards/hazard-conditions/hazard");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    try
                    {
                        switch (node.Attributes.GetNamedItem("hazardCode").Value)
                        {
                            case "WS.W": //temp
                                resp.AddWarning("WinterStorm" + node.Attributes.GetNamedItem("significance").InnerText, node.Attributes.GetNamedItem("significance").InnerText);
                                break;
                        }
                    }
                    catch
                    {
                        try
                        {
                            switch (node.Attributes.GetNamedItem("hazardCode").Value)
                            {
                                case "WS.\" ":
                                    resp.AddWarning("WinterStorm", "Warning");
                                    break;
                            }
                        }
                        catch
                        {
                            //Do Nothing;
                        }
                    }
                }
            }
            return resp;
        }
        private static void UpdateLatitudeLongitudeInDB(string zipCode, LatLongPair latLongPair, string connectionString){
            //do nothing
        }
        private static LatLongPair LookupLatitudeLongitudeInDB(string zipCode, string connectionString)
        {
            LatLongPair value = new LatLongPair();
            string SQL = ("SELECT Latitude, Longitude FROM Common..LatitudeLongitudeByZipCode (nolock) WHERE ZipCode = @ZipCode");
            List<SqlParameter> parms = new List<SqlParameter>();            
            parms.Add(DBHelper.AddParm("@ZipCode", SqlDbType.VarChar, 10, zipCode));            
            DataTable result = DBHelper.GetDataTable(SQL, "LatLongByZip", connectionString, parms, "common");
            foreach(DataRow row in result.Rows){
                value.Latitude = (double)row["Latitude"];
                value.Longitude = (double)row["Longitude"];
                break;
            }
            return value;
        }

        private static weatherParametersType setWeatherParms()
        {
            weatherParametersType parms = new weatherParametersType();
            //WeatherParameters.wx = true //weather type coverage intensity
            //WeatherParameters.conhazo = true //Convective hazard outlook
            parms.appt = true; //temperature in Fahrenheit
            parms.rh = true; //relative humidity
            parms.pop12 = true; //Chance of rain in the next 12 hours
            parms.snow = true; // snowfall in inches

            parms.cumw34 = true; //> 34 knot tropical storm
            parms.cumw50 = true; //> 50 knot tropical storm
            parms.cumw64 = true; //> 64 knot tropical storm

            parms.ptornado = true; //Probability of tornadoes
            parms.pxtornado = true; //Probability of extreme tornadoes

            parms.phail = true; //% chance hail
            parms.pxhail = true; //% chance extreme hail

            parms.ptstmwinds = true; //% chance storm winds
            parms.pxtstmwinds = true; //% chance extreme storm winds

            parms.wwa = true;

            return parms;
        }


        public static bool WeatherOverride(CorPolicy.clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string overrideDate = ConfigurationManager.AppSettings["WeatherOverrideDate"].ToString();
            if (overrideDate == string.Empty)
            {
                return WeatherOverrideByCounty(pol, stateInfo, connectionString);
            }
            else
            {
                try
                {
                    if (DateTime.Parse(overrideDate) < DateTime.Now)
                    {
                        return true;
                    }
                    else
                    {
                        return WeatherOverrideByCounty(pol, stateInfo, connectionString);
                    }
                }
                catch
                {
                    return WeatherOverrideByCounty(pol, stateInfo, connectionString);
                }                                
            }
        }
        private static bool WeatherOverrideByCounty(CorPolicy.clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString){
            string value = stateInfo.GetStringValue(pol, "WEATHEROVERRIDE", "COUNTY", pol.PolicyInsured.County.Trim().ToUpper(), connectionString);
            if (value.ToUpper() == "TRUE")
            {
                return true;
            }else{
                return WeatherOverrideByZip(pol, stateInfo, connectionString);
            }
        }

        private static bool WeatherOverrideByZip(CorPolicy.clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string value = stateInfo.GetStringValue(pol, "WEATHEROVERRIDE", "ZIP", pol.PolicyInsured.Zip, connectionString);
            if (value.ToUpper() == "TRUE")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
