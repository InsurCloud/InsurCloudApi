using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace USPSAddress
{

    public class CityStateLookupRequest
    {
        public CityStateLookupRequest()
        {
            ZipCodes = new List<ZipCode>();
        }
        [XmlAttribute]
        public string USERID { get; set; }
        [XmlElement("ZipCode")]
        public List<ZipCode> ZipCodes { get; set; }


        public string Serialize()
        {           
            XmlSerializer s = new XmlSerializer(this.GetType());
            StringBuilder sb = new StringBuilder();
            TextWriter w = new StringWriter(sb);
            s.Serialize(w, this);
            w.Flush();
            return sb.ToString();
        }
    }

        

    public class ZipCode
    {
        [XmlAttribute]
        public int ID { get; set; }
        public int Zip5 { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public static ZipCode FromXml(string xml)
        {
            int idx1 = 0;
            int idx2 = 0;
            ZipCode z = new ZipCode();
            if (xml.Contains("<City>"))
            {
                idx1 = xml.IndexOf("<City>") + 6;
                idx2 = xml.IndexOf("</City>");
                z.City = xml.Substring(idx1, idx2 - idx1);
            }
            if (xml.Contains("<State>"))
            {
                idx1 = xml.IndexOf("<State>") + 7;
                idx2 = xml.IndexOf("</State>");
                z.State = xml.Substring(idx1, idx2 - idx1);
            }

            if (xml.Contains("<Zip5>"))
            {
                idx1 = xml.IndexOf("<Zip5>") + 6;
                idx2 = xml.IndexOf("</Zip5>");
                z.Zip5 = int.Parse(xml.Substring( idx1, idx2- idx1));
            }
            return z;

            //if (xml.Contains("<Zip4>"))
            //{
            //    idx1 = xml.IndexOf("<Zip4>") + 6;
            //    idx2 = xml.IndexOf("</Zip4>");
            //    z.Zip5 = xml.Substring(idx1, idx2-idx1);
            //}
        }
    }
}
