using CoreCommon.Attributes;
using CoreAgency.Repository;
using CorePolicy.Model;
using CorePolicy.Repository;
using InsurCloud.Auth.Api.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CoreAuthentication.Repository;
using CoreAuthentication.Model;
using CoreAgency.Model;
using CoreAuthentication.Enum;
using CoreCommon.Model;
using CorePolicy.Context;
using CorPolicy;


namespace InsurCloud.Auth.Api.Controllers
{
    [RequireHttps]
    [RoutePrefix("api/Policy")]
    public class PolicyController : ApiController
    {

        public AuthRepository _authRepo = new AuthRepository();
        public AgencyRepository _agencyRepo = new AgencyRepository();


        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }


        private IHttpActionResult GetTestPolicyView(string id)
        {
            if (id == "78910")
            {
                return NotFound();
            }
            else
            {
                PolicyView policy = new PolicyView();
                policy.PolicyUniqueId = id;
                PolicyNumberContext ctx = new PolicyNumberContext();
                PolicyNumber num = ctx.PolicyNumbers.Where(p => p.PolicyUniqueId == policy.PolicyUniqueId).FirstOrDefault();
                
                policy.PolicyNumber = num.FormattedNumber();
                policy.PolicyStatus = "ISSUED";
                
                policy.Company.CarrierName = "Aspen";
                policy.Company.CompanyName = "Aspen MGA";
                policy.Company.ContactInfo.Add(new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Billing", PhoneNumber = "1(800)555-1234", EmailAddress = "underwriting@renissance.com" });
                policy.Company.ContactInfo.Add(new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Claims (Existing)", PhoneNumber = "1(800)555-2234", EmailAddress = "claims@renissance.com" });
                policy.Company.ContactInfo.Add(new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Claims (New)", PhoneNumber = "1(800)555-3234", EmailAddress = "newclaim@renissance.com" });
                policy.Company.ContactInfo.Add(new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "5501 LBJ Freeway", Address2 = "Suite 1200", City = "Dallas", State = "TX", PostalCode = "75240", County = "Dallas" }, ContactType = "Underwriting", PhoneNumber = "1(800)555-1234", EmailAddress = "underwriting@renissance.com" });
                policy.Company.ImageURL = "";
                policy.Company.ProductLine = "Personal";
                policy.Company.Product = "Private Auto";
                policy.Company.StateAbbreviation = "TX";
                policy.Company.Program = "Non-Standard";
                policy.CoveredUnits.Add(new PersonalAutoCoveredUnit { IndexNumber = 1, Address = new InsurCloud.Auth.Api.Models.Address { PostalCode = "75241" }, ModelYear = 2011, Make = "Dodge", VIN = "1D7RB1CT2BS505077", AssignedDriverNumber = 1, Description = "2011 Dodge" });
                policy.CoveredUnits[0].Coverages().Add(new InsurCloud.Auth.Api.Models.Coverage { CoverageCode = "BI:30/60:L:P", CovGroupName = "Bodily Injury", CovGroupAbbr = "BI", IndexNumber = 1, LimitDeductibleDescription = "$30,000 per person, $60,000 per accident", WrittenPremium = 333.00 });
                policy.CoveredUnits[0].Coverages().Add(new InsurCloud.Auth.Api.Models.Coverage { CoverageCode = "PD:25:L:P", CovGroupName = "Property Damage", CovGroupAbbr = "PD", IndexNumber = 2, LimitDeductibleDescription = "$25,000 per incident", WrittenPremium = 222.00 });
                policy.CoveredUnits.Add(new PersonalAutoCoveredUnit { IndexNumber = 2, Address = new InsurCloud.Auth.Api.Models.Address { PostalCode = "75241" }, ModelYear = 1997, Make = "Buick", VIN = "2G4WB52K3V1436641", AssignedDriverNumber = 2, Description = "1997 Buick" });
                policy.CoveredUnits[1].Coverages().Add(new InsurCloud.Auth.Api.Models.Coverage { CoverageCode = "BI:30/60:L:P", CovGroupName = "Bodily Injury", CovGroupAbbr = "BI", IndexNumber = 1, LimitDeductibleDescription = "$30,000 per person, $60,000 per accident", WrittenPremium = 222.00 });
                policy.CoveredUnits[1].Coverages().Add(new InsurCloud.Auth.Api.Models.Coverage { CoverageCode = "PD:25:L:P", CovGroupName = "Property Damage", CovGroupAbbr = "PD", IndexNumber = 2, LimitDeductibleDescription = "$25,000 per incident", WrittenPremium = 210.00 });
                policy.EffectiveDate = new DateTime(2014, 12, 11);
                policy.ExpirationDate = new DateTime(2014, 6, 11);
                HouseholdMember insured = new HouseholdMember { IndexNumber = 1, RelationToInsured = "Self", ContactInfo = new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "3225 Golfing Green Drive", City = "Dallas", State = "TX", PostalCode = "75234", County = "Dallas" }, EmailAddress = "mprice@insurcloud.com", PhoneNumber = "(214)240-8085" }, BirthDate = new DateTime(1974, 7, 3), Gender = "Male", MaritalStatus = "Married", FirstName = "Matt", LastName = "Price", SetupForENotification = "Yes", RatedAge = 40 };
                HouseholdMember jointInsured = new HouseholdMember { IndexNumber = 2, RelationToInsured = "Spouse", ContactInfo = new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "3225 Golfing Green Drive", City = "Dallas", State = "TX", PostalCode = "75234", County = "Dallas" }, EmailAddress = "", PhoneNumber = "" }, BirthDate = new DateTime(1971, 4, 30), Gender = "Female", MaritalStatus = "Married", FirstName = "Colleen", LastName = "Price", SetupForENotification = "No", RatedAge = 43 };
                jointInsured.Violations.Add(new InsurCloud.Auth.Api.Models.Violation { IndexNumber = 1, Points = 1, ViolationDescription = "Speeding, Generally", ViolationDate = new DateTime(2013, 3, 17) });
                jointInsured.Violations.Add(new InsurCloud.Auth.Api.Models.Violation { IndexNumber = 2, Points = 0, ViolationDescription = "SR-22 Filling", ViolationDate = new DateTime(2015, 2, 13) });
                policy.PrimaryNamedInsured = insured;
                policy.JointNamedInsured = jointInsured;
                policy.HouseholdMembers.Add(insured);
                policy.HouseholdMembers.Add(jointInsured);
                policy.HouseholdMembers.Add(new HouseholdMember { IndexNumber = 1, RelationToInsured = "Child", ContactInfo = new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "3225 Golfing Green Drive", City = "Dallas", State = "TX", PostalCode = "75234", County = "Dallas" }, EmailAddress = "", PhoneNumber = "" }, BirthDate = new DateTime(2000, 3, 1), Gender = "Female", MaritalStatus = "Single", FirstName = "Ella", LastName = "Price", SetupForENotification = "No", RatedAge = 15 });

                policy.PayPlan = "Installments";
                policy.PolicyTermType = "New";
                policy.PolicyUniqueId = "1234";
                policy.Producer = new ProducerView { AgencyInfo = new AgencyLocationView { AgencyId = 55555, DisplayName = "Bob's Agency", ImageURL = "" }, ContactInfo = new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "1423 Test Way", City = "Test", State = "TX", County = "Dallas", PostalCode = "75432" }, PhoneNumber = "555-444-1234", EmailAddress = "jmartin@agency.com" }, FirstName = "Joseph", LastName = "Martin", ProducerUserId = "123413415" };
                policy.Producer.AgencyInfo.ContactInfo.Add(new ContactView { Address = new InsurCloud.Auth.Api.Models.Address { Address1 = "1423 Test Way", City = "Test", State = "TX", County = "Dallas", PostalCode = "75432" }, ContactType = "Main", EmailAddress = "bobsagency@agency.com", PhoneNumber = "1(800)555-4444", Name = "Main Street" });

                return Ok(policy);
            }
        }


        public async Task<clsPolicyPPA> GetPolicyData(string id)
        {
            clsPolicyPPA pol;

            PolicyRepository policyRepo = new PolicyRepository("Renaissance");
            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                if (agencyUser != null)
                {
                    PolicyNumberContext ctx = new PolicyNumberContext();
                    PolicyNumber num = ctx.PolicyNumbers.Where(p => p.FormattedNumber() == id).FirstOrDefault();
                    if (num == null)
                    {
                        return null;
                    }
                    List<PolicyTransactionNumber> trans = ctx.Transactions.Where(p => p.PolicyUniqueId == num.PolicyUniqueId).ToList();
                    if (trans == null || trans.Count <= 0)
                    {
                        return null;
                    }

                    pol = await policyRepo.Load(agencyUser.Location.Agency.AgencyId, id);
                    
                }
                else
                {
                    throw new ArgumentException("Unable to locate current user's agency");
                }

            }
            else
            {
                results = policyRepo.Search(0, searchText).ToList();
            }


        }
        [Authorize]
        [HttpGet]
        [Route("v1/policy/{id}", Name = "policy")]
        public async Task<IHttpActionResult> GetPolicy(string id)
        {
            try
            {

                return GetTestPolicyView(id);            
                
            }
            catch
            {
                return InternalServerError();
            }
        }

        [Authorize]
        [HttpGet]
        [Route("v1/policies/{searchText}", Name="policySearch")]
        public async Task<IHttpActionResult> PolicySearch(string searchText)
        {
            try
            {
                List<InsurCloud.Auth.Api.Models.PolicySearchResult> results = new List<Models.PolicySearchResult>();
                //List<PolicySearchResult> results = TestResults();
                //return Ok(results.Where(p => p.PolicyNumber.ToUpper() == searchText.ToUpper() || p.InsuredFullName.ToUpper().Contains(searchText.ToUpper()) || p.InsuredPhoneNumber.ToUpper() == searchText.ToUpper()).ToList());
                List<CorePolicy.Model.PolicySearchResult> result = new List<CorePolicy.Model.PolicySearchResult>();
                result = await getSearchResults(searchText, 0);
                foreach (CorePolicy.Model.PolicySearchResult res in result)
                {
                    results.Add(MapResult(res));
                }
                return Ok(result);

            }
            catch
            {
                return InternalServerError();
            }
        }

        private async Task<List<CorePolicy.Model.PolicySearchResult>> getSearchResults(string searchText, int numberToReturn)
        {

            List<CorePolicy.Model.PolicySearchResult> results;
            PolicyRepository policyRepo = new PolicyRepository("Renaissance");
            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                if (agencyUser != null)
                {
                    results = policyRepo.Search(agencyUser.Location.Agency.AgencyId, searchText).ToList();
                    
                }
                else
                {
                    throw new ArgumentException("Unable to locate current user's agency");
                }

            }
            else
            {
                results = policyRepo.Search(0, searchText).ToList();
            }

            //if (searchText != "")
            //{
            //    results = results.Where(c => c.PolicyNumber.ToUpper() == searchText.ToUpper() || c.InsuredFullName.ToUpper().Contains(searchText.ToUpper()) || c.InsuredFirstName.ToUpper().Contains(searchText.ToUpper()) || c.InsuredLastName.ToUpper().Contains(searchText.ToUpper()) || c.InsuredPhoneNumber.ToUpper().Contains(searchText.ToUpper()) || c.InsuredEmailAddress.ToUpper().Contains(searchText.ToUpper())).ToList();
                
            //}

            if (numberToReturn > 0)
            {
                results = results.Take(numberToReturn).ToList();
            }
            return results;


        }       


        [Authorize]
        [HttpGet]
        [Route("v1/policies", Name = "policySearchAll")]
        public async Task<IHttpActionResult> PolicySearchAll()
        {
            try
            {
                List<InsurCloud.Auth.Api.Models.PolicySearchResult> results = new List<Models.PolicySearchResult>();
                //return Ok(results);
                List<CorePolicy.Model.PolicySearchResult> result = new List<CorePolicy.Model.PolicySearchResult>();
                result = await getSearchResults("", 0);
                foreach (CorePolicy.Model.PolicySearchResult res in result)
                {
                    results.Add(MapResult(res));
                }
                return Ok(results);
                
            }
            catch
            {
                return InternalServerError();
            }
        }

        private Models.PolicySearchResult MapResult(CorePolicy.Model.PolicySearchResult result)
        {
            InsurCloud.Auth.Api.Models.PolicySearchResult entity = new Models.PolicySearchResult();
            entity.CurrentAmountDue = result.CurrentDueAmount;
            entity.DueDate = result.CurrentDueDate;
            entity.InstallmentAmount = result.InstallmentAmount;
            entity.InstallmentFeeAmount = result.InstallmentFeeAmount;
            entity.CancellationDate = result.CancellationDate;
            entity.EffectiveDate = result.PolicyEffectiveDate;
            entity.InsuredFullName = result.InsuredFullName;
            entity.InsuredPhoneNumber = result.InsuredPhoneNumber;
            entity.IsPastDue = result.PastDueAmount >= 0;
            entity.PolicyNumber = result.PolicyNumber;
            entity.PolicyUniqueId = result.PolicyUniqueId;
            entity.PolicyStatus = result.PolicyStatus;
            entity.PolicyTermTypeCode = result.PolicyTermTypeCode;
            entity.AgencyId = result.AgencyOfRecordAgencyId;
            return entity;
        }

        private List<InsurCloud.Auth.Api.Models.PolicySearchResult> TestResults()
        {
            List<InsurCloud.Auth.Api.Models.PolicySearchResult> results = new List<InsurCloud.Auth.Api.Models.PolicySearchResult>();
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 123.54, DueDate = new DateTime(2015, 3, 12), DueDateFormatted = "03/12/2015", IsPastDue = false, PolicyNumber = "1234001234", EffectiveDate = new DateTime(2014, 12, 12), EffectiveDateFormatted = "12/12/2014", InsuredFullName = "Milton Price", InsuredPhoneNumber = "(214)240-8085", PolicyStatus = "Active" });
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 227.81, DueDate = new DateTime(2015, 2, 26), DueDateFormatted = "02/26/2015", IsPastDue = true, PolicyNumber = "2340012341", EffectiveDate = new DateTime(2015, 1, 15), EffectiveDateFormatted = "01/15/2015", InsuredFullName = "Jack Russell", InsuredPhoneNumber = "(972)065-0056", PolicyStatus = "Cancel Pending", CancellationDate = new DateTime(2015, 3, 15), CancellationDateFormatted = "03/15/2015" });
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 0.0, IsPastDue = false, PolicyNumber = "3400123412", EffectiveDate = new DateTime(2014, 11, 1), EffectiveDateFormatted = "11/01/2014", InsuredFullName = "Palma Granite", InsuredPhoneNumber = "(214)326-1648", PolicyStatus = "Expired" });
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 133.48, DueDate = new DateTime(2015, 3, 23), DueDateFormatted = "03/23/2015", IsPastDue = false, PolicyNumber = "4001234123", EffectiveDate = new DateTime(2014, 12, 23), EffectiveDateFormatted = "12/23/2014", InsuredFullName = "Arnold Palmer", InsuredPhoneNumber = "(476)652-0543", PolicyStatus = "Cancelled", CancellationDate = new DateTime(2015, 2, 15), CancellationDateFormatted = "02/15/2015" });
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 274.14, DueDate = new DateTime(2015, 3, 2), DueDateFormatted = "03/02/2015", IsPastDue = false, PolicyNumber = "0012341234", EffectiveDate = new DateTime(2015, 2, 2), EffectiveDateFormatted = "02/02/2015", InsuredFullName = "Jamie Foxx", InsuredPhoneNumber = "(817)220-8465", PolicyStatus = "Active" });
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 135.00, DueDate = new DateTime(2015, 3, 24), DueDateFormatted = "03/24/2015", IsPastDue = false, PolicyNumber = "0123412340", EffectiveDate = new DateTime(2015, 2, 24), EffectiveDateFormatted = "02/24/2015", InsuredFullName = "Abraham Lincolm", InsuredPhoneNumber = "(214)555-0000", PolicyStatus = "Active" });
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 214.01, DueDate = new DateTime(2015, 2, 18), DueDateFormatted = "02/18/2015", IsPastDue = true, PolicyNumber = "1234123400", EffectiveDate = new DateTime(2014, 10, 30), EffectiveDateFormatted = "10/30/2014", InsuredFullName = "Maya Rudolph", InsuredPhoneNumber = "(817)444-4321", PolicyStatus = "Cancel Pending", CancellationDate = new DateTime(2015, 3, 10), CancellationDateFormatted = "03/10/2015" });
            results.Add(new InsurCloud.Auth.Api.Models.PolicySearchResult { CurrentAmountDue = 321.45, DueDate = new DateTime(2015, 3, 11), DueDateFormatted = "03/11/2015", IsPastDue = false, PolicyNumber = "2341234001", EffectiveDate = new DateTime(2014, 11, 11), EffectiveDateFormatted = "11/11/2014", InsuredFullName = "Hally Barry", InsuredPhoneNumber = "(214)888-1234", PolicyStatus = "Active" });
            return results;
        }
    }
}
