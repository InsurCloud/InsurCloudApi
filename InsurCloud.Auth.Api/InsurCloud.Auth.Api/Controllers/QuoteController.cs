using CoreCommon.Attributes;
using CoreAgency.Repository;
using CoreQuote.Model;
using CoreQuote.Repository;
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
using CorePolicy.Repository;
using CorePolicy.Model;
using CoreImaging;
using CoreAudit.Model;
using CorePolicy.Service;

namespace InsurCloud.Auth.Api.Controllers
{
    [RequireHttps]
    [RoutePrefix("api/Quote")]
    public class QuoteController : ApiController
    {
        public AuthRepository _authRepo = new AuthRepository();
        public AgencyRepository _agencyRepo = new AgencyRepository();

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
                QuoteRepository quoteRepo = new QuoteRepository("renaissance");
                ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
                if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
                {
                    AgencyUser agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                    if (agencyUser != null)
                    {
                        PPAQuote q = await quoteRepo.Load(agencyUser.Location.Agency.AgencyId.ToString(), id);
                        return Ok(q);
                    }
                    else
                    {
                        return BadRequest("Unable to locate Agency User");
                    }
                }
                else
                {
                    return BadRequest("Unable to locate User");
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
        public async Task<IHttpActionResult> QuoteSearch(string searchText, bool showAllAgency = false)
        {
            try
            {
                
                List<QuoteSearchResult> result = await getSearchResults(searchText, 20, showAllAgency);
                return Ok(result);
            }
            catch
            {
                return InternalServerError();
            }
        }
       

        private async Task<List<QuoteSearchResult>> getSearchResults(string searchText, int numberToReturn, bool showAllAgency)
        {
            
            QuoteRepository quoteRepo = new QuoteRepository("Renaissance");
            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                if (agencyUser != null)
                {
                    var results = quoteRepo.SearchQuotes(agencyUser.Location.Agency.AgencyId, agencyUser.UserId, numberToReturn, showAllAgency).ToList();
                    if (searchText != "")
                    {
                        return results.Where(c => c.QuoteUniqueId == searchText || c.QuoteNumber == searchText || c.InsuredFullName.ToUpper().Contains(searchText.ToUpper()) || c.InsuredFirstName.ToUpper().Contains(searchText.ToUpper()) || c.InsuredLastName.ToUpper().Contains(searchText.ToUpper()) || c.InsuredPhoneNumber.ToUpper().Contains(searchText.ToUpper()) || c.InsuredEmailAddress.ToUpper().Contains(searchText.ToUpper())).ToList();
                    }
                    return results;
                }
                else
                {
                    throw new ArgumentException("Unable to locate current user's agency");
                }

            }
            else
            {
                throw new ArgumentException("Can not identify agency user");
            }


        }       

        [Authorize]
        [HttpGet]
        [Route("v1/quotes", Name = "quoteSearchAll")]
        public async Task<IHttpActionResult> QuoteSearchAll(bool showAllAgency)
        {
            try
            {

                List<QuoteSearchResult> result = new List<QuoteSearchResult>();
                result = await getSearchResults("", 0, showAllAgency);
                return Ok(result);
            }
            catch
            {
                return InternalServerError();
            }
        }

