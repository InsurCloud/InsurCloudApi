using CoreCommon.Attributes;
using InsurCloud.Auth.Api.Models;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace InsurCloud.Auth.Api.Controllers
{
    [RequireHttps]
    [RoutePrefix("api/Quote")]
    public class QuoteController : ApiController
    {
        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        [Authorize]
        [HttpGet]
        [Route("v1/quote/{id}", Name = "quote")]
        public async Task<IHttpActionResult> GetQuote(string id)
        {
            try
            {

                if (id == "78910")
                {
                    return NotFound();
                }
                else
                {
                    QuoteView policy = new QuoteView();
                    policy.QuoteStatus = "Lead";
                    policy.QuoteUniqueId = Guid.NewGuid().ToString();
                    policy.PolicyNumber = "";
                    policy.Company.CarrierName = "Renissance";
                    policy.Company.CompanyName = "Renissance Mutual";
                    policy.Company.ContactInfo.Add(new ContactView { Address = new Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Billing", PhoneNumber = "1(800)555-1234", EmailAddress = "underwriting@renissance.com" });
                    policy.Company.ContactInfo.Add(new ContactView { Address = new Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Claims (Existing)", PhoneNumber = "1(800)555-2234", EmailAddress = "claims@renissance.com" });
                    policy.Company.ContactInfo.Add(new ContactView { Address = new Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Claims (New)", PhoneNumber = "1(800)555-3234", EmailAddress = "newclaim@renissance.com" });
                    policy.Company.ContactInfo.Add(new ContactView { Address = new Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Underwriting", PhoneNumber = "1(800)555-1234", EmailAddress = "underwriting@renissance.com" });
                    policy.Company.ImageURL = "";
                    policy.Company.ProductLine = "Personal";
                    policy.Company.Product = "Private Auto";
                    policy.Company.StateAbbreviation = "TX";
                    policy.Company.Program = "Non-Standard";
                    policy.CoveredUnits.Add(new PersonalAutoCoveredUnit { IndexNumber = 1, Address = new Address { PostalCode = "75241" }, ModelYear = 2011, Make = "Dodge", VIN = "1D7RB1CT2BS505077", AssignedDriverNumber = 1, Description = "2011 Dodge" });
                    policy.CoveredUnits[0].Coverages().Add(new Coverage { CoverageCode = "BI:30/60:L:P", CovGroupName = "Bodily Injury", CovGroupAbbr = "BI", IndexNumber = 1, LimitDeductibleDescription = "$30,000 per person, $60,000 per accident", WrittenPremium = 333.00 });
                    policy.CoveredUnits[0].Coverages().Add(new Coverage { CoverageCode = "PD:25:L:P", CovGroupName = "Property Damage", CovGroupAbbr = "PD", IndexNumber = 2, LimitDeductibleDescription = "$25,000 per incident", WrittenPremium = 222.00 });
                    policy.CoveredUnits.Add(new PersonalAutoCoveredUnit { IndexNumber = 2, Address = new Address { PostalCode = "75241" }, ModelYear = 1997, Make = "Buick", VIN = "2G4WB52K3V1436641", AssignedDriverNumber = 2, Description = "1997 Buick" });
                    policy.CoveredUnits[1].Coverages().Add(new Coverage { CoverageCode = "BI:30/60:L:P", CovGroupName = "Bodily Injury", CovGroupAbbr = "BI", IndexNumber = 1, LimitDeductibleDescription = "$30,000 per person, $60,000 per accident", WrittenPremium = 222.00 });
                    policy.CoveredUnits[1].Coverages().Add(new Coverage { CoverageCode = "PD:25:L:P", CovGroupName = "Property Damage", CovGroupAbbr = "PD", IndexNumber = 2, LimitDeductibleDescription = "$25,000 per incident", WrittenPremium = 210.00 });
                    policy.EffectiveDate = new DateTime(2014, 12, 11);
                    policy.ExpirationDate = new DateTime(2014, 6, 11);
                    HouseholdMember insured = new HouseholdMember { IndexNumber = 1, RelationToInsured = "Self", ContactInfo = new ContactView { Address = new Address { Address1 = "3225 Golfing Green Drive", City = "Dallas", State = "TX", PostalCode = "75234", County = "Dallas" }, EmailAddress = "mprice@insurcloud.com", PhoneNumber = "(214)240-8085" }, BirthDate = new DateTime(1974, 7, 3), Gender = "Male", MaritalStatus = "Married", FirstName = "Matt", LastName = "Price", SetupForENotification = "Yes", RatedAge = 40 };
                    HouseholdMember jointInsured = new HouseholdMember { IndexNumber = 2, RelationToInsured = "Spouse", ContactInfo = new ContactView { Address = new Address { Address1 = "3225 Golfing Green Drive", City = "Dallas", State = "TX", PostalCode = "75234", County = "Dallas" }, EmailAddress = "", PhoneNumber = "" }, BirthDate = new DateTime(1971, 4, 30), Gender = "Female", MaritalStatus = "Married", FirstName = "Colleen", LastName = "Price", SetupForENotification = "No", RatedAge = 43 };
                    jointInsured.Violations.Add(new Violation { IndexNumber = 1, Points = 1, ViolationDescription = "Speeding, Generally", ViolationDate = new DateTime(2013, 3, 17) });
                    jointInsured.Violations.Add(new Violation { IndexNumber = 2, Points = 0, ViolationDescription = "SR-22 Filling", ViolationDate = new DateTime(2015, 2, 13) });
                    policy.PrimaryNamedInsured = insured;
                    policy.JointNamedInsured = jointInsured;
                    policy.HouseholdMembers.Add(insured);
                    policy.HouseholdMembers.Add(jointInsured);
                    policy.HouseholdMembers.Add(new HouseholdMember { IndexNumber = 1, RelationToInsured = "Child", ContactInfo = new ContactView { Address = new Address { Address1 = "3225 Golfing Green Drive", City = "Dallas", State = "TX", PostalCode = "75234", County = "Dallas" }, EmailAddress = "", PhoneNumber = "" }, BirthDate = new DateTime(2000, 3, 1), Gender = "Female", MaritalStatus = "Single", FirstName = "Ella", LastName = "Price", SetupForENotification = "No", RatedAge = 15 });

                    policy.PayPlan = "Installments";
                    policy.PolicyTermType = "New";
                    policy.Producer = new ProducerView { AgencyInfo = new AgencyLocationView { AgencyId = 55555, DisplayName = "Bob's Agency", ImageURL = "" }, ContactInfo = new ContactView { Address = new Address { Address1 = "1423 Test Way", City = "Test", State = "TX", County = "Dallas", PostalCode = "75432" }, PhoneNumber = "555-444-1234", EmailAddress = "jmartin@agency.com" }, FirstName = "Joseph", LastName = "Martin", ProducerUserId = "123413415" };
                    policy.Producer.AgencyInfo.ContactInfo.Add(new ContactView { Address = new Address { Address1 = "1423 Test Way", City = "Test", State = "TX", County = "Dallas", PostalCode = "75432" }, ContactType = "Main", EmailAddress = "bobsagency@agency.com", PhoneNumber = "1(800)555-4444", Name = "Main Street" });

                    return Ok(policy);
                }
            }
            catch
            {
                return InternalServerError();
            }
        }

        [Authorize]
        [HttpGet]
        [Route("v1/quotes/{searchText}", Name = "quoteSearch")]
        public async Task<IHttpActionResult> QuoteSearch(string searchText)
        {
            try
            {
                List<QuoteSearchResult> results = TestResults();
                return Ok(results.Where(p => p.PolicyNumber == searchText || p.InsuredFullName.Contains(searchText) || p.InsuredPhoneNumber == searchText).ToList());
            }
            catch
            {
                return InternalServerError();
            }
        }


        private List<QuoteSearchResult> TestResults()
        {
            List<QuoteSearchResult> results = new List<QuoteSearchResult>();
            results.Add(new QuoteSearchResult { QuoteNumber = "242000012345", RateAmount = 750.51, LastRateDate = new DateTime(2015, 2, 25), LastRateDateFormatted = "02/25/2015", InsuredFullName = "Milton Price", InsuredPhoneNumber = "(214)240-8085", QuoteStatus = "Quote" });
            results.Add(new QuoteSearchResult { QuoteNumber = "242000012346", RateAmount = 451.81, LastRateDate = new DateTime(2015, 2, 26), LastRateDateFormatted = "02/26/2015", InsuredFullName = "Jack Russell", InsuredPhoneNumber = "(972)065-0056", QuoteStatus = "Application" });
            results.Add(new QuoteSearchResult { QuoteNumber = "242000012347", RateAmount = 0.0, LastRateDate = new DateTime(2015, 2, 24), LastRateDateFormatted = "02/24/2015", InsuredFullName = "Palma Granite", InsuredPhoneNumber = "(214)326-1648", QuoteStatus = "Lead" });
            results.Add(new QuoteSearchResult { QuoteNumber = "242000012348", RateAmount = 1254.48, LastRateDate = new DateTime(2015, 1, 31), LastRateDateFormatted = "01/31/2015", InsuredFullName = "Arnold Palmer", InsuredPhoneNumber = "(476)652-0543", QuoteStatus = "Quote" });
            results.Add(new QuoteSearchResult { QuoteNumber = "242000012349", RateAmount = 354.24, LastRateDate = new DateTime(2015, 2, 15), LastRateDateFormatted = "02/15/2015", PolicyNumber = "0012341234", InsuredFullName = "Jamie Foxx", InsuredPhoneNumber = "(817)220-8465", QuoteStatus = "Policy" });
            return results;
        }

        [Authorize]
        [HttpGet]
        [Route("v1/quotes", Name = "quoteSearchAll")]
        public async Task<IHttpActionResult> QuoteSearchAll()
        {
            try
            {
                List<QuoteSearchResult> results = TestResults();
                return Ok(results);
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
