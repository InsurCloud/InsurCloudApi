using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorPolicy;
using Helpers;
using Helpers.Model;

namespace Helpers
{
    public static class QuoteExchangeHelpers
    {
        public static clsPolicyPPA MapPolicy(Policy policy)
        {
            //Map Policy to CorPolicy.clsPolicyPPA

            clsPolicyPPA pol = new clsPolicyPPA();

            //pol = GetDummyData();

            pol.Status = "2";
            pol.Program = "DIRECT";
            pol.CallingSystem = "WEBRATER";
            pol.EffDate = policy.EffectiveDate;
            pol.Term = 6;
            pol.PayPlanCode = "205";
            pol.RateDate = DateTime.Now;
            pol.StateCode = "42";
            pol.Product = "2";
            pol.AppliesToCode = "N";
            pol.ProgramInfo = new clsEntityProgramInfo();
            pol.ProgramInfo.CompanyCode = "IF";
            pol.Agency = new clsEntityAgency();
            pol.Agency.AgencyID = "12345";
            pol.PolicyTermTypeInd = "New";
            
            //Map Insured Info
            pol.PolicyInsured = new clsEntityPolicyInsured();
            MapPolicyInsured(policy, pol.PolicyInsured);
            foreach (Driver drv in policy.Drivers)
            {
                clsEntityDriver driver = new clsEntityDriver();
                MapDriver(policy, drv, driver);
                pol.Drivers.Add(driver);
            }

            foreach (Vehicle veh in policy.Vehicles)
            {
                clsVehicleUnit vehicle = new clsVehicleUnit();
                MapVehicle(policy, veh, vehicle);
                pol.VehicleUnits.Add(vehicle);
            }
            
            return pol;
        }

        private static void MapVehicle(Policy policy, Vehicle veh, clsVehicleUnit vehicle)
        {
            
            vehicle.VinNo = veh.VIN;
            vehicle.VehicleYear = veh.ModelYear.ToString();
            vehicle.VehicleMakeCode = veh.Make;
            vehicle.VehicleModelCode = veh.Model;
            vehicle.VehicleBodyStyleCode = veh.BodyStyle;
            vehicle.Zip = veh.GaragingAddress.PostalCode;
            vehicle.VehicleTypeCode = veh.VehicleType.ToString();
            vehicle.StatedAmt = decimal.Parse(veh.StatedAmount.ToString());
            vehicle.TypeOfUseCode = veh.UsageType.ToString();
            foreach(Coverage cov in policy.PolicyLevelCoverages){
                clsPACoverage cover = MapCoverage(cov, true);
                vehicle.Coverages.Add(cover);
            }
            foreach(Coverage cov in veh.VehicleLevelCoverages){
                clsPACoverage cover = MapCoverage(cov, false);
                vehicle.Coverages.Add(cover);
                
            }
            foreach (Lienholder lien in veh.Lienholders)
            {
                clsEntityLienHolder lienhold = new clsEntityLienHolder();
                Address mailAddr = lien.ContactInfo.Addresses.Find(p => p.AddressType == AddressType.Mail);
                lienhold.Address1 = mailAddr.Address1;
                lienhold.Address2 = mailAddr.Address2;
                lienhold.City = mailAddr.City;
                lienhold.State = mailAddr.State;
                lienhold.County = mailAddr.County;
                lienhold.EntityName1 = lien.CompanyName;
                lienhold.EntityName2 = lien.DBA;
                lienhold.DownPaymentAmount = lien.DownPaymentAmount;
                lienhold.NumInstallments = lien.NumberOfInstallments;
                lienhold.PaymentInterval = lien.PaymentInterval;
                lienhold.PaymentMethod = lien.PaymentMethod;
                lienhold.FirstPaymentDate = lien.FirstPaymentDate;
                lienhold.FederalIDNo = lien.FederalIDNo;
                lienhold.EntityType = lien.LienHolderType.ToString();
                lienhold.LoanNum = lien.LoanNumber;
                vehicle.LienHolders.Add(lienhold);                
            }
           
            

        }

