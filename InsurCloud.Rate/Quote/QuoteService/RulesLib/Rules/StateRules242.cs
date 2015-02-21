using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace RulesLib.Rules
{
    public class StateRules242 : StateRules2
    {


        public override void CheckDWICountUnder21(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverList = string.Empty;

            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol))
                {                    
                    if (drv.DriverStatus.ToUpper() == "ACTIVE" && !drv.IsMarkedForDelete)
                    {
                        DateTime age21DOB;
                        age21DOB = drv.DOB.AddYears(21);
                        int dwi = 0;
                        foreach (clsBaseViolation viol in drv.Violations)
                        {
                            if (viol.ViolGroup == "DWI" && viol.ViolDate < age21DOB)
                            {
                                dwi++;
                                break;
                            }
                        }
                        if (dwi > 0)
                        {
                            driverList = (driverList == string.Empty) ? drv.IndexNum.ToString() : string.Concat(driverList, ", ", drv.IndexNum.ToString());
                        }
                    }                    
                }
            }
            if (driverList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following driver(s) have drug or alcohol violations prior to the age of 21 - " + driverList + ".", "ChargeableDWICount", "IER");
            }
        }

        public override void CheckDWICount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverList = string.Empty;

            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol))
                {
                    if (drv.DriverStatus.ToUpper() == "ACTIVE" && !drv.IsMarkedForDelete)
                    {                        
                        int dwi = 0;
                        foreach (clsBaseViolation viol in drv.Violations)
                        {
                            if (viol.ViolGroup == "DWI" && viol.ViolDate.AddMonths(35) > pol.EffDate)
                            {
                                dwi++;
                                break;
                            }
                        }
                        if (dwi > 2)
                        {
                            driverList = (driverList == string.Empty) ? drv.IndexNum.ToString() : string.Concat(driverList, ", ", drv.IndexNum.ToString());
                        }
                    }
                    
                }
            }
            if (driverList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following driver(s) have more than 2 drug or alcohol violations - " + driverList + ".", "ChargeableDWICount", "IER");
            }
        }

        public override void CheckArtisanUse(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int counter = 0;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol))
                {
                    string vehUseType = veh.TypeOfUseCode.ToUpper();
                    if (vehUseType == "ARTISAN" || vehUseType == "BUSINESS" || vehUseType == "ART")
                    {
                        counter++;
                    }
                }
            }
            if (pol.Program.ToUpper() == "DIRECT" && pol.CallingSystem.ToUpper() == "WEBRATER")
            {
                if (counter > 0)
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles with Business Use require company approval.", "ArtisanLimit", "IER");
                }                
            }
            else
            {
                if (counter > 1)
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: Policies may have no more than one (1) business use vehicle.", "ArtisanLimit", "IER");
                }                
            }
        }

        public override void CheckTotalPoints(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int totalPoints = 0;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.DriverStatus.ToUpper() == "ACTIVE")
                {
                    totalPoints += drv.Points;
                }
            }
            if (totalPoints > 17)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: Maxmium of 18 violation points is allowed for all drivers", "DriverPtTot", "IER");
            }
        }


        public override void CheckPolicyPoints(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            // 6/13/2011 2 point restriction removed for TX
        }

        public override void CheckMinimumPermitAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverList = string.Empty;
            int minDriverPermitAge = 0;
            int.TryParse(stateInfo.GetStringValue(pol, "MINIMUM", "PERMITAGE", "", connectionString), out minDriverPermitAge);
            if (minDriverPermitAge > 0)
            {
                foreach (clsEntityDriver drv in pol.Drivers)
                {
                    if (drv.DriverStatus.ToUpper() == "PERMITTED" || drv.DriverStatus.ToUpper() == "EXCLUDED")
                    {
                        if (drv.Age < minDriverPermitAge)
                        {
                            driverList = (driverList == string.Empty) ? drv.IndexNum.ToString() : string.Concat(driverList, ", ", drv.IndexNum.ToString());
                        }
                    }
                }
            }
            if (driverList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following driver(s) are under the minimum age for state permit - " + driverList + ".", "MinDriverPermitAge", "IER");
            }
        }

        public override void CheckVehicleAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete && veh.VehicleAge > 40 && veh.VinNo.ToUpper().Trim() != "NONOWNER")
                {
                    vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                }
            }
            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: Vehicle age cannot be greater than 40 years.  - " + vehicleList + ".", "VehicleAgeGT40", "IER");
            }
        }

        public override void CheckSymbol2(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete && CheckSymbol2(veh, pol, stateInfo, connectionString))
                {
                    vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                }
            }
            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) are unacceptable due to vehicle value (Code: Symb) - " + vehicleList + ".", "SymbolOver22", "IER");
            }
        }

        private bool CheckSymbol2(clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if(veh.VinNo.ToUpper() == "NONOWNER" || 
                    VehicleHelper.VehicleSymbolIsStatedAmountSymbol(veh.VehicleSymbolCode, int.Parse(veh.VehicleYear)) ||
                    VehicleHelper.VehicleSymbolIsStatedAmountSymbol(veh.CompSymbolCode, int.Parse(veh.VehicleYear)) ||
                    VehicleHelper.VehicleSymbolIsStatedAmountSymbol(veh.CollSymbolCode, int.Parse(veh.VehicleYear))){
                return false;
            }

            int maxSymbol = 0;
            maxSymbol = VehicleHelper.GetMaxMSRPSymbol(veh.VehicleYear, pol, stateInfo, connectionString);
            int vehSymbolCode = 0;
            int.TryParse(veh.VehicleSymbolCode, out vehSymbolCode);
            if (vehSymbolCode > maxSymbol)
            {
                return true;
            }

            if (int.Parse(veh.VehicleYear) >= 2011)
            {
                int compSymbol = 0;
                int.TryParse(veh.CompSymbolCode.Trim(), out compSymbol);
                if (compSymbol > maxSymbol)
                {
                    return true;
                }

                int collSymbol = 0;
                int.TryParse(veh.CollSymbolCode.Trim(), out collSymbol);

                if (collSymbol > maxSymbol)
                {
                    return true;
                }
            }

            return false;
        }

        public override void CheckDriverPointsClassic(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int maxPoints = 0;
            int.TryParse(ProgramSettingHelper.FindSetting("MaxDriverPoints", pol, connectionString), out maxPoints);

            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol) && drv.Points > maxPoints)
                {
                    if (pol.Program.ToUpper() == "DIRECT" && pol.CallingSystem.ToUpper() == "WEBRATER")
                    {
                        NotesHelper.AddNote(pol, "Ineligible Risk: Driver " + drv.IndexNum.ToString() + " has more than " + maxPoints.ToString() + " violation points", "DriverPoints", "IER");
                    }
                    else
                    {
                        NotesHelper.AddNote(pol, "Underwriting Approval Needed: Driver " + drv.IndexNum.ToString() + " has more than " + maxPoints + " violation points", "DriverPoints", "UWW");
                    }                    
                }
            }
        }

        public override void CheckDriverViolationsClassic(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol) && ViolationHelper.ChargeableViolationCount(drv) > 6)
                {
                    NotesHelper.AddNote(pol, "Underwriting Approval Needed: Driver " + drv.IndexNum.ToString() + " has more than 6 violations", "DriverViolCount", "UWW");
                }
            }
        }
    }
}
