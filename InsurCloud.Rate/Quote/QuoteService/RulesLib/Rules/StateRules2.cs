using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using Helpers;
using Helpers.Models;

namespace RulesLib.Rules
{
    public class StateRules2 : IPPAStateRule
    {        
        public virtual bool PolicyHasIneligibleRisk(clsPolicyPPA pol)
        {            
            return false;
        }
        public virtual bool HasSurchargeOverride(clsPolicyPPA pol, string factortype, string factorCode, string connectionString)
        {
            string SQL = "";

            SQL = " SELECT OverrideID FROM pgm" + pol.Product + pol.StateCode + ".." + "RiskOverride with(nolock)";
            SQL += " WHERE ";
            if(pol.QuoteID.Trim() != ""){
                SQL += " (QuoteID = @QuoteID ";
            }
            else
            {
                SQL += " (";
            }
            if (pol.PolicyID.Trim() != "")
            {
                SQL += pol.QuoteID.Trim() != "" ? "or PolicyNo = @PolicyID )" : " PolicyNo = @PolicyID )";
            }
            else
            {
                SQL += ")";
            }
            SQL += " AND DeletedFlag = 0 ";
            SQL += " AND RiskCode = @RiskCode ";

            List<SqlParameter> parms = new List<SqlParameter>();

            parms.Add(DBHelper.AddParm("@QuoteID", SqlDbType.VarChar, 50, pol.QuoteID));
            parms.Add(DBHelper.AddParm("@PolicyID", SqlDbType.VarChar, 50, pol.PolicyID));
            parms.Add(DBHelper.AddParm("@RiskCode", SqlDbType.VarChar, 50, factortype + ":" + factorCode));


            string value = DBHelper.GetScalarValue(SQL, "OverrideID", connectionString, parms);
            if (value != string.Empty)
            {
                return true;
            }
            return false;
        }
        public virtual bool HasOPF(clsPolicyPPA pol)
        {
            try
            {
                foreach (clsEntityLienHolder lien in pol.LienHolders)
                {
                    if (lien.EntityType.ToUpper() == "PFC")
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public virtual bool EligibleForTransferDiscount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            bool ineligible = false;

            if (pol.PolicyInsured.PriorLimitsCode != "0")
            {
                int maxTransferDiscountPoints = 5;
                //GetProgramSetting("MaxTransferDiscountPoints", pol)
                int maxDaysLapse = -1;
                //GetProgramSetting("MaxTransferDaysLapse", pol)
                int minPriorMonths = -1;
                //GetProgramSetting("MinTransferPriorMonths", pol)
                TimeSpan ts = pol.EffDate - pol.PolicyInsured.PriorExpDate;
                if (maxDaysLapse > 0 && ts.Days > maxDaysLapse)
                {
                    ineligible = true;
                }

                if (minPriorMonths > 0 && pol.PolicyInsured.MonthsPriorContCov < minPriorMonths)
                {
                    ineligible = true;
                }

                foreach (clsEntityDriver drv in pol.Drivers)
                {
                    if (!drv.IsMarkedForDelete)
                    {
                        if (drv.DriverStatus.ToUpper() == "ACTIVE" || drv.DriverStatus.ToUpper() == "PERMITTED")
                        {
                            if (drv.Points > maxTransferDiscountPoints)
                            {
                                ineligible = true;
                                break;
                            }
                        }
                    }
                }                
            }
            return !ineligible;
        }      
        protected bool SpouseAllowedOnNonOwner(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string value = stateInfo.GetStringValue(pol, "NONOWNER", "ALLOWSPOUSE", "", connectionString);
            if (value.ToUpper().Trim() == "TRUE")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected string CheckPhysicalDamageRestriction(clsVehicleUnit veh)
        {
            if (!veh.IsMarkedForDelete && Int32.Parse(veh.VehicleYear) < DateTime.Now.AddYears(-15).Year)
            {
                if (VehicleHelper.PhysicalDamageCoverageRequested(veh))
                {
                    return veh.IndexNum.ToString();
                }
            }
            return string.Empty;
        }
        protected string CheckVehicleStatedValue(clsVehicleUnit veh)
        {
            int symbol = -1;
            Int32.TryParse(veh.VehicleSymbolCode, out symbol);
            if (symbol > 0)
            {
                if (VehicleHelper.VehicleSymbolIsStatedAmountSymbol(veh.VehicleSymbolCode, Int32.Parse(veh.VehicleYear)))
                {
                    if (veh.StatedAmt < 500 || veh.StatedAmt > 60000)
                    {
                        return veh.IndexNum.ToString();
                    }
                }
            }
            return "";
        }
        public virtual void CheckNonOwner(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            bool isNonOwner = false;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete && veh.VinNo.ToUpper() == "NONOWNER")
                {
                    isNonOwner = true;
                    break;
                }
            }

            if (isNonOwner)
            {
                for (int i = pol.VehicleUnits.Count - 1; i < 0; i--)
                {
                    if (!pol.VehicleUnits[i].IsMarkedForDelete && pol.VehicleUnits[i].VinNo.ToUpper() != "NONOWNER")
                    {
                        pol.VehicleUnits.Remove(pol.VehicleUnits[i]);
                    }
                }

                foreach (clsEntityDriver drv in pol.Drivers)
                {
                    if (!drv.IsMarkedForDelete && drv.RelationToInsured.ToUpper() != "SELF" && drv.DriverStatus.ToUpper() == "ACTIVE")
                    {
                        if (drv.RelationToInsured.ToUpper() == "SPOUSE" && drv.MaritalStatus.ToUpper() == "MARRIED")
                        {
                            if (!SpouseAllowedOnNonOwner(pol, stateInfo, connectionString))
                            {
                                NotesHelper.AddNote(pol, "Ineligible Risk: Only one named insured is allowed on a Non-owner policy", "NonOwner", "IER");             
                            }
                        }
                        else
                        {
                            NotesHelper.AddNote(pol, "Ineligible Risk: Only one named insured and spouse (if applicable) are allowed on a Non-owner policy", "NonOwner", "IER");             
                        }
                    }
                }
            }            
        }
        public virtual void CheckEffectiveDate(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (pol.PolicyID != "")
            {
                if (pol.EffDate < DateTime.Today)
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: Cannot have an Effective Date in the past", "PastEffDate", "IER");
                }
                else if (pol.EffDate > DateTime.Today.AddDays(30))
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: Cannot have an Effective Date more than 30 days in the future", "FutureEffDate", "IER");
                }
            }
            else if (pol.TransactionEffDate > pol.ExpDate)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: Cannot have an Effective Date past the policy expiration date", "FutureEffDateEnd", "IER");
            }
            
        }
        public virtual void CheckCoverages(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string invalidCoverages = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (!veh.IsMarkedForDelete)
                {
                    invalidCoverages += VehicleHelper.ValidateCoverages(veh, pol, stateInfo, connectionString);
                }
            }

            if (invalidCoverages != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: " + invalidCoverages, "InvalidCoverages", "IER");
            }
        }
        public virtual void CheckPhysicalDamageWeather(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol))
                {
                    if (VehicleHelper.PhysicalDamageCoverageRequested(veh))
                    {
                        bool cont = true;
                        int zipCode;

                        try
                        {
                            zipCode = Int32.Parse(pol.PolicyInsured.Zip);
                            if (zipCode > 99999)
                            {
                                cont = false;
                                NotesHelper.AddNote(pol, "Ineligible Risk: Invalid Policy Insured Zip Code.", "InvalidZip", "IER");
                            }
                        }
                        catch
                        {
                            cont = false;
                            NotesHelper.AddNote(pol, "Ineligible Risk: Invalid Policy Insured Zip Code.", "InvalidZip", "IER");
                        }

                        if (cont)
                        {
                            try
                            {
                                WeatherAlerts wa = WeatherHelper.CheckWeather(pol.PolicyInsured.Zip, connectionString);
                                if (wa.TropicalStormsLikely() && !WeatherHelper.WeatherOverride(pol, stateInfo, connectionString))
                                {
                                    //if not test
                                    NotesHelper.AddNote(pol, "Ineligible Risk: Imperial is unable to bind policies with Physical Damage coverage during a severe weather event.  This includes, but is not limited to, tropical storm warning, hurricane warning, and ice storms.", "TropicalStorm", "IER");
                                }
                            }
                            catch(Exception ex)
                            {
                                if (ex.Message.Contains("Error with one or more zip codes"))
                                {
                                    //do nothing
                                }
                                else
                                {
                                    throw new Exception("Error in CheckPhysicalDamageWeather", ex);
                                }
                            }
                        }
                    }
                }
                break;
            }
        }
        public virtual void CheckPhysicalDamageRestriction(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            string vehicle = string.Empty;

            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                vehicle = CheckPhysicalDamageRestriction(veh);
                vehicleList = (vehicleList == string.Empty) ? vehicle : string.Concat(vehicleList, ", ", vehicle);
            }
            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) have Physical Damage coverage and are older than 15 years - " + vehicleList + ".", "PhysDamageOver15", "IER");
            }
        }
        public virtual void CheckNamedInsuredActive(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol) && drv.RelationToInsured.ToUpper().Trim() == "SELF" && !DriverHelper.IsActiveOrExcludedDriver(drv))
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: The Policyholder must be either Active or Excluded.", "InsuredDriverStatus", "IER");                    
                }
            }
        }
        public virtual void CheckDriverNamesEntered(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.DriverStatus.ToUpper() != "NHH" && DriverHelper.DriverApplies(drv, pol))
                {
                    DriverHelper.CleanDriverName(drv);
                    if (DriverHelper.NameMissing(drv))
                    {
                        NotesHelper.AddNote(pol, "Ineligible Risk: Driver " + drv.IndexNum.ToString() + " must have both first and last names entered.", "DriverNamesProvided", "IER");                    
                    }
                }
            }
        }
        public virtual void CheckPolicyHasNamedInsuredDriver(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.RelationToInsured.ToUpper().Trim() == "SELF")
                {
                    return;
                }
            }
            NotesHelper.AddNote(pol, "Ineligible Risk: At least one driver must have relation to insured as 'Insured'.", "NoNamedInsured", "IER");                    
        }
        public virtual void CheckPermittedNotExcluded(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            CheckPolicyHasNamedInsuredDriver(pol, stateInfo, connectionString);
            string driverList = string.Empty;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol) && drv.DriverStatus.ToUpper().Trim() == "PERMITTED")
                {
                    if (drv.MaritalStatus.ToUpper() == "SINGLE" && drv.Age <= 18)
                    {
                        //Do nothing
                    }
                    else
                    {
                        driverList = (driverList == string.Empty) ? drv.IndexNum.ToString() : string.Concat(driverList, ", ", drv.IndexNum.ToString());
                    }
                }
            }
            if (driverList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following driver(s) must be rated as Active or Excluded - " + driverList + ".", "PermittedNotPermitted", "IER");                    
            }
        }
        public virtual void CheckVehicleStatedValue(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol))
                {
                    string vehicle = CheckVehicleStatedValue(veh);
                    if (vehicle != string.Empty)
                    {
                        vehicleList = (vehicleList == string.Empty) ? vehicle : string.Concat(vehicleList, ", ", vehicle);
                    }
                }
            }
            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) do not have a valid stated value amount (It must be between $500 and $60,000) -  " + vehicleList + ".", "InvalidStatedValue", "IER");                    
            }
        }
        public virtual void CheckVehicleComplete(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol) && veh.IncompleteVehicle)
                {
                    vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                }
            }
            if (vehicleList != string.Empty)
            {
                if (pol.Program.ToUpper() == "DIRECT" && pol.CallingSystem.ToUpper() == "WEBRATER")
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) have an Invalid VIN. (Code 092008) -  " + vehicleList + ".", "IncompleteVehicle", "IER");
                }
                else
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) are incomplete vehicles -  " + vehicleList + ".", "IncompleteVehicle", "IER");                    
                }                
            }
        }
        public virtual void CheckLienholderType(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol))
                {
                    bool lienTypeRequired = false;
                    string lienList = string.Empty;
                    foreach (clsEntityLienHolder lienHolder in veh.LienHolders)
                    {
                        if (lienHolder.EntityType.ToUpper().Trim() == "AN")
                        {
                            lienHolder.EntityType = "AI";
                        }
                        if (lienHolder.EntityType == string.Empty)
                        {
                            lienTypeRequired = true;
                            lienList = (lienList == string.Empty) ? lienHolder.EntityName1 : string.Concat(lienList, ", ", lienHolder.EntityName1);
                        }
                    }

                    if (lienTypeRequired)
                    {
                        vehicleList = (vehicleList == string.Empty) ? string.Concat("(", veh.IndexNum.ToString(), ") ", lienList) : string.Concat("; (", veh.IndexNum.ToString(), ") ", lienList);
                    }
                }
            }
            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following lienholders(s) must have a lienholder type selected - " + vehicleList + ".", "LienTypeRequired", "IER");                    
            }
        }
        public virtual void CheckLeasedVehHasLienholder(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (pol.CallingSystem.ToUpper().Trim() != "BRIDGE")
            {
                string vehicleList = string.Empty;
                foreach (clsVehicleUnit veh in pol.VehicleUnits)
                {
                    if (VehicleHelper.VehicleApplies(veh, pol) && VehicleHelper.IsLeasedVehicle(veh) && !VehicleHelper.HasLessorListed(veh))
                    {
                        vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                    }
                }

                if (vehicleList != string.Empty)
                {
                    if (pol.Program.ToUpper().Trim() == "DIRECT" && pol.CallingSystem.ToUpper().Trim() == "WEBRATER")
                    {
                        NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) are required to have a Lien Holder/Leasing Company - " + vehicleList + ".", "LeasedVehWOAddlInsured", "IER");
                    }
                    else
                    {
                        NotesHelper.AddNote(pol, "Ineligible Risk: The following lienholders(s) must have a lienholder type selected - " + vehicleList + ".", "LienTypeRequired", "IER");                    
                    }
                }
            }
        }
        public virtual void CheckGaragingZip(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int statusNum = PolicyHelper.NormalizeStatus(pol);
            bool diffGaragingZip = false;
            bool UWQAnswered = false;
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol))
                {
                    if (veh.Zip != pol.PolicyInsured.Zip && veh.VinNo != "NONOWNER")
                    {
                        diffGaragingZip = true;
                        vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                    }
                }
            }
            if (diffGaragingZip)
            {                
                foreach (clsUWQuestion q in pol.UWQuestions)
                {
                    if (q.QuestionCode == "306" && q.AnswerText.ToUpper().Contains("NO;") && q.AnswerText.Length > 4)
                    {
                        UWQAnswered = true;
                    }
                }
            }

            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Underwriting Approval Needed: The garaging Zip Code(s) for the following vehicle(s) do not match the Policy Address.  Please correct the Zip Code or contact Imperial for approval -  " + vehicleList + ".", "InvalidGaragingZip", "UWW");
                if (pol.UWQuestions.Count > 0 && !UWQAnswered)
                {
                    if (pol.Program == "DIRECT" && pol.CallingSystem.ToUpper() == "WEBRATER")
                    {
                        NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) are not garaged at the policy address. The address must be entered for the vehicle(s) under question #7 on the Purchase Screen -  " + vehicleList + ".", "InvalidGaragingZip", "IER");
                    }
                    else
                    {
                        if (pol.StateCode == "09")
                        {
                            NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) are not garaged at the policy address. The address must be entered for the vehicle(s) under #18 in the Additional Information section -  " + vehicleList + ".", "InvalidGaragingZip", "IER");
                        }
                        else
                        {
                            NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) are not garaged at the policy address. The address must be entered for the vehicle(s) under #7 in the Additional Information section -  " + vehicleList + ".", "InvalidGaragingZip", "IER");
                        }
                    }
                }
            }

        }

        public virtual void CheckRentToOwnVehHasLienholder(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol) && CheckRentToOwnVehHasLienholder(veh, pol, stateInfo, connectionString))
                {
                    vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                }
            }
            if(vehicleList != string.Empty){
                NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) do not have an Additional Insured or a Loss Payee listed - " + vehicleList + ".", "RentToOwnWOAddlInsured", "IER");
            }
                
        }

        private bool CheckRentToOwnVehHasLienholder(clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (FactorsHelper.FactorOn(veh.Factors, "RENT_TO_OWN"))
            {
                bool lienExists = false;
                foreach (clsEntityLienHolder lien in veh.LienHolders)
                {
                    if (lien.EntityType == "AI" || lien.EntityType == "LP")
                    {
                        lienExists = true;
                        break;
                    }
                }
                if (!lienExists)
                {
                    return false;                    
                }
            }
            return true;
        }

        public virtual void CheckCustomEquipmentLimits(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (VehicleHelper.VehicleApplies(veh, pol) && CheckCustomEquipmentLimits(veh, pol, stateInfo, connectionString))
                {
                    vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                }
            }
            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The Custom Equipment limit chosen does not match the amount of Custom Equipment entered for the following vehicle(s) -  " + vehicleList + ".", "CustomEquipMismatch", "IER");
            }
        }

        private bool CheckCustomEquipmentLimits(clsVehicleUnit veh, clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {

            foreach (clsBaseCoverage cov in veh.Coverages)
            {
                if (cov.CovGroup.ToUpper().Trim() == "SPE" && !cov.IsMarkedForDelete)
                {
                    int pos = cov.CovLimit.ToString().IndexOf('-');
                    decimal lowerLimit = decimal.Parse(cov.CovLimit.Substring(1, pos - 1));
                    decimal upperLimit = decimal.Parse(cov.CovLimit.Substring(pos + 1, cov.CovLimit.Trim().Length));
                    if (veh.CustomEquipmentAmt < lowerLimit || veh.CustomEquipmentAmt > upperLimit)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual void CheckSR22Term(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (pol.Term == 1)
            {
                foreach (clsEntityDriver drv in pol.Drivers)
                {
                    if (DriverHelper.DriverApplies(drv, pol))
                    {
                        if (drv.SR22)
                        {
                            NotesHelper.AddNote(pol, "Ineligible Risk: Policy Term must be greater than 1 month to have an SR22.", "OneTermWithSR22", "IER");
                            break;
                        }
                    }
                }
            }
        }

        public virtual void CheckVehicleBusinessUse(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int numVehsWithBusUse = 0;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (FactorsHelper.FactorOn(veh.Factors, "BUS_USE"))
                {
                    numVehsWithBusUse++;
                }
            }
            if (numVehsWithBusUse > 1)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: Only one vehicle on a policy may have Business Use.", "OnlyOneBusinessUse", "IER");
            }
        }

        public virtual void CheckPayPlan(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (!PolicyHelper.ValidPayPlan(pol, stateInfo, connectionString))
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The policy has an invalid pay plan. Please make sure a valid pay plan is selected.", "InvalidPayPlan", "IER");
            }
        }

        public virtual void CheckMarried(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            if (pol.VehicleUnits.Count > 0)
            {
                if (!pol.VehicleUnits[0].IsMarkedForDelete)
                {
                    if (pol.VehicleUnits[0].VinNo.ToUpper() != "NONOWNER")
                    {
                        bool marriedNIWithSpouse = false;
                        foreach (clsEntityDriver drv in pol.Drivers)
                        {
                            if (!drv.IsMarkedForDelete && drv.RelationToInsured.ToUpper() == "SELF" && drv.MaritalStatus.ToUpper() == "MARRIED")
                            {
                                foreach (clsEntityDriver spouseDrv in pol.Drivers)
                                {
                                    if (!spouseDrv.IsMarkedForDelete && spouseDrv.RelationToInsured.ToUpper() == "SPOUSE")
                                    {
                                        marriedNIWithSpouse = true;
                                        break;
                                    }
                                }
                                if (!marriedNIWithSpouse)
                                {
                                    if (pol.Program.ToUpper() == "DIRECT" && pol.CallingSystem.ToUpper() == "WEBRATER")
                                    {
                                        NotesHelper.AddNote(pol, "Ineligible Risk: Policyholder is listed as married.  Spouse must be listed on the application as Active or Excluded.", "MarriedWithoutSpouse", "IER");
                                    }
                                    else
                                    {
                                        NotesHelper.AddNote(pol, "Ineligible Risk: Named Insured is listed as married.  Spouse must be listed on the application as active or excluded.", "MarriedWithoutSpouse", "IER");
                                    }
                                }
                            }
                        }

                        if (marriedNIWithSpouse)
                        {
                            int numMarriedDrivers = 0;
                            int numNewMarriedDrivers = 0;
                            int numMaleMarried = 0;
                            int numFemaleMarried = 0;
                            bool oneDriverIsActive = false;

                            foreach (clsEntityDriver drv in pol.Drivers)
                            {
                                if (!drv.IsMarkedForDelete && drv.MaritalStatus.ToUpper() == "MARRIED")
                                {
                                    numMarriedDrivers++;
                                    if (drv.IsNew)
                                    {
                                        numNewMarriedDrivers++;
                                    }
                                    if (drv.Gender.ToUpper().StartsWith("M"))
                                    {
                                        numMaleMarried++;
                                    }
                                    else
                                    {
                                        numFemaleMarried++;
                                    }
                                    if (drv.DriverStatus.ToUpper().Trim() == "ACTIVE")
                                    {
                                        oneDriverIsActive = true;
                                    }
                                }
                            }
                            if (pol.CallingSystem.Contains("OLE") || pol.CallingSystem.ToUpper().Contains("UWC"))
                            {
                                if (numMarriedDrivers % 2 != 0 && numNewMarriedDrivers != 0)
                                {
                                    NotesHelper.AddNote(pol, "Ineligible Risk: There is an uneven number of married drivers on the policy.", "UnevenMarriedDrivers", "IER");
                                }
                            }
                            else
                            {
                                if (numMarriedDrivers % 2 != 0 || (numMaleMarried != numFemaleMarried && oneDriverIsActive))
                                {
                                    NotesHelper.AddNote(pol, "Ineligible Risk: There is an uneven number of married drivers on the policy.", "UnevenMarriedDrivers", "IER");
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void CheckActiveDriverDOB(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverList = string.Empty;
            bool hasActiveDriver = false;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol))
                {
                    if (drv.DriverStatus.ToUpper() == "ACTIVE")
                    {
                        hasActiveDriver = true;
                        if (drv.DOB == DateTime.MinValue)
                        {
                            driverList = (driverList == string.Empty) ? drv.IndexNum.ToString() : string.Concat(driverList, ", ", drv.IndexNum.ToString());
                        }
                    }
                }
            }
            if (driverList != string.Empty)
            {
                if (pol.Program.ToUpper() == "DIRECT" && pol.CallingSystem.ToUpper() == "WEBRATER")
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: Driver(s)  " + driverList + " do not have a valid Date of Birth.", "InvalidDOB", "IER");
                }
                else
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: Driver(s)  " + driverList + " are listed as Active but do not have a valid Date of Birth.", "InvalidDOB", "IER");
                }
            }
            if (!hasActiveDriver)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The policy must contain at least one Active driver.", "NoActiveDrv", "IER");
            }
        }

        public virtual void CheckDriverDisclosure(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckMissingVIN(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckInsuredAddress(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckVehicleCount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckDLPattern(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckLienholderState(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckRoutingNumbers(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Nothing to do here
        }

        public virtual void CheckSR22Date(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckMSRPRestriction(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;
            int maxSymbol = 0;
            foreach (clsVehicleUnit veh in pol.VehicleUnits)
            {
                if (veh.VinNo != "NONOWNER" && veh.StatedAmt == 0 && !VehicleHelper.VehicleHasDefaultSymbol(veh))
                {
                    maxSymbol = VehicleHelper.GetMaxMSRPSymbol(veh.VehicleYear, pol, stateInfo, connectionString);
                    int vehSymbol = 0;
                    if (veh.PriceNewSymbolCode.Trim() == string.Empty)
                    {
                        vehSymbol = 0;
                    }
                    else
                    {
                        int.TryParse(veh.PriceNewSymbolCode.Trim(), out vehSymbol);
                    }

                    if (vehSymbol > maxSymbol)
                    {
                        vehicleList = (vehicleList == string.Empty) ? vehicleList : string.Concat(vehicleList, ", ", veh.IndexNum);
                    }
                }
            }

            string price = "$45,000";
            if (maxSymbol == 57 || maxSymbol == 24)
            {
                price = "$60,000";
            }

            if (vehicleList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) have an Original Cost New above " + price + " - " + vehicleList + ".", "MSRPOver45k", "IER");
            }
        }

        public virtual void CheckPhysicalDamageWithLienholder(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckNamedInsuredAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int namedInsuredCount = 0;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (!drv.IsMarkedForDelete && drv.DriverStatus.ToUpper() == "ACTIVE")
                {
                    if (drv.RelationToInsured.ToUpper() == "SELF")
                    {
                        namedInsuredCount++;
                        if (drv.Age < 18)
                        {
                            if (pol.Program.ToUpper() == "DIRECT" && pol.CallingSystem.ToUpper() == "WEBRATER")
                            {
                                NotesHelper.AddNote(pol, "Ineligible Risk: The Policyholder must be at least 18 years of age.", "UnderAgeNamedInsured", "IER");
                            }
                            else
                            {
                                NotesHelper.AddNote(pol, "Ineligible Risk: Named insured must be at least 18 years of age.", "UnderAgeNamedInsured", "IER");
                            }
                        }
                    }
                }
            }

            if (namedInsuredCount > 1)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: Cannot have more than 1 driver with relationship of SELF.", "SelfCount", "IER");
            }
        }

        public virtual void CheckSR22CaseCode(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckSR22Excluded(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string driverList = string.Empty;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (DriverHelper.DriverApplies(drv, pol))
                {
                    if (drv.SR22 && drv.DriverStatus.ToUpper() == "EXCLUDED")
                    {
                        driverList = (driverList == string.Empty) ? drv.IndexNum.ToString() : string.Concat(driverList, ", ", drv.IndexNum.ToString());
                    }
                }
            }
            if (driverList != string.Empty)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: Drivers requesting an SR22 filing must be Active.", "SR22Excluded", "IER");
            }
        }

        public virtual void CheckValidVIN(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }        

        public virtual void CheckMVRDriverDOBMismatch(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckDWICountUnder21(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckDWICount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckArtisanUse(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckOutOfStateZip(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            bool allowOutOfState = true;
            DataRow[] rows = stateInfo.GetRows(pol, "VEHICLE", "TERRITORY", "ALLOWOUTOFSTATE", connectionString);
            foreach (DataRow row in rows)
            {
                if (row["ItemValue"].ToString().ToUpper() == "FALSE")
                {
                    allowOutOfState = false;
                }
                else
                {
                    allowOutOfState = true;                    
                }
                break;
            }

            string vehicleList = string.Empty;
            if (!allowOutOfState)
            {
                DataTable codeTerritory = VehicleHelper.LoadCodeTerritoryDefinitionsTable(pol, stateInfo, connectionString);

                if (codeTerritory != null && codeTerritory.Rows.Count > 0)
                {

                    foreach (clsVehicleUnit veh in pol.VehicleUnits)
                    {
                        DataRow[] vehRows = codeTerritory.Select("Zip = '" + veh.Zip + "'");
                        if (!veh.IsMarkedForDelete && vehRows.Length == 0)
                        {
                            vehicleList = (vehicleList == string.Empty) ? veh.IndexNum.ToString() : string.Concat(vehicleList, ", ", veh.IndexNum.ToString());
                        }
                    }
                }
                if (vehicleList != string.Empty)
                {
                    NotesHelper.AddNote(pol, "Ineligible Risk: The following vehicle(s) have an out of state garaging zip - " + vehicleList + ".", "OutOfStateZip", "IER");
                }                
            }
        }

        public virtual void CheckTotalPoints(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckPolicyPoints(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            int totalPoints = 0;
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if ((drv.DriverStatus.ToUpper() == "ACTIVE" || drv.DriverStatus.ToUpper() == "PERMITTED" || drv.DriverStatus.ToUpper() == "EXCLUDED" && !drv.IsMarkedForDelete))
                {
                    totalPoints += drv.Points;
                }
            }
            if (totalPoints > 2)
            {
                NotesHelper.AddNote(pol, "Ineligible Risk: Policy is ineligible based on the number of driver violation points", "PolPointsover2", "IER");
            }
        }

        public virtual void CheckMinimumPermitAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckDLDupes(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            List<string> listDLN = new List<string>();
            foreach (clsEntityDriver drv in pol.Drivers)
            {
                if (drv.DLN.Length > 0)
                {
                    if (listDLN.Contains(drv.DLN.ToUpper()))
                    {
                        NotesHelper.AddNote(pol, "Ineligible Risk: Driver's License Number is duplicated on two or more drivers.", "DLNDuplicate", "IER");
                        break;
                    }
                    else
                    {
                        if (drv.DLN.ToUpper().Trim() != "UNKNOWN")
                        {
                            listDLN.Add(drv.DLN.ToUpper());
                        }
                    }
                }                
            }
        }

        public virtual void CheckVehicleAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckSymbol2(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckDriverPointsClassic(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckDriverViolationsClassic(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public virtual void CheckSalvagedUWW(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            string vehicleList = string.Empty;

            if (pol.UWQuestions.Count > 0)
            {
                //Add check later
            }
        }



        public void CheckPhysicianStatement(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public void CheckNonInteractiveMVR(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public void CheckMilitaryDiscount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public void CheckWindowEtch(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public void CheckExistingRenewal(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public void CheckActualRateDate(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public void CheckMatureDriverDiscountDocsRequired(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }

        public void CheckScholasticDiscountDocsRequired(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString)
        {
            //Not Implemented
        }
    }
}