        private static clsPACoverage MapCoverage(Coverage cov, bool isPolicyLevel)
        {
            clsPACoverage cover = new clsPACoverage();
            cover.CovGroup = GetCoverageGroup(cov);
            cover.CovDeductible = cov.Deductible;
            cover.CovLimit = cov.Limit;
            cover.PolicyLevel = isPolicyLevel;
            cover.CovCode = GetCoverageCode(cov);
            return cover;
        }

        private static string GetCoverageGroup(Coverage cover)
        {
            switch (cover.Group)
            {
                case CoverageGroup.Policy_BodilyInjury:
                    return "BI";
                case CoverageGroup.Policy_PropertyDamage:
                    return "PD";
                case CoverageGroup.Vehicle_Collision:
                    return "COL";
                case CoverageGroup.Vehicle_Comprehensive:
                    return "OTC";
                case CoverageGroup.Policy_MedicalPayments:
                    return "MED";
                case CoverageGroup.Policy_PersonalInjuryProtection:
                    return "PIP";
                case CoverageGroup.Vehicle_Rental:
                    return "REN";
                case CoverageGroup.Vehicle_Towing:
                    return "TOW";
                case CoverageGroup.Policy_UninsuredUnderinsuredMotoristBI:
                    return "UUMBI";
                case CoverageGroup.Policy_UninsuredUnderinsuredMotoristPD:
                    return "UUMPD";
                case CoverageGroup.Vehicle_SpecialEquipement:
                    return "SPE"; //TODO: REPLACE THIS WITH LOOKUP LOGIC
            }
            throw new ArgumentException("Unable to determine coverage code for mapping!");
        }

        private static string GetCoverageCode(Coverage cover)
        {
            switch (cover.Group)
            {
                case CoverageGroup.Policy_BodilyInjury:
                    return "BI:30/60:L:P";
                case CoverageGroup.Policy_PropertyDamage:
                    return "PD:25:L:P";
                case CoverageGroup.Vehicle_Collision:
                    decimal ded = 0;
                    if (cover.Deductible != null && cover.Deductible != string.Empty && decimal.TryParse(cover.Deductible, out ded))
                    {
                        if(ded <= 500)
                        {
                            return "COL:470:D:V";
                        }
                        else
                        {
                            return "COL:970:D:V";
                        }
                    }
                    break;
                case CoverageGroup.Vehicle_Comprehensive:
                    if (cover.Deductible != null && cover.Deductible != string.Empty && decimal.TryParse(cover.Deductible, out ded))
                    {
                        if (ded <= 500)
                        {
                            return "OTC:470:D:V";
                        }
                        else
                        {
                            return "OTC:970:D:V";
                        }
                    }
                    break;
                case CoverageGroup.Policy_MedicalPayments:
                    return "MED:600:L:P";
                case CoverageGroup.Policy_PersonalInjuryProtection:
                    return "PIP:2600:L:P";
                case CoverageGroup.Vehicle_Rental:
                    return "REN:20/600:L:V";
                case CoverageGroup.Vehicle_Towing:
                    return "TOW:50:L:V";
                case CoverageGroup.Policy_UninsuredUnderinsuredMotoristBI:
                    return "UUMBI:30/60:L:P";
                case CoverageGroup.Policy_UninsuredUnderinsuredMotoristPD:
                    return "UUMPD:25/250:B:P";
                case CoverageGroup.Vehicle_SpecialEquipement:
                    return "SPE:2001-2500:L:V"; //TODO: REPLACE THIS WITH LOOKUP LOGIC
            }
            throw new ArgumentException("Unable to determine coverage code for mapping!");
        }


