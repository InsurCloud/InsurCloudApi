using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rate.Models;

namespace RatingTester
{
    public static class ClientHelper
    {
        public static Policy GetPolicyObject()
        {
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
            addr.Address1 = "3225 Golfing Green Drive";
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

        private static Coverage GetCoverage(CoverageGroup covGroup, string covLimit, string covDed)
        {
            Coverage cov = new Coverage();
            cov.Deductible = covDed;
            cov.Limit = covLimit;
            cov.Group = covGroup;
            return cov;
        }
    }
}