        [Authorize]
        [HttpPost]
        [Route("v1/newQuote", Name= "startNewQuote")]
        public async Task<IHttpActionResult> StartNewQuote(NewQuoteRequest request)
        {
            try
            {
                AgencyUser agencyUser;
                ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
                if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
                {
                    agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                    if (agencyUser == null)
                    {
                        return BadRequest("Cannot find agency user information");
                    }
                }
                else
                {
                    return BadRequest("Cannot find agency user information");
                }

                PPAQuote quoteView = new PPAQuote();
                quoteView.Coverages.Id = "MINIMUM";
                quoteView.PolicyTermTypeCode = "N"; //New Business                
                quoteView.DownPaymentInfo.PayPlanCode = "205";
                quoteView.QuoteUniqueId = Guid.NewGuid().ToString();
                quoteView.TermMonths = 6;
                quoteView.Program.CarrierName = "Renaissance";
                quoteView.Program.CompanyName = "Renaissance";
                quoteView.Program.ProductLine = "Personal";
                quoteView.Program.Product = "Private Passenger";
                quoteView.Program.ProductCode = "2";
                quoteView.Program.Program = "Classic";
                quoteView.Program.TermMonths = 6;
                quoteView.Program.ProgramCode = "242";
                quoteView.Program.ProgramId = 242;
                quoteView.Program.StateAbbreviation = "TX";
                quoteView.Program.StateCode = "42";                
                quoteView.AgencyId = agencyUser.Location.Agency.AgencyId;
                quoteView.ProducerUserId = agencyUser.UserId;
                quoteView.CreatedByUserId = quoteView.ProducerUserId;
                quoteView.CreateDate = DateTime.Now;
                quoteView.ModifyDate = quoteView.CreateDate;
                quoteView.ModifiedByUserId = quoteView.ProducerUserId;
                quoteView.QuoteStatus = "LEAD";
                quoteView.EffectiveDate.DateValue = DateTime.Now;
                quoteView.RateDate.DateValue = DateTime.Now;
                
                quoteView.Insured = new Insured();
                quoteView.Insured.FirstName = request.Insured.FirstName;
                quoteView.Insured.LastName = request.Insured.LastName;
                quoteView.Insured.PhoneNumber = request.Insured.PhoneNumber;
                quoteView.Insured.EmailAddress = request.Insured.EmailAddress;
                quoteView.Insured.Address.PostalCode = request.PostalCode;
                quoteView.Insured.Address.City = request.ZipCode.City;
                quoteView.Insured.Address.State = request.ZipCode.State;
                quoteView.Insured.DiscountInfo.CurrentlyInsured = request.PriorCoverage;
                quoteView.Insured.DiscountInfo.Homeowner = request.Homeowner;
                if (request.Married)
                {
                    quoteView.Insured.MaritalStatus = "M";
                }
                else
                {
                    quoteView.Insured.MaritalStatus = "S";
                }

                
                for (int i = 0; i < request.NumberOfVehicles; i++)
                {
                    Vehicle veh = new Vehicle();
                    veh.Number = 1;
                    veh.CommuteMiles = 10;
                    veh.CommuteDaysPerWeek = 5;
                    veh.GaragingZipCode = request.PostalCode;
                    veh.PhotoSrc = "img/IC_finalBUG.png";
                    quoteView.CoveredUnits.Add(veh);                    
                }
                
                Driver drv = new Driver();
                drv.FirstName = request.Insured.FirstName;
                drv.LastName = request.Insured.LastName;
                drv.EmailAddress = request.Insured.EmailAddress;
                drv.PhoneNumber = request.Insured.PhoneNumber;
                drv.RelationToInsured = "SELF";
                drv.IsPrimaryNamedInsured = true;
                drv.Number = 1;
                drv.PrimaryDriver = true;
                drv.BirthDate = DateTime.MinValue;
                drv.BirthDateFormatted = "";
                drv.PhysicalAddress.PostalCode = request.PostalCode;
                USPSAddress.CityStateLookup lookupService = new USPSAddress.CityStateLookup();
                
                if (request.PostalCode.Length == 5)
                {
                    int zipCode = 0;
                    int.TryParse(request.PostalCode.Substring(0, 5), out zipCode);
                    if (zipCode > 0)
                    {
                        USPSAddress.ZipCode zip = await lookupService.LookupCityStateByZipCode(zipCode);
                        if (zip != null)
                        {
                            drv.PhysicalAddress.City = zip.City;
                            drv.PhysicalAddress.State = zip.State;
                        }
                        else
                        {
                            drv.PhysicalAddress.City = "";
                            drv.PhysicalAddress.State = "TX";
                        }
                    }
                }
                else
                {
                    drv.PhysicalAddress.City = "";
                    drv.PhysicalAddress.State = "TX";
                }
                
                
                
                drv.mailingSameAsPhysical = true;
                drv.MailingAddress.Address1 = drv.PhysicalAddress.Address1;
                drv.MailingAddress.Address2 = drv.PhysicalAddress.Address2;
                drv.MailingAddress.City = drv.PhysicalAddress.City;
                drv.MailingAddress.State = drv.PhysicalAddress.State;
                drv.MailingAddress.PostalCode = drv.PhysicalAddress.PostalCode;
                drv.MailingAddress.County = drv.PhysicalAddress.County;

                drv.DiscountInfo.CurrentlyInsured = request.PriorCoverage;
                drv.DiscountInfo.Homeowner = request.Homeowner;
                drv.DiscountInfo.PriorRate = 0.00;
                drv.License.AgeFirstLicensed = 16;
                drv.License.DriversLicenseState = drv.MailingAddress.State;
                drv.License.DriversLicenseStatus = "VALID";
                drv.DriverStatus = "ACTIVE";
                drv.MaritalStatus = quoteView.Insured.MaritalStatus;

                if (request.Married)
                {
                    drv.MaritalStatus = "M";
                }
                else
                {
                    quoteView.Insured.MaritalStatus = "S";
                }

                quoteView.HouseholdMembers.Add(drv);

                for (int a = 1; a < request.NumberOfDrivers; a++)
                {
                    Driver newDrv = new Driver();
                    if (a == 2 && quoteView.Insured.MaritalStatus == "M")
                    {
                        newDrv.RelationToInsured = "SPOUSE";
                        newDrv.MaritalStatus = "M";
                    }
                    else
                    {
                        newDrv.RelationToInsured = "OTHER";
                        newDrv.MaritalStatus = "S";
                    }
                    newDrv.LivesWithPrimaryNamedInsured = true;
                    newDrv.IsPrimaryNamedInsured = false;
                    newDrv.Number = a;
                    newDrv.BirthDate = DateTime.MinValue;
                    newDrv.BirthDateFormatted = "";
                    newDrv.License.AgeFirstLicensed = 16;
                    newDrv.License.DriversLicenseState = 
                    newDrv.DriverStatus = "ACTIVE";

                    quoteView.HouseholdMembers.Add(newDrv);                    
                }
                
                quoteView.UnderwritingQuestions = new List<UnderwritingQuestion>();
                quoteView.UnderwritingQuestions.Add(new UnderwritingQuestion { QuestionText = "This is question number one?" });
                quoteView.UnderwritingQuestions.Add(new UnderwritingQuestion { QuestionText = "This is question number two?" });
                quoteView.UnderwritingQuestions.Add(new UnderwritingQuestion { QuestionText = "This is question number three?" });

                QuoteRepository repo = new QuoteRepository("renaissance");
                quoteView.QuoteNumber = await repo.Upsert(quoteView, agencyUser);
                QuoteSearchResult quote = new QuoteSearchResult();
                quote.InsuredFullName = string.Concat(quoteView.Insured.FirstName, " ", quoteView.Insured.LastName);
                quote.QuoteUniqueId = quoteView.QuoteUniqueId;
                quote.QuoteStatus = quoteView.QuoteStatus;
                quote.QuoteNumber = quoteView.QuoteNumber;

                
                return Ok(quote);
            }
            catch
            {
                return InternalServerError();
            }
            
        }