        private static void MapDriver(Policy policy, Driver drv, clsEntityDriver driver)
        {
            driver.EntityName1 = drv.FirstName;
            driver.EntityName2 = drv.LastName;
            if (drv.Suffix != null && drv.Suffix != string.Empty)
            {
                driver.EntityName2 += " " + drv.Suffix;
            }
            if (drv.MiddleName != null && drv.MiddleName != string.Empty)
            {
                driver.EntityName2 += ", " + drv.MiddleName;
            }
            driver.MaritalStatus = drv.MaritalStatus.ToString();
            driver.RelationToInsured = drv.RelationToInsured.ToString();
            driver.SSN = drv.SSN;
            driver.DOB = drv.DOB;
            DateTime nextBDay = Helpers.DriverHelper.GetNextBirthDay(policy.EffectiveDate.Year, driver.DOB.Month, driver.DOB.Day);
            if (nextBDay > policy.EffectiveDate)
            {
                nextBDay = Helpers.DriverHelper.GetNextBirthDay(policy.EffectiveDate.Year - 1, driver.DOB.Month, driver.DOB.Day);
            }
            driver.Age = nextBDay.Year - driver.DOB.Year;
            driver.DriverStatus = drv.DriverStatus.ToString();
            if (drv.DriversLicense != null)
            {
                driver.DLNState = drv.DriversLicense.State;
                driver.DLN = drv.DriversLicense.DLN;
                driver.DLNStatus = drv.DriversLicense.Status.ToString();
                driver.LicenseIssueDate = drv.DriversLicense.IssuanceDate;
                driver.LicenseExpireDate = drv.DriversLicense.ExpirationDate;
            }
            if (drv.EmploymentInfo != null)
            {
                driver.Occupation = drv.EmploymentInfo.Occupation;
            }
            if (drv.FRFiling != null)
            {
                driver.SR22CaseCode = drv.FRFiling.CaseCode;
                driver.SR22 = drv.FRFiling.NeedSR22Filing;
                driver.SR22Date = drv.FRFiling.SR22Date;
            }
            else
            {
                driver.SR22 = false;
                driver.SR22CaseCode = "";
                driver.SR22Date = DateTime.MinValue;
            }
            driver.Gender = drv.Gender.ToString();
            if (drv.Violations != null && drv.Violations.Count > 0)
            {
                foreach (Violation viol in drv.Violations)
                {
                    clsBaseViolation violation = new clsBaseViolation();
                    violation.AtFault = viol.IsAtFault;
                    violation.ViolCode = viol.ViolationCode;
                    violation.ViolDate = viol.ViolationDate;
                    violation.ViolDesc = viol.ViolationDescription;
                    violation.ViolTypeCode = viol.ViolationType.ToString();
                    driver.Violations.Add(violation);
                }
            }
        }

