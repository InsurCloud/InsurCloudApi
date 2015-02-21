using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;

namespace RulesLib.Rules
{
    public interface IPPAStateRule
    {        
        bool PolicyHasIneligibleRisk(clsPolicyPPA pol);
        bool HasSurchargeOverride(clsPolicyPPA pol, string factortype, string factorCode, string connectionString);
        bool HasOPF(clsPolicyPPA pol);
        bool EligibleForTransferDiscount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckNonOwner(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckEffectiveDate(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckCoverages(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckPhysicalDamageWeather(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckPhysicalDamageRestriction(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);        
        void CheckNamedInsuredActive(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDriverNamesEntered(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckPermittedNotExcluded(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckVehicleStatedValue(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckVehicleComplete(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckLienholderType(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckLeasedVehHasLienholder(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckGaragingZip(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckRentToOwnVehHasLienholder(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckCustomEquipmentLimits(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckSR22Term(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckVehicleBusinessUse(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckPayPlan(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckMarried(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckActiveDriverDOB(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDriverDisclosure(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckMissingVIN(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckInsuredAddress(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckVehicleCount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDLPattern(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckLienholderState(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckRoutingNumbers(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckSR22Date(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckMSRPRestriction(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckPhysicalDamageWithLienholder(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckNamedInsuredAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckSR22CaseCode(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckSR22Excluded(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckValidVIN(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDLDupes(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckMVRDriverDOBMismatch(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDWICount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDWICountUnder21(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckArtisanUse(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckOutOfStateZip(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckTotalPoints(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckPolicyPoints(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckMinimumPermitAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);      
        void CheckVehicleAge(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckSymbol2(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDriverPointsClassic(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckDriverViolationsClassic(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckSalvagedUWW(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckPhysicianStatement(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckNonInteractiveMVR(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckMilitaryDiscount(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckWindowEtch(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckExistingRenewal(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckActualRateDate(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckMatureDriverDiscountDocsRequired(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
        void CheckScholasticDiscountDocsRequired(clsPolicyPPA pol, StateInfoHelper stateInfo, string connectionString);
    }
}
