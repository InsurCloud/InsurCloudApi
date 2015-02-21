using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace VINServiceLib
{
    public class VINService
    {
        protected static DataTable _vinTable;
        protected static bool _loadingVinTable;

        protected DataTable VINTable {
            get 
            {
                if (_vinTable == null)
                {
                    _vinTable = GetVinTable();
                    _loadingVinTable = false;
                }
                return _vinTable;
            }            
        }

        private DataTable GetVinTable()
        {
            if (_loadingVinTable == true)
            {
                return GetVinTable();
            }
            else
            {
                _loadingVinTable = true;
            }
            
            try
            {
                using (SqlConnection conn = new SqlConnection("Server=tcp:emuxtovazm.database.windows.net,1433;Database=common;User ID=AppUser@emuxtovazm;Password=AppU$er!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"))
                {
                    string SQL = " SELECT a.VINCode, a.VINDesc, a.ModelYear, a.ISOIdentification, a.StateException, RTrim(a.VehicleMakeCode) as VehicleMakeCode, RTrim(a.VehicleModelCode) as VehicleModelCode, ";
                    SQL += "   a.VehicleRestraintTypeCode, a.CountryVehiclePerformanceCode, a.NonVSRVehiclePerformanceCode, a.VehicleCylinderCode, ";
                    SQL += "   a.VehicleBodyStyleCode, a.VehicleAntiTheftCode, a.VehicleABSCode, a.VehicleClassCode, a.VehicleDaytimeLightCode, ";
                    SQL += "   a.VehicleEngineTypeCode, a.SpecialProcessingYN, b.NonVSRVehicleSymbolCode, b.VSRVehicleSymbolCode, b.LiabilitySymbolCode, ";
                    SQL += "   b.PIPMedPaySymbolCode, b.CompSymbolCode, b.CollSymbolCode, b.VINSymbolEffDate, a.AddedDateT ";
                    SQL += " FROM Common..VIN a with (nolock), Common..VINSymbol b with (nolock) ";
                    SQL += " WHERE a.VINCode = b.VINCode";
                    SQL += " and b.VINSymbolEffDate >= (SELECT MAX(VINSymbolEffDate) FROM Common..VINSymbol with (nolock) where VINCode = a.VINCode)";


                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL, conn);                    
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataTable dt = new DataTable();
                    da.Fill(dt);                    
                    return dt;
                }
            }
            catch
            {
                return null;
            }           
        }

        public DataSet BridgeVINData(string vin)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string partialVIN = String.Empty;
            int vehModelYear;

            if (vin == "NONOWNER")
            {
                partialVIN = vin;
                vehModelYear = 1;
            }
            else
            {
                partialVIN = String.Concat(vin.Substring(0, 8), "&", vin.Substring(9, 1));
                vehModelYear = GetNonRegVINModelYear(String.Concat(vin.Substring(0, 3), "&", vin.Substring(4, 4), "&", vin.Substring(9, 1)));
                if (vehModelYear == 0)
                    vehModelYear = GetVehicleModelYear(vin.Substring(6, 1), vin.Substring(9, 1));
            }
            dt = LoadVehicleInformation(vehModelYear, partialVIN);

            if (dt.Rows.Count == 0)
            {
                partialVIN = partialVIN.Substring(0, 9) + GetVINModelYear(vehModelYear - 1);
                dt = LoadVehicleInformation(vehModelYear - 1, partialVIN);
                if (dt.Rows.Count > 0)
                {
                    dt.Rows[0]["ModelYear"] = vehModelYear;
                    dt.Rows[0].AcceptChanges();
                }
            }
            
            ds.Tables.Add(dt);
            return ds;

        }
       
        private string GetVINModelYear(int vehModelYear)
        {
            string tenthChar = "";

            switch(vehModelYear){
                case 1981:
                    tenthChar = "B";
                    break;
                case 1982:
                    tenthChar = "C";
                    break;
                case 1983:
                    tenthChar = "D";
                    break;
                case 1984:
                    tenthChar = "E";
                    break;
                case 1985:
                    tenthChar = "F";
                    break;
                case 1986:
                    tenthChar = "G";
                    break;
                case 1987:
                    tenthChar = "H";
                    break;
                case 1988:
                    tenthChar = "J";
                    break;
                case 1989:
                    tenthChar = "K";
                    break;
                case 1990:
                    tenthChar = "L";
                    break;
                case 1991:
                    tenthChar = "M";
                    break;
                case 1992:
                    tenthChar = "N";
                    break;
                case 1993:
                    tenthChar = "P";
                    break;
                case 1994:
                    tenthChar = "R";
                    break;
                case 1995:
                    tenthChar = "S";
                    break;
                case 1996:
                    tenthChar = "T";
                    break;
                case 1997:
                    tenthChar = "V";
                    break;
                case 1998:
                    tenthChar = "W";
                    break;
                case 1999:
                    tenthChar = "X";
                    break;
                case 2000:
                    tenthChar = "Y";
                    break;
                case 2010:
                    tenthChar = "A";
                    break;
                case 2011:
                    tenthChar = "B";
                    break;
                case 2012:
                    tenthChar = "C";
                    break;
                case 2013:
                    tenthChar = "D";
                    break;
                case 2014:
                    tenthChar = "E";
                    break;
                case 2015:
                    tenthChar = "F";
                    break;
                case 2016:
                    tenthChar = "G";
                    break;
                case 2017:
                    tenthChar = "H";
                    break;
                case 2018:
                    tenthChar = "J";
                    break;
                case 2019:
                    tenthChar = "K";
                    break;
                case 2020:
                    tenthChar = "L";
                    break;
                case 2021:
                    tenthChar = "M";
                    break;
                case 2022:
                    tenthChar = "N";
                    break;
                case 2023:
                    tenthChar = "P";
                    break;
                case 2024:
                    tenthChar = "R";
                    break;
                case 2025:
                    tenthChar = "S";
                    break;
                case 2026:
                    tenthChar = "T";
                    break;
                case 2027:
                    tenthChar = "V";
                    break;
                case 2028:
                    tenthChar = "W";
                    break;
                case 2029:
                    tenthChar = "X";
                    break;
                case 2030:
                    tenthChar = "Y";
                    break;
                case 2031:
                    tenthChar = "1";
                    break;
                case 2032:
                    tenthChar = "2";
                    break;
                case 2033:
                    tenthChar = "3";
                    break;
                case 2034:
                    tenthChar = "4";
                    break;
                case 2035:
                    tenthChar = "5";
                    break;
                case 2036:
                    tenthChar = "6";
                    break;
                case 2037:
                    tenthChar = "7";
                    break;
                case 2038:
                    tenthChar = "8";
                    break;
                case 2039:
                    tenthChar = "9";
                    break;
            }

            return tenthChar;

        }

        private DataTable LoadVehicleInformation(int vehModelYear, string partialVIN)
        {
            DataTable dt = new DataTable();
            partialVIN = PartialVINLookup(vehModelYear, partialVIN);
            dt = VINTable.Clone();
            DataRow[] rows = VINTable.Select("VINCode = '" + partialVIN + "'");
            foreach (DataRow row in rows)
            {
                dt.ImportRow(row);
            }
            return dt;

        }

        private string PartialVINLookup(int vehModelYear, string partialVIN)
        {
            if(partialVIN.Length == 10){
                switch(vehModelYear){
                    case 1981:
                    case 1982:
                    case 1983:
                    
                        switch(partialVIN.Substring(0, 3)){
                            case "JN1":
                            case "JN6":
                                partialVIN = partialVIN.Substring(0, 5) + "&" + partialVIN.Substring(partialVIN.Length-4, 4);
                                break;
                        }
                        break;
                    case 1984:
                        switch(partialVIN.Substring(0, 3)){
                            case "JN1":
                            case "JN6":
                            case "1N6":
                                partialVIN = partialVIN.Substring(0, 5) + "&" + partialVIN.Substring(partialVIN.Length - 4, 4);
                                break;
                        }
                        break;  
                    case 1985:
                        switch(partialVIN.Substring(0, 3)){
                            case "JN1":
                            case "1N4":
                            case "JN6":
                            case "1N6":
                                partialVIN = partialVIN.Substring(0, 5) + "&" + partialVIN.Substring(partialVIN.Length - 4, 4);
                                break;
                        }
                        break;
                    case 1986:
                    case 1987:
                    case 1988:
                    case 1989:
                        switch(partialVIN.Substring(0, 3)){
                            case "JN1":
                            case "JN4":
                            case "1N4":
                            case "JN8":
                            case "1N8":
                                partialVIN = partialVIN.Substring(0, 5) + "&" + partialVIN.Substring(partialVIN.Length - 4, 4);
                                break;
                        }
                        break;
                    case 1990:
                    case 1991:
                    case 1992:
                    case 1993:
                    case 1994:
                        if((partialVIN.Substring(0, 3) == "1FT" || partialVIN.Substring(0, 3) == "2FT") && vehModelYear == 1994 &&
                                (partialVIN.Substring(4, 3) == "F25" || partialVIN.Substring(4, 3) == "F26" || partialVIN.Substring(4, 3) == "X25" ||
                                 partialVIN.Substring(4, 3) == "X26" || partialVIN.Substring(4, 3) == "W25" || partialVIN.Substring(4, 3) == "W26")){
                            //Do Nothing
                        }else if((partialVIN.Substring(0, 3) == "1GC" || partialVIN.Substring(0, 3) == "2GC" || 
                                    partialVIN.Substring(0, 3) == "1GN" || partialVIN.Substring(0, 3) == "2GN") && vehModelYear == 1994 &&
                                (partialVIN.Substring(4, 3) == "G15" || partialVIN.Substring(4, 3) == "G25" || partialVIN.Substring(4, 3) == "G35" ||
                                 partialVIN.Substring(4, 3) == "G39")){
                            //Do Nothing
                        }else{
                            switch(partialVIN.Substring(0, 3)){
                                case "JGC": 
                                case "1GC": 
                                case "1GN":
                                case "2CC":
                                case "2GC":
                                case "2GN":
                                case "2CN":
                                case "1C4":
                                case "JB4":
                                case "JB7":
                                case "1B4":
                                case "1B5":
                                case "1B7":
                                case "2B4":
                                case "2B5":
                                case "2B7":
                                case "3B4":
                                case "3B7":
                                case "1FB":
                                case "1FM":
                                case "1FT":
                                case "2FT":
                                case "1GD":
                                case "1GJ":
                                case "1GK":
                                case "1GT":
                                case "2GD":
                                case "2GJ":
                                case "2GK":
                                case "2GT":
                                case "JAA":
                                case "JAC":
                                case "LES":
                                case "LM5":
                                case "4S1":
                                case "4S2":
                                case "1J4":
                                case "1J7":
                                case "2J4":
                                case "2J7":
                                case "4F4":
                                case "4M2":
                                case "4M4":
                                case "JA4":
                                case "JA7":
                                case "1GH":
                                case "JP4":
                                case "1P4":
                                case "2P4":
                                case "1GM":
                                case "2GM":
                                    partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    break;
                                case "4F2":
                                    if(vehModelYear == 1994){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "137":
                                    partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    break;
                            }
                        }
                        break;
                    default: // >= 1995
                        if((partialVIN.Substring(0, 3) == "1FT" || partialVIN.Substring(0, 3) == "2FT") &&
                                (vehModelYear == 1995 || vehModelYear == 1996) &&
                                (partialVIN.Substring(4, 3) == "F25" || partialVIN.Substring(4, 3) == "F26" ||
                                    partialVIN.Substring(4, 3) == "X25" || partialVIN.Substring(4, 3) == "X26" ||
                                    partialVIN.Substring(4, 3) == "W25" || partialVIN.Substring(4, 3) == "W26")){
                            //Do Nothing
                        }else if((partialVIN.Substring(0, 3) == "1GC" || partialVIN.Substring(0, 3) == "2GC" ||
                                    partialVIN.Substring(0, 3) == "1GN" || partialVIN.Substring(0, 3) == "2GN") &&
                                (vehModelYear == 1995) &&
                                (partialVIN.Substring(4, 3) == "G15" || partialVIN.Substring(4, 3) == "G25" ||
                                    partialVIN.Substring(4, 3) == "G35" || partialVIN.Substring(4, 3) == "G39")){
                            //Do Nothing
                        }else if(partialVIN.Substring(0, 3) == "3FT" && vehModelYear == 1997){
                            partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                        }else if((partialVIN.Substring(0, 3) == "1B3" || partialVIN.Substring(0, 3) == "3B3") && 
                                (vehModelYear == 2002) &&
                                (partialVIN.Substring(4, 2) == "A1" || partialVIN.Substring(4, 2) == "U1")){
                            partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                        }else{
                            switch(partialVIN.Substring(0, 3)){
                                case "5GA": // BUIK (Buick)
                                    if(vehModelYear >= 2004 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "KL4":
                                    if(vehModelYear == 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GY": // ' CADI (Cadillac)
                                    if(vehModelYear >= 1999 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3GY": // ' CADI (Cadillac)
                                    if(vehModelYear >= 2002 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GC":
                                case "2GC": // ' CHEV (Chevrolet)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }else if(vehModelYear >= 2010 && vehModelYear <= 2012){
                                        switch(partialVIN.Substring(4, 2)){
                                            case "GA":
                                            case "GF":
                                            case "GG":
                                            case "GT":
                                            case "GU":
                                            case "HA":
                                                partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                                break;
                                        }
                                    }
                                    break;
                                case "3GC": // ' CHEV (Chevrolet)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }else if(vehModelYear == 2010){
                                        switch(partialVIN.Substring(4, 2)){
                                            case "AA":
                                            case "AB":
                                            case "AC":
                                            case "AE":
                                            case "AF":
                                            case "AG":
                                                partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                                break;
                                        }
                                    }else if(vehModelYear == 2011){
                                        switch(partialVIN.Substring(4, 2)){
                                            case "AA":                                      
                                            case "AE":
                                                partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                                break;
                                        }
                                    }
                                    break;                            
                                case "1GA":
                                case "2GA": // ' CHEV (Chevrolet)
                                    if(vehModelYear >= 1999 && vehModelYear <= 2012){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2CN": // ' CHEV (Chevrolet)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2012){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;                        
                                case "1GN":
                                case "2GN":
                                case "3GN": // ' CHEV (Chevrolet)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;          
                                case "1A4": // ' CHRY (Chrysler)
                                    if(vehModelYear >= 2006 && vehModelYear <= 2008){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;                       
                                case "1A8":
                                case "2A8":
                                case "3A8": // ' CHRY (Chrysler) 
                                    if(vehModelYear >= 2006 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;                     
                                case "2A4": //' CHRY (Chrysler) 
                                    if((vehModelYear >= 2006 && vehModelYear <= 2008) || 
                                        (vehModelYear >= 2010 && vehModelYear <= 2011)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;                       
                                case "3A4": // ' CHRY (Chrysler) 
                                    if((vehModelYear >= 2006 && vehModelYear <= 2007) || 
                                        (vehModelYear == 2010)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2C4": // ' CHRY (Chrysler), RAM
                                    if((vehModelYear >= 2000 && vehModelYear <= 2005) || 
                                        (vehModelYear >= 2012 && vehModelYear <= 2013)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1D3":
                                case "3D3": // ' DODG (Dodge)
                                    if(vehModelYear >= 2002 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2D3": // ' DODG (Dodge)
                                    if(vehModelYear >= 2008 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1D8":
                                case "2D7":
                                case "2D8": // ' DODG (Dodge)
                                    if(vehModelYear >= 2003 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1D4":
                                case "1D7":
                                case "3D7": // ' DODG (Dodge)
                                    if(vehModelYear >= 2002 && vehModelYear <= 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2D4": // ' DODG (Dodge)
                                    if(vehModelYear >= 2003 && vehModelYear <= 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3D8": //  ' DODG (Dodge)
                                    if(vehModelYear == 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3D4": // ' DODG (Dodge)
                                    if(vehModelYear >= 2009 && vehModelYear <= 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;                          
                                case "3C4":
                                    if((vehModelYear >= 2001 && vehModelYear <= 2005) || 
                                        (vehModelYear >= 2012 && vehModelYear <= 2013)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1C4":
                                    if((vehModelYear >= 1995 && vehModelYear <= 2005) || 
                                        (vehModelYear >= 2012 && vehModelYear <= 2013)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1C6":
                                case "3C6": // ' DODG (Dodge)
                                    if(vehModelYear >= 2012 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3FT": // ' FORD (Ford)
                                    if((vehModelYear >= 2000 && vehModelYear <= 2005) || 
                                        (vehModelYear >= 2007 && vehModelYear <= 2009)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;                         
                                case "1FB":
                                case "2FT": // ' FORD (Ford)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2012){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1FM":
                                case "2FM":
                                case "1FT": // ' FORD (Ford)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "NM0": // ' FORD (Ford)
                                    if(vehModelYear >= 2010 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GT":
                                case "2GT": // ' GMC (GMC)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }else if(vehModelYear == 2010){
                                        switch(partialVIN.Substring(4, 2)){
                                            case "GA":
                                            case "GF":
                                            case "GG":
                                            case "GT":
                                            case "GU":
                                            case "HA":
                                                partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                                break;
                                        }
                                    }else if(vehModelYear >= 2011 && vehModelYear <= 2012){
                                        switch(partialVIN.Substring(4, 2)){
                                            case "7A":
                                            case "7F":
                                            case "7G":
                                            case "7T":
                                            case "7U":
                                            case "8A":
                                                partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                                break;
                                        }
                                    }
                                    break;               
                                case "3GT": // ' GMC (GMC)
                                    if((vehModelYear >= 1995 && vehModelYear <= 1999) || 
                                        (vehModelYear >= 2006 && vehModelYear <= 2009)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GJ":
                                case "2GJ": // ' GMC (GMC)
                                    if((vehModelYear >= 1995 && vehModelYear <= 2005) || 
                                        (vehModelYear >= 2008 && vehModelYear <= 2012)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GK":
                                case "2GK":
                                case "3GK": //  ' GMC (GMC)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2CT": // ' GMC (GMC)
                                    if(vehModelYear >= 2010 && vehModelYear <= 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "5GR": // ' HUMM (Hummer)
                                    if(vehModelYear >= 2006 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "5GT": //' HUMM (Hummer)
                                    if(vehModelYear >= 2006 && vehModelYear <= 2010){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "5GN": // ' HUMM (Hummer)
                                    if(vehModelYear >= 2009 && vehModelYear <= 2010){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GG": // ' ISZU (Isuzu)
                                    if((vehModelYear >= 1996 && vehModelYear <= 2000) || 
                                        (vehModelYear >= 2006 && vehModelYear <= 2008)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4NU": // ' ISZU (Isuzu)
                                    if(vehModelYear >= 2003 && vehModelYear <= 2008){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1J8": //' JEEP (Jeep)
                                    if(vehModelYear >= 2002 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1J4": // ' JEEP (Jeep)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "5LT": // ' LINC (Lincoln)
                                    if((vehModelYear == 2002) || 
                                        (vehModelYear >= 2006 && vehModelYear <= 2008)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "5LM": //' LINC (Lincoln)
                                    if(vehModelYear >= 1998 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2LM": //' LINC (Lincoln)
                                    if(vehModelYear >= 2007 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4F4": // ' MAZD (Mazda)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4F2": // ' MAZD (Mazda)
                                    if((vehModelYear >= 2001 && vehModelYear <= 2006) || 
                                        (vehModelYear >= 2008 && vehModelYear <= 2012)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4M2": // ' MERC (Mercury)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4A4": // ' MITS (Mitsubishi)
                                    if((vehModelYear >= 2004 && vehModelYear <= 2008) || 
                                        (vehModelYear >= 2010 && vehModelYear <= 2011)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1Z7":// ' MITS (Mitsubishi)
                                    if(vehModelYear >= 2006 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "JA4": // ' MITS (Mitsubishi)
                                    if(vehModelYear >= 1995 && vehModelYear <= 2012){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2CK":// ' PONT (Pontiac)
                                    if(vehModelYear >= 2006 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "5S3": // ' SAAB (Saab)
                                    if(vehModelYear >= 2005 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3G0":// ' SAAB (Saab)
                                    if(vehModelYear == 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3GS":// ' SATN (Saturn)
                                    if(vehModelYear >= 2008 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "5GZ": // ' SATN (Saturn)
                                    if(vehModelYear >= 2002 && vehModelYear <= 2010){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2V8": // ' VLKS (Volkswagen)
                                    if(vehModelYear == 2009){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2V4": //' VLKS (Volkswagen)
                                     if(vehModelYear >= 2010 && vehModelYear <= 2011){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                     break;
                                case "JAE":
                                    if(vehModelYear == 1999){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3G5":
                                    if(vehModelYear >= 2002 && vehModelYear <= 2007){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2CC":
                                    if(vehModelYear >= 1995 && vehModelYear <= 1998){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1CC":
                                    if(vehModelYear >= 1997 && vehModelYear <= 1998){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1CN":
                                    if(vehModelYear >= 1997 && vehModelYear <= 2004){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2J4":
                                    if(vehModelYear >= 1995 && vehModelYear <= 2005){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "3CA":
                                    if(vehModelYear == 2001){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1D5":
                                case "3D5":
                                    if(vehModelYear == 2006){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1C8":
                                case "2C8":
                                case "3C8":
                                case "3G7":
                                    if(vehModelYear >= 2001 && vehModelYear <= 2005){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1B4":
                                case "1B7":
                                case "2B4":
                                case "2B5":
                                case "2B7":
                                case "3B7":
                                    if(vehModelYear >= 1995 && vehModelYear <= 2002){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1B8":
                                case "2B8":
                                    if(vehModelYear >= 2001 && vehModelYear <= 2002){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4S6":
                                    if(vehModelYear >= 1999 && vehModelYear <= 2002){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "JAA":
                                    if(vehModelYear == 1995){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "JAC":
                                    if((vehModelYear >= 1995 && vehModelYear <= 1996) || 
                                        (vehModelYear >= 2000 && vehModelYear <= 2002)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4S2":
                                    if((vehModelYear >= 1995 && vehModelYear <= 1996) || 
                                        (vehModelYear >= 2000 && vehModelYear <= 2004)){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2J8":
                                    if(vehModelYear >= 2002 && vehModelYear <= 2005){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4M4":
                                    if(vehModelYear >= 1995 && vehModelYear <= 1999){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "2MR":
                                    if(vehModelYear >= 2004 && vehModelYear <= 2007){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "JA7":
                                    if(vehModelYear >= 1995 && vehModelYear <= 1996){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1Z3":
                                    if(vehModelYear == 2006){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "4N2":
                                    if(vehModelYear >= 2001 && vehModelYear <= 2002){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GH":
                                    if(vehModelYear >= 1995 && vehModelYear <= 2004){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1P4":
                                case "2P4":
                                    if(vehModelYear >= 1995 && vehModelYear <= 2000){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "1GM":
                                    if(vehModelYear >= 1995 && vehModelYear <= 2006){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "KM8": // ' HYUN (Hyundai)
                                    if(vehModelYear >= 2001 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 6) + "&" + partialVIN.Substring(partialVIN.Length-3, 3);
                                    }
                                    break;
                                case "5NM": // ' HYUN (Hyundai)
                                    if(vehModelYear >= 2007 && vehModelYear <= 2009){
                                        //' We actually need to check VehicleMakeCode on this one!! 
                                        //'If moBOC("VehicleMakeCode").Contains("HYUN") Then
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                        //'End If
                                    }
                                    break;
                                case "5UX": // 'BMW (BMW)
                                    if(vehModelYear >= 2002 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;
                                case "WBX": // 'BMW (BMW)
                                    if(vehModelYear >= 2004 && vehModelYear <= 2010){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;
                                case "5YM":// 'BMW (BMW)
                                    if(vehModelYear >= 2010 && vehModelYear <= 2012){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;
                                case "WDC":
                                case "4JG": // ' MBNZ (Mercedes)
                                    if(vehModelYear >= 2002 && vehModelYear <= 2009){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;
                                case "4S4": // ' SUBA (Subaru)
                                    if(vehModelYear >= 2003 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;
                                case "JF2":// ' SUBA (Subaru)
                                    if(vehModelYear >= 2009 && vehModelYear <= 2013){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;
                                case "523":// ' SAAB (Saab)
                                    if(vehModelYear >= 2011 && vehModelYear <= 2012){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(partialVIN.Length-6, 6);
                                    }
                                    break;
                                case "ZFB":// ' FIAT (Fiat)
                                     if(vehModelYear >= 2014){
                                        partialVIN = partialVIN.Substring(0, 3) + "&" + partialVIN.Substring(4, 4) + "&" + partialVIN.Substring(9, 1);
                                    }
                                     break;
                            }

                            switch(partialVIN.Substring(0, 5)){
                                case "KNDMC":// ' HYUN (Hyundai)
                                    if(vehModelYear >= 2007 && vehModelYear <= 2008){
                                        partialVIN = partialVIN.Substring(0, 6) + "&" + partialVIN.Substring(partialVIN.Length-3, 3);
                                    }
                                    break;
                            }

                           switch(partialVIN.Substring(0, 7)){
                               case "WBAFB33":
                                    if(vehModelYear >= 2000 && vehModelYear <= 2004){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;
                               case "WBAFA53":
                                    if(vehModelYear >= 2001 && vehModelYear <= 2004){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;                         
                               case "WBAFB53":
                               case "WBAFA13":
                                    if(vehModelYear == 2004){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                    break;                                                                             
                               case "WBAFA93":
                                   if(vehModelYear >= 2004 && vehModelYear <= 2005){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                    }
                                   break;
                               case "4USFE43":
                               case "4USFE83":
                                   if(vehModelYear == 2007){
                                        partialVIN = partialVIN.Substring(0, 7) + "&" + partialVIN.Substring(partialVIN.Length-2, 2);
                                   }
                                   break;
                           }
                        }
                        break;
                }
            }
            return partialVIN;
        }
        
        private int GetVehicleModelYear(string seventhChar, string tenthChar)
        {
            if (seventhChar.IsNumeric())
            {
                switch (tenthChar.ToUpper())
                {
                    
                    case "A": return 1980;
                    case "B": return 1981;
                    case "C": return 1982;
                    case "D": return 1983;
                    case "E": return 1984;
                    case "F": return 1985;
                    case "G": return 1986;
                    case "H": return 1987;
                    case "J": return 1988;
                    case "K": return 1989;
                    case "L": return 1990;
                    case "M": return 1991;
                    case "N": return 1992;
                    case "P": return 1993;
                    case "R": return 1994;
                    case "S": return 1995;
                    case "T": return 1996;
                    case "V": return 1997;
                    case "W": return 1998;
                    case "X": return 1999;
                    case "Y": return 2000;
                    case "1": return 2001;
                    case "2": return 2002;
                    case "3": return 2003;
                    case "4": return 2004;
                    case "5": return 2005;
                    case "6": return 2006;
                    case "7": return 2007;
                    case "8": return 2008;
                    case "9": return 2009;
                }
            }
            else
            {
                switch (tenthChar.ToUpper())
                {

                    case "A": return 2010;
                    case "B": return 2011;
                    case "C": return 2012;
                    case "D": return 2013;
                    case "E": return 2014;
                    case "F": return 2015;
                    case "G": return 2016;
                    case "H": return 2017;
                    case "J": return 2018;
                    case "K": return 2019;
                    case "L": return 2020;
                    case "M": return 2021;
                    case "N": return 2022;
                    case "P": return 2023;
                    case "R": return 2024;
                    case "S": return 2025;
                    case "T": return 2026;
                    case "V": return 2027;
                    case "W": return 2028;
                    case "X": return 2029;
                    case "Y": return 2030;
                    case "1": return 2031;
                    case "2": return 2032;
                    case "3": return 2033;
                    case "4": return 2034;
                    case "5": return 2035;
                    case "6": return 2036;
                    case "7": return 2037;
                    case "8": return 2038;
                    case "9": return 2039;
                }
            }
            return 0;
        }

        private int GetNonRegVINModelYear(string vinStub)
        {
            if (vinStub == "TPVDJ0B" || vinStub == "TPVDJ2B" || vinStub == "TPVDJ4B" || 
                    vinStub == "TPVDJ6B" || vinStub == "TPVDJ8B" || vinStub == "TPVDV0B" || 
                    vinStub == "TPVDV2B" || vinStub == "TPVDV4B" || vinStub == "TPVDV6B" || vinStub == "TPVDV8B")
            {
                return 1981;
            }
            if (vinStub == "1B7&G2AN&Y" || vinStub == "1B7&G2AX&Y" || vinStub == "1B7&G2AZ&Y" || 
                    vinStub == "1B7&L2AN&Y" || vinStub == "1B7&L2AX&Y" || vinStub == "1B7&L2AZ&Y")
            {
                return 2000;
            }
            if(vinStub == "3C4&Y4BB&1" || vinStub == "3C8&Y4BB&1" || vinStub == "3CA&Y4BB&1" || 
                    vinStub == "1B7&G2AN&1" || vinStub == "1B7&G2AX&1" || vinStub == "1B7&G2AZ&1" || 
                    vinStub == "1B7&L2AN&1" || vinStub == "1B7&L2AX&1" || vinStub == "1B7&L2AZ&1"){
                return 2001;
            }
            if (vinStub == "YS3FB76Y&A" || vinStub == "YS3FH76Y&A" || vinStub == "YS3FB79Y&A" || vinStub == "YS3FH79Y&A")
            {
                return 2010;
            }
            if (vinStub == "523MF11&&B" || vinStub == "523MP11&&B" || vinStub == "523MF12&&B" || vinStub == "523MP12&&B")
            {
                return 2011;
            }
            return 0;

        }

        public string VerifyCheckDigit(string vin)
        {
            vin = vin.ToUpper();
            string partialVIN = String.Empty;
            int vehModelYear;
            int sumOfProducts = 0;
            string checkDigit = "";

            string ninthChar = vin.Substring(8, 1);
            if (vin == "NONOWNER")
            {
                partialVIN = vin;
                vehModelYear = 1;
            }
            else
            {
                partialVIN = String.Concat(vin.Substring(0, 8), "&", vin.Substring(9, 1));
                vehModelYear = GetNonRegVINModelYear(String.Concat(vin.Substring(0, 3), "&", vin.Substring(4, 4), "&", vin.Substring(9, 1)));
                if (vehModelYear == 0)
                    vehModelYear = GetVehicleModelYear(vin.Substring(6, 1), vin.Substring(9, 1));
                if (vehModelYear > 1980)
                {
                    for (int i = 1; i <= 17; i++)
                    {
                        sumOfProducts += (AssignedValue(vin.Substring(i - 1, 1)) * WeightFactor(i));
                    }
                    int remainder = sumOfProducts % 11;
                    switch (remainder)
                    {
                        case 10:
                            checkDigit = "X";
                            break;
                        default:
                            checkDigit = remainder.ToString();
                            break;
                    }
                    if (ninthChar != checkDigit)
                    {
                        return "ERROR! VIN Check Digit does not match! ";
                    }
                }
            }
            return string.Empty;

        }

        private int WeightFactor(int position)
        {
            switch(position){
                case 1:
                case 11:
                    return 8;
                case 2:
                case 12:
                    return 7;
                case 3:
                case 13:
                    return 6;
                case 4:
                case 14:
                    return 5;
                case 5:
                case 15:
                    return 4;
                case 6:
                case 16:
                    return 3;
                case 7:
                case 17:
                    return 2;
                case 8:
                    return 10;
                case 9:
                    return 0;
                case 10:
                    return 9;
                default:
                    return -1;
            }
        }

        public static int AssignedValue(string vinLetter)
        {
            int letterNum = 0;
            if (Int32.TryParse(vinLetter, out letterNum))
            {
                return letterNum;
            }
            else
            {
                switch (vinLetter.ToUpper())
                {
                    case "A":
                    case "J":
                        return 1;
                    case "B":
                    case "K":
                    case "S":
                        return 2;
                    case "C":
                    case "L":
                    case "T":
                        return 3;
                    case "D":
                    case "M":
                    case "U":
                        return 4;
                    case "E":
                    case "N":
                    case "V":
                        return 5;
                    case "F":
                    case "W":
                        return 6;
                    case "G":
                    case "P":
                    case "X":
                        return 7;
                    case "H":
                    case "Y":
                        return 8;
                    case "R":
                    case "Z":
                        return 9;
                    default:
                        return -1;
                }
            }
        }
    }

    public static class Extension
    {
        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }
    }
}