        private static void MapPolicyInsured(Policy policy, clsEntityPolicyInsured insured)
        {

            Driver insuredDriver = policy.Drivers.Find(d => d.IsNamedInsured == true);
            if (insuredDriver != null)
            {
                insured.EntityName1 = insuredDriver.FirstName;
                insured.EntityName2 = insuredDriver.LastName;
                if (insuredDriver.ContactInfo != null && insuredDriver.ContactInfo.Addresses != null)
                {
                    Address mailAddr = insuredDriver.ContactInfo.Addresses.Find(p => p.AddressType == AddressType.Mail);
                    insured.Address1 = mailAddr.Address1;
                    insured.Address2 = mailAddr.Address2;
                    insured.City = mailAddr.City;
                    insured.State = mailAddr.State;
                    insured.Zip = mailAddr.PostalCode;
                    insured.County = mailAddr.County;
                }
                insured.DOB = insuredDriver.DOB;
                DateTime nextBDay = Helpers.DriverHelper.GetNextBirthDay(policy.EffectiveDate.Year, insured.DOB.Month, insured.DOB.Day);
                if (nextBDay > policy.EffectiveDate)
                {
                    nextBDay = Helpers.DriverHelper.GetNextBirthDay(policy.EffectiveDate.Year - 1, insured.DOB.Month, insured.DOB.Day);
                }
                insured.Age = nextBDay.Year - insured.DOB.Year;
                if (insuredDriver.DriversLicense != null)
                {
                    insured.DLN = insuredDriver.DriversLicense.DLN;
                    insured.DLNState = insuredDriver.DriversLicense.State;
                    insured.DLNStatus = insuredDriver.DriversLicense.State;
                    insured.LicenseIssueDate = insuredDriver.DriversLicense.IssuanceDate;
                    insured.LicenseExpireDate = insuredDriver.DriversLicense.ExpirationDate;
                    insured.LicenseStatus = insuredDriver.DriversLicense.Status.ToString();
                }
                insured.DriverStatus = insuredDriver.DriverStatus.ToString();
                if (insuredDriver.EmploymentInfo != null)
                {
                    insured.Employer = insuredDriver.EmploymentInfo.Employer;
                    insured.EmployerYears = insuredDriver.EmploymentInfo.YearsWithEmployer;
                    insured.Occupation = insuredDriver.EmploymentInfo.Occupation;
                }
                insured.Gender = insuredDriver.Gender.ToString();
                insured.MaritalStatus = insuredDriver.MaritalStatus.ToString();
                if (insuredDriver.OccupancyType != null)
                    insured.OccupancyType = insuredDriver.OccupancyType.ToString();
                insured.RelationToInsured = insuredDriver.RelationToInsured.ToString();
                if (insuredDriver.SSN != null)
                    insured.SSN = insuredDriver.SSN;
                //if (insuredDriver.Suffix != null)
                insured.EntityName2 += " " + insuredDriver.Suffix;

                if (policy.PriorCoverageInfo != null)
                {
                    insured.PriorLimitsCode = policy.PriorCoverageInfo.PriorLimits.ToString();
                    insured.MonthsPriorContCov = policy.PriorCoverageInfo.MonthsContinuousCoverage;
                    insured.PriorExpDate = policy.PriorCoverageInfo.ExpirationDate;
                }
                else
                {
                    insured.PriorLimitsCode = "0";
                    insured.MonthsPriorContCov = 0;
                    insured.PriorExpDate = DateTime.MinValue;
                }
            }
            else
            {
                throw new ArgumentException("There must be one driver that is listed as the Named Insured");
            }
        }