        [Authorize]
        [HttpGet]
        [Route("v1/coveragedefaults/", Name = "GetCoverageDefaults")]
        public async Task<IHttpActionResult> GetCoverageDefaults()
        {
            try
            {
                QuoteRepository quoteRepo = new QuoteRepository("renaissance");
                return Ok(await quoteRepo.GetCoverageDefaults());
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            
        }

        [Authorize]
        [HttpPost]
        [Route("v1/quote", Name = "saveQuote")]
        public async Task<IHttpActionResult> SaveQuote(PPAQuote quote)
        {
            
            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                if (agencyUser != null)
                {
                    if (quote.QuoteStatus != "ISSUED")
                    {
                        quote.ProducerUserId = agencyUser.UserId;
                        foreach (Driver drv in quote.HouseholdMembers)
                        {
                            DateTime dt;
                            DateTime.TryParse(drv.BirthDateFormatted, out dt);
                            drv.BirthDate = dt;
                            if (drv.RelationToInsured.ToUpper() == "SELF")
                            {
                                DateTime peDt;
                                DateTime.TryParse(drv.DiscountInfo.PriorExpirationDateFormatted, out peDt);
                                drv.DiscountInfo.PriorExpirationDate = peDt;
                                quote.Insured.DiscountInfo = drv.DiscountInfo;
                                quote.Insured.Gender = drv.Gender;
                                quote.Insured.MaritalStatus = drv.MaritalStatus;

                            }
                        }

                        if (quote.QuoteNumber == "")
                        {
                            QuoteRepository quoteRepo = new QuoteRepository(quote.Program.CarrierName);
                            string quoteNumber = await quoteRepo.Upsert(quote, agencyUser);
                            quote.QuoteNumber = quoteNumber;
                        }


                        quote.Rates = new List<Rate>();
                        //quote.DownPaymentInfo.PayPlanCode = "100";
                        PayPlan pp = new PayPlan("0:100:0.00", "100");
                        quote = (PPAQuote)PolicyRatingService.RatePolicy((PPAPolicy)quote, pp, quote.Coverages.Id);

                        if (!quote.RatingStatusMessage.Contains("NEI"))
                        {
                            //quote.DownPaymentInfo.PayPlanCode = "205";
                            pp = new PayPlan("5:20:5.50", "205");
                            quote = (PPAQuote)PolicyRatingService.RatePolicy((PPAPolicy)quote, pp, quote.Coverages.Id);

                        }
                    }
                    
                    QuoteRepository quoteRepo2 = new QuoteRepository(quote.Program.CarrierName);
                    string quoteNumber2 = await quoteRepo2.Upsert(quote, agencyUser);
                    
                    return Ok(quote);
                }
                else
                {
                    return BadRequest("Unable to locate agency user");
                }
            }
            else
            {
                return BadRequest("Unable to locate user");
            }
        }
        

        
        [Authorize]
        [HttpPost]
        [Route("v1/issuecoverage", Name= "issueCoverage")]
        public async Task<IHttpActionResult> IssueCoverage(PPAQuote quote)
        {
            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                if (agencyUser != null)
                {
                    //PPAPolicy policy = (PPAPolicy)quote;
                    quote.ChangeEffectiveDate = new DateTimeSortFormat();
                    quote.ChangeEffectiveDate.DateValue = DateTime.Now;
                    quote.ModifiedByUserId = agencyUser.UserId;
                    quote.ModifyDate = DateTime.Now;
                    quote.PolicyIssuanceDate = new DateTimeSortFormat();
                    quote.PolicyIssuanceDate.DateValue = DateTime.Now;
                    quote.PolicyStatus = "ISSUED";
                    quote.PolicyUniqueId = Guid.NewGuid().ToString();
                    quote.ProducerUserId = agencyUser.UserId;
                    quote.TransactionNumber = 1;
                    
                    try
                    {
                        PolicyRepository policyRepo = new PolicyRepository(quote.Program.CarrierName.ToLower());
                        quote = (PPAQuote)(await policyRepo.AgentPPAPolicyUpsert((PPAPolicy)quote, agencyUser));
                        quote = (PPAQuote)(await PolicyBindingService.BindPolicy((PPAPolicy)quote, agencyUser.UserId));
                        if (quote.PolicyStatus == "ISSUED" && quote.PolicyNumber != "")
                        {
                            quote.QuoteStatus = "ISSUED";                                                        
                            return await SaveQuote(quote);                                                 
                        }
                        else
                        {
                            return InternalServerError();
                        }
                        
                    }
                    catch
                    {

                    }
                    
                }
            }
            return InternalServerError();
        }