        private static clsPolicyPPA GetDummyData()
        {
            clsPolicyPPA pol = new clsPolicyPPA();

            pol.ProgramCode = "TXD";
            pol.RateDate = DateTime.Now;
            pol.Status = "1";
            pol.AppliesToCode = "B";
            pol.Product = "2";
            pol.StateCode = "42";
            pol.Term = 6;
            pol.Program = "Direct";
            pol.ProgramCode = "TXD";
            pol.FormType = "Direct";
            pol.Status = "1";
            pol.PayPlanCode = "205";
            pol.AppliesToCode = "N";
            pol.CallingSystem = "WebRater";
            pol.EffDate = DateTime.Now;
            pol.ExpDate = pol.EffDate.AddMonths(pol.Term);
            pol.Type = "NEW";
            pol.OrigQuoteDate = DateTime.Now;
            pol.QuoteID = "2420000001";

            pol.PolicyInsured = new clsEntityPolicyInsured();
            pol.PolicyInsured.State = "TX";
            pol.ProgramInfo = new clsEntityProgramInfo();
            pol.ProgramInfo.Address1 = "P.O. Box 702507";
            pol.ProgramInfo.Address2 = "";
            pol.ProgramInfo.City = "Dallas";
            pol.ProgramInfo.Fax = "(866) 530-3242";
            pol.ProgramInfo.Phone1 = "(888) 522-8242";
            pol.ProgramInfo.Phone2 = "";
            pol.ProgramInfo.State = "TX";
            pol.ProgramInfo.Zip = "75370-2507";
            pol.ProgramInfo.CompanyCode = "IF";
            pol.ProgramInfo.CountyCode = "";
            pol.ProgramInfo.DeliveryMethod = "";
            pol.ProgramInfo.FederalIDNo = "";
            pol.ProgramInfo.IndexNum = 1;
            pol.ProgramInfo.LegalStructureCode = "";

            clsEntityAgency Agency = new clsEntityAgency();
            Agency.AgencyID = "20000";
            Agency.EntityName1 = "InsurCloud Agency";
            Agency.EntityName2 = "";
            Agency.Address1 = "3225 Golfing Green Drive";
            Agency.Address2 = "";
            Agency.City = "Dallas";
            Agency.State = "TX";
            Agency.Zip = "75234";
            Agency.emailAddress = "mprice@insurcloud.com";
            Agency.Phone1 = "214.240.8085";
            Agency.Phone2 = "";
            Agency.Fax = "";
            Agency.IndexNum = 0;
            Agency.AgencyGroup = "";
            Agency.AgencyGroupID = "20000";
            Agency.CountyCode = "";
            Agency.DeliveryMethod = "";
            Agency.ePaymentAvailable = true;
            Agency.FederalIDNo = "58-1234564";
            Agency.GroupCodes = null;
            Agency.LegalStructureCode = "";
            pol.Agency = Agency;

            pol.PolicyInsured.FirstName = "Matt";
            pol.PolicyInsured.LastName = "Price";
            pol.PolicyInsured.Address1 = "3225 Golfing Green Drive";
            pol.PolicyInsured.Address2 = "";
            pol.PolicyInsured.City = "Dallas";
            pol.PolicyInsured.State = "TX";
            pol.PolicyInsured.Zip = "75234";
            pol.PolicyInsured.emailAddress = "mprice@insurcloud.com";
            pol.PolicyInsured.Phone1 = "214.240.8085";
            pol.PolicyInsured.PriorExpDate = DateTime.MinValue;
            pol.PolicyInsured.DLN = "10701538";
            pol.PolicyInsured.DLNState = "TX";
            pol.PolicyInsured.OccupancyType = "H";
            pol.PolicyInsured.CreditScore = 651;
            pol.PolicyInsured.MaritalStatus = "M";
            pol.PolicyInsured.PriorLimitsCode = "0";
            pol.PolicyInsured.DOB = new DateTime(1974, 7, 3);
            pol.PolicyInsured.Age = 40;
            pol.PolicyInsured.UWTier = "1";
            pol.Drivers = new List<clsEntityDriver>();
            pol.VehicleUnits = new List<clsVehicleUnit>();

            clsEntityDriver drv = new clsEntityDriver();
            drv.Age = 40;
            drv.DOB = new DateTime(1974, 7, 3);
            drv.RelationToInsured = "SELF";
            drv.MaritalStatus = "S";
            drv.Gender = "M";
            drv.DriverStatus = "ACTIVE";
            drv.DLNState = "TX";
            drv.FirstName = "Matt";
            drv.LastName = "Price";
            drv.IndexNum = 1;
            drv.Violations = new List<clsBaseViolation>();
            drv.IsNew = true;
            pol.Drivers.Add(drv);

            clsVehicleUnit veh = new clsVehicleUnit();
            veh.VinNo = "1HGCP2F75AA009704";
            veh.ValidVIN = true;
            veh.Zip = pol.PolicyInsured.Zip;
            veh.County = "Dallas";
            veh.VehicleYear = "2010";
            veh.VehicleMakeCode = "HONDA";
            veh.VehicleModelCode = "ACCORD EX";
            veh.VehicleRestraintTypeCode = "R";
            veh.VehiclePerformanceCode = "Q";
            veh.VehicleCylinderCode = "4";
            veh.VehicleBodyStyleCode = "SEDAN 4D";
            veh.VehicleAntiTheftCode = "P";
            veh.VehicleABSCode = "S";
            veh.VehicleClassCode = "44";
            veh.VehicleDaytimeLightCode = "S";
            veh.VehicleEngineTypeCode = "Q";
            veh.LiabilitySymbolCode = "295";
            veh.PIPMedLiabilityCode = "495";
            veh.CollSymbolCode = "";
            veh.CompSymbolCode = "";
            veh.PriceNewSymbolCode = "13";
            veh.VehicleSymbolCode = "15";
            veh.IndexNum = 1;
            veh.IsNew = true;

            veh.Coverages = new List<clsPACoverage>();

            clsPACoverage covBI = new clsPACoverage();
            covBI.CovGroup = "BI";
            covBI.CovDesc = "Bodily Injury";
            covBI.UnitNum = 1;
            covBI.IndexNum = 1;
            covBI.CovCode = "BI:25/50:L:P";
            covBI.CovLimit = "25/50";
            covBI.CovPrintLimit = "25/50";
            covBI.CovDeductible = "";
            covBI.PolicyLevel = true;
            covBI.IsNew = true;


            clsPACoverage covPD = new clsPACoverage();
            covPD.CovGroup = "PD";
            covPD.CovDesc = "Property Damage";
            covPD.UnitNum = 1;
            covPD.IndexNum = 2;
            covPD.CovCode = "PD:25:L:P";
            covPD.CovLimit = "25";
            covPD.CovPrintLimit = "25";
            covPD.CovDeductible = "";
            covPD.PolicyLevel = true;
            covPD.IsNew = true;

            veh.Coverages.Add(covBI);
            veh.Coverages.Add(covPD);
            //veh.AssignedDriverNum = 1;
            pol.VehicleUnits.Add(veh);


            clsVehicleUnit veh2 = new clsVehicleUnit();
            veh2.VinNo = "1GKFK263X9R210393";
            veh2.ValidVIN = true;
            veh2.Zip = pol.PolicyInsured.Zip;
            veh2.County = "Dallas";
            veh2.VehicleYear = "2010";
            veh2.VehicleMakeCode = "HONDA";
            veh2.VehicleModelCode = "ACCORD EX";
            veh2.VehicleRestraintTypeCode = "R";
            veh2.VehiclePerformanceCode = "Q";
            veh2.VehicleCylinderCode = "4";
            veh2.VehicleBodyStyleCode = "SEDAN 4D";
            veh2.VehicleAntiTheftCode = "P";
            veh2.VehicleABSCode = "S";
            veh2.VehicleClassCode = "44";
            veh2.VehicleDaytimeLightCode = "S";
            veh2.VehicleEngineTypeCode = "Q";
            veh2.LiabilitySymbolCode = "295";
            veh2.PIPMedLiabilityCode = "495";
            veh2.CollSymbolCode = "";
            veh2.CompSymbolCode = "";
            veh2.PriceNewSymbolCode = "13";
            veh2.VehicleSymbolCode = "15";
            veh2.IndexNum = 2;
            veh2.IsNew = true;

            veh2.Coverages = new List<clsPACoverage>();

            covBI = new clsPACoverage();
            covBI.CovGroup = "BI";
            covBI.CovDesc = "Bodily Injury";
            covBI.UnitNum = 2;
            covBI.IndexNum = 1;
            covBI.CovCode = "BI:25/50:L:P";
            covBI.CovLimit = "25/50";
            covBI.CovPrintLimit = "25/50";
            covBI.CovDeductible = "";
            covBI.PolicyLevel = true;
            covBI.IsNew = true;

            covPD = new clsPACoverage();
            covPD.CovGroup = "PD";
            covPD.CovDesc = "Property Damage";
            covPD.UnitNum = 2;
            covPD.IndexNum = 2;
            covPD.CovCode = "PD:25:L:P";
            covPD.CovLimit = "25";
            covPD.CovPrintLimit = "25";
            covPD.CovDeductible = "";
            covPD.PolicyLevel = true;
            covPD.IsNew = true;

            veh2.Coverages.Add(covBI);
            veh2.Coverages.Add(covPD);
            //veh.AssignedDriverNum = 1;
            pol.VehicleUnits.Add(veh2);

            return pol;

        }

        public static Quote MapQuote(clsPolicyPPA pol)
        {
            Quote quo = new Quote();
            string statusMessage;
            if (!NotesHelper.HasErrorNotes(pol.Notes, out statusMessage))
            {
                quo.EffectiveDate = pol.EffDate;
                quo.QuoteID = pol.QuoteID;
                quo.RateDate = pol.RateDate;
                quo.Term = pol.Term;
                quo.Options = new List<QuoteOption>();

                QuoteOption opt = new QuoteOption();
                opt.FullTermPremium = pol.FullTermPremium;
                opt.TotalPolicyFees = pol.TotalFees;
                opt.DownPaymentAmt = pol.DownPaymentAmt;
                opt.OptionNumber = 1;
                opt.OptionDescription = pol.PayPlanCode;
                quo.Options.Add(opt);
                quo.Status = "Success";
            }
            else
            {
                quo.Status = "ERROR";
                quo.StatusMessage = statusMessage;
            }
            return quo;
        }

        private static Coverage GetCoverage(CoverageGroup covGroup, string covLimit, string covDed)
        {
            Coverage cov = new Coverage();
            cov.Deductible = covDed;
            cov.Limit = covLimit;
            cov.Group = covGroup;
            return cov;
        }

        public static Policy GetPolicy()
        {
            //return GetHeaderData();

            clsBasePolicy pol = GetDummyData();
            Policy policy = new Policy();

            policy.EffectiveDate = DateTime.Now;
            policy.Term = 6;
            policy.PolicyLevelCoverages = new List<Coverage>();
            Coverage covBI = GetCoverage(CoverageGroup.Policy_BodilyInjury, "25/50", "");
            Coverage covPD = GetCoverage(CoverageGroup.Policy_PropertyDamage, "25", "");

            policy.PolicyLevelCoverages.Add(covBI);
            policy.PolicyLevelCoverages.Add(covPD);

            Driver drv = new Driver();
            drv.FirstName = "Matt";
            drv.LastName = "Price";
            drv.Gender = Gender.Male;
            drv.ContactInfo = new ContactInfo();
            Address addr = new Address();
            addr.Address1 = "3222 Golfing Green Drive";
            addr.Address2 = "";
            addr.City = "Dallas";
            addr.County = "Dallas";
            addr.State = "TX";
            addr.PostalCode = "75234";
            drv.ContactInfo.Addresses = new List<Address>();
            drv.ContactInfo.Addresses.Add(addr);
            PhoneNumber ph = new PhoneNumber();
            ph.Number = "(214) 240-8085";
            ph.PhoneType = PhoneType.Cell;
            ph.Extension = "";
            drv.ContactInfo.PhoneNumbers = new List<PhoneNumber>();
            drv.ContactInfo.PhoneNumbers.Add(ph);
            EmailAddress em = new EmailAddress();
            em.Address = "mprice@insurcloud.com";
            em.EmailType = EmailType.Work;
            drv.ContactInfo.EmailAddresses = new List<EmailAddress>();
            drv.ContactInfo.EmailAddresses.Add(em);
            drv.DOB = DateTime.Parse("01/01/1974");
            drv.DriversLicense = new DriversLicenseInfo();
            drv.DriversLicense.DLN = "10701538";
            drv.DriversLicense.State = "TX";
            drv.DriversLicense.Status = LicenseStatus.Valid;
            drv.DriverStatus = DriverStatus.Active;
            drv.IsNamedInsured = true;
            drv.OccupancyType = OccupancyType.OwnHome;
            drv.RelationToInsured = RelationToInsured.Self;
            drv.MaritalStatus = MaritalStatus.Single;

            EmploymentInfo emp = new EmploymentInfo();
            emp.Employer = "InsurCloud LLC";
            emp.Occupation = "Consultant";
            emp.YearsWithEmployer = 1;
            drv.EmploymentInfo = emp;

            FinancialResponsibilityFiling fr = new FinancialResponsibilityFiling();
            fr.CaseCode = "";
            fr.NeedSR22Filing = false;
            fr.SR22Date = DateTime.MinValue;
            fr.SR22State = "";
            drv.FRFiling = fr;

            Violation v = new Violation();
            v.ConvictionDate = DateTime.MinValue;
            v.ViolationDate = new DateTime(2014, 1, 20, 12, 33, 02);
            v.ViolationType = ViolationType.Citation;
            v.ViolationCode = "SPD";
            v.ViolationDescription = "Speeding 1 to 15 miles over speed limit";
            drv.Violations = new List<Violation>();
            drv.Violations.Add(v);

            UnderwritingQuestion uwq = new UnderwritingQuestion();
            uwq.QuestionCode = "1";
            uwq.AnswerCode = "1";
            uwq.AnswerText = "Yes";
            drv.UWQuestions = new List<UnderwritingQuestion>();
            drv.UWQuestions.Add(uwq);

            UnderwritingQuestion uwq2 = new UnderwritingQuestion();
            uwq2.QuestionCode = "1";
            uwq2.AnswerCode = "1";
            uwq2.AnswerText = "Yes";
            policy.UWQuestions = new List<UnderwritingQuestion>();
            policy.UWQuestions.Add(uwq);

            policy.Drivers = new List<Driver>();
            policy.Drivers.Add(drv);

            Vehicle veh = new Vehicle();
            veh.VIN = "1HGCP2F75AA009704";
            veh.OwnershipType = OwnershipType.OwnNoPayments;
            veh.GaragingAddress = new Address();
            veh.GaragingAddress = drv.ContactInfo.Addresses[0];
            veh.UsageType = UsageType.Commute;
            veh.VehicleType = VehicleType.PrivatePassenger;
            veh.VehicleLevelCoverages = new List<Coverage>();
            policy.Vehicles = new List<Vehicle>();
            policy.Vehicles.Add(veh);

            Vehicle veh2 = new Vehicle();
            veh2.VIN = "1FTEX15Y0DKA07840";
            veh2.OwnershipType = OwnershipType.OwnNoPayments;
            veh2.GaragingAddress = new Address();
            veh2.GaragingAddress = drv.ContactInfo.Addresses[0];
            veh2.UsageType = UsageType.Commute;
            veh2.VehicleType = VehicleType.PrivatePassenger;
            veh2.VehicleLevelCoverages = new List<Coverage>();
            policy.Vehicles.Add(veh2);

            Lienholder l = new Lienholder();
            l.CompanyName = "DATCU";
            l.EntityType = EntityType.Company;
            l.LienHolderType = LienHolderType.LossPayee;
            l.LoanNumber = "A12345";

            l.ContactInfo = new ContactInfo();
            l.ContactInfo.Addresses = new List<Address>();
            l.ContactInfo.Addresses.Add(new Address());
            l.ContactInfo.Addresses[0].Address1 = "1 Test Street";
            l.ContactInfo.Addresses[0].City = "Denton";
            l.ContactInfo.Addresses[0].State = "TX";
            l.ContactInfo.Addresses[0].County = "Denton";
            l.ContactInfo.Addresses[0].AddressType = AddressType.Mail;
            l.ContactInfo.PhoneNumbers = new List<PhoneNumber>();
            l.ContactInfo.PhoneNumbers.Add(new PhoneNumber());
            l.ContactInfo.PhoneNumbers[0].PhoneType = PhoneType.Work;
            l.ContactInfo.PhoneNumbers[0].Number = "214.240.8888";
            l.ContactInfo.PhoneNumbers[0].IsDefault = true;
            l.ContactInfo.PhoneNumbers[0].Extension = "";
            l.ContactInfo.EmailAddresses = new List<EmailAddress>();
            l.ContactInfo.EmailAddresses.Add(new EmailAddress());
            l.ContactInfo.EmailAddresses[0].EmailType = EmailType.Work;
            l.ContactInfo.EmailAddresses[0].Address = "test@datcu.com";
            l.MainContact = new Individual();
            l.MainContact.FirstName = "Don";
            l.MainContact.LastName = "Dat";
            l.MainContact.EntityType = EntityType.Individual;
            policy.Vehicles[0].Lienholders = new List<Lienholder>();
            policy.Vehicles[0].Lienholders.Add(l);
            
            return policy;
        }

        public static bool IsEnoughToRate(clsPolicyPPA pol, out string msg)
        {
            msg = "";
            clsBaseNote note = NotesHelper.FindNote(pol, "NEI", "");
            if (note != null)
            {
                msg = note.NoteText;
                return false;
            }
            return true;            

        }
    }
}