        [Authorize]
        [HttpPost]
        [Route("v1/validateupload", Name="validateUpload")]
        public async Task<IHttpActionResult> ValidateUpload(PPAQuote quote)
        {
            return await SaveQuote(quote);
        }

        [Authorize]
        [HttpPost]
        [Route("v1/paymentsetup", Name = "paymentSetup")]
        public async Task<IHttpActionResult> PaymentSetup(PPAQuote quote)
        {
            //TODO: Send Authorize call to Payment Process if it is a CC payment
            //TODO: Save Payment Details
            return Ok();

        }

        [Authorize]
        [HttpGet]
        [Route("v1/auditUpload/{policyUniqueID}", Name = "auditUpload")]
        public async Task<IHttpActionResult> AuditUpload(string policyUniqueID)
        {
            List<CoreAudit.Model.AuditItem> audits = new List<CoreAudit.Model.AuditItem>();
            audits.Add(new CoreAudit.Model.AuditItem { AuditType = "Number of Violations", CreateDate = DateTime.Now, Status = "Open", PolicyUniqueId = policyUniqueID });
            return Ok(audits);
        }

        [Authorize]
        [HttpGet]
        [Route("v1/generateDocuments/{policyUniqueID}", Name = "generateDocuments")]
        public async Task<IHttpActionResult> GenerateDocuments(string policyUniqueID)
        {
            List<PolicyImage> images = new List<PolicyImage>();
            var ctx = new CoreCommon.Context.CommonContext();
            try
            {                
                
                var docTypes = ctx.DocumentTypes.ToList();
                foreach (DocumentType dt in docTypes)
                {
                    var doc = new PolicyImage { PolicyUniqueId = policyUniqueID, DocumentType = dt, AddedByUser = User.Identity.GetUserId(), AddedDate = DateTime.Now, FilePath = "test.com", Status = "Generated" };
                    images.Add(doc);
                }            
            }
            catch
            {

            }
            finally
            {
                ctx.Dispose();
            }
            
            return Ok(images);
        }

        [Authorize]
        [HttpGet]
        [Route("v1/sendNewPolicyNotifications/{policyUniqueID}", Name = "sendNewPolicyNotifications")]
        public async Task<IHttpActionResult> SendNewPolicyNotifications(string policyUniqueID)
        {
            //Look up policy
            List<Notification> notifications = new List<Notification>();

            var policyRepo = new PolicyRepository("Renaissance");
            try
            {
                ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
                if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
                {
                    AgencyUser agencyUser = await _agencyRepo.getCurrentAgencyUser(user);
                    if (agencyUser != null)
                    {
                        var pol = await policyRepo.Load(agencyUser.Location.Agency.AgencyId.ToString(), policyUniqueID);
                        notifications.Add(new Notification { PolicyUniqueID = policyUniqueID, EmailAddress = pol.Insured.EmailAddress, NotificationSentDate = DateTime.Now, NotificationSentType = "Email", NotificationType = "Welcome Email" });
                    }
                }
                return Ok(notifications);
            }
            catch
            {
                return InternalServerError();
            }
            finally
            {
                policyRepo = null;
            }
            
        }




    }
}
