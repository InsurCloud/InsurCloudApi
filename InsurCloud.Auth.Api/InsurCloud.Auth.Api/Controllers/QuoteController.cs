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
                    AgencyUser agencyUser = await getCurrentAgencyUser(user);
                    if (agencyUser != null)
                    {
                        Quote q = await quoteRepo.Load(agencyUser.Location.Agency.AgencyId.ToString(), id);
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
                return Ok(result.Where(c => c.QuoteUniqueId == searchText || c.QuoteNumber == searchText || c.InsuredFullName.Contains(searchText) || c.InsuredFirstName.Contains(searchText) || c.InsuredLastName.Contains(searchText) || c.InsuredPhoneNumber.Contains(searchText) || c.InsuredEmailAddress.Contains(searchText)).ToList());
            }
            catch
            {
                return InternalServerError();
            }
        }

        private async Task<AgencyUser> getCurrentAgencyUser(ExtendedIdentityUser user)
        {
            
            if (user != null)
            {
                AgencyUser agencyUser = await _agencyRepo.GetAgencyUser(user.Id);
                if (agencyUser != null)
                {
                    return agencyUser;
                }
                else
                {
                    throw new ArgumentException("Unable to locate current user");
                }                
                
            }
            else
            {
                throw new ArgumentException("Severe error captured. Please log back in and retry");
            }
        }

        private async Task<List<QuoteSearchResult>> getSearchResults(string searchText, int numberToReturn, bool showAllAgency)
        {
            
            QuoteRepository quoteRepo = new QuoteRepository("Renaissance");
            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await getCurrentAgencyUser(user);
                if (agencyUser != null)
                {
                    var results = quoteRepo.SearchQuotes(agencyUser.Location.Agency.AgencyId, agencyUser.UserId, numberToReturn, showAllAgency).ToList();
                    if (searchText != "")
                    {
                        return results.Where(c => c.QuoteUniqueId == searchText || c.QuoteNumber == searchText || c.InsuredFullName.Contains(searchText) || c.InsuredFirstName.Contains(searchText) || c.InsuredLastName.Contains(searchText) || c.InsuredPhoneNumber.Contains(searchText) || c.InsuredEmailAddress.Contains(searchText)).ToList();
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

            return null;

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
                    agencyUser = await getCurrentAgencyUser(user);
                    if (agencyUser == null)
                    {
                        return BadRequest("Cannot find agency user information");
                    }
                }
                else
                {
                    return BadRequest("Cannot find agency user information");
                }

                Quote quoteView = new Quote();
                quoteView.QuoteUniqueId = Guid.NewGuid().ToString();
                quoteView.TermMonths = 6;
                quoteView.CarrierId = 1;
                quoteView.CarrierName = "Renaissance";
                quoteView.Product = "Private Auto";
                quoteView.ProductLine = "Peronsal";
                quoteView.ProgramCode = "242";
                quoteView.AgencyId = agencyUser.Location.Agency.AgencyId;
                quoteView.ProducerUserId = agencyUser.UserId;
                quoteView.CreatedByUserId = quoteView.ProducerUserId;
                quoteView.CreateDate = DateTime.Now;
                quoteView.ModifyDate = quoteView.CreateDate;
                quoteView.ModifiedByUserId = quoteView.ProducerUserId;
                quoteView.QuoteStatus = "Lead";
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
                drv.RelationToInsured = "Self";
                drv.IsPrimaryNamedInsured = true;
                drv.Number = 1;
                drv.PrimaryDriver = true;
                drv.BirthDate = DateTime.MinValue;
                drv.BirthDateFormatted = "";
                drv.PhysicalAddress.PostalCode = request.PostalCode;
                drv.PhysicalAddress.City = "Dallas";
                drv.PhysicalAddress.State = "TX";
                drv.mailingSameAsPhysical = true;
                drv.MailingAddress.State = "TX";
                drv.DiscountInfo.CurrentlyInsured = request.PriorCoverage;
                drv.DiscountInfo.Homeowner = request.Homeowner;
                drv.DiscountInfo.PriorRate = 0.00;
                drv.License.AgeFirstLicensed = 16;
                drv.DriverStatus = "Active";
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
                        newDrv.RelationToInsured = "Spouse";
                        newDrv.MaritalStatus = "M";
                    }
                    else
                    {
                        newDrv.RelationToInsured = "Other";
                        newDrv.MaritalStatus = "S";
                    }
                    newDrv.LivesWithPrimaryNamedInsured = true;
                    newDrv.IsPrimaryNamedInsured = false;
                    newDrv.Number = a;
                    newDrv.BirthDate = DateTime.MinValue;
                    newDrv.BirthDateFormatted = "";
                    newDrv.License.AgeFirstLicensed = 16;
                    newDrv.DriverStatus = "Active";

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
        public async Task<IHttpActionResult> SaveQuote(Quote quote)
        {

            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await getCurrentAgencyUser(user);
                if (agencyUser != null)
                {

                    quote.ProducerUserId = agencyUser.UserId;
                    foreach (Driver drv in quote.HouseholdMembers)
                    {
                        DateTime dt;
                        DateTime.TryParse(drv.BirthDateFormatted, out dt);
                        drv.BirthDate = dt;
                    }                    

                    ProgramInfo p = new ProgramInfo();
                    p.CarrierName = "Renaissance";
                    p.CompanyName = "Renaissance";
                    p.ProductLine = "Personal";
                    p.Product = "Private Passenger";
                    p.Program = "Classic";
                    p.TermMonths = 6;
                    p.ProgramId = 242;
                    p.StateAbbreviation = "TX";

                    List<Rate> rates = new List<Rate>();

                    if (quote.Coverages.Id == "Minimum")
                    {
                        PayPlan pp = new PayPlan("0:100:0.00");
                        rates.Add(SetupRate(p, pp, 425.00, 50.00, quote.Coverages.Id));

                        pp = new PayPlan("5:20:5.50");
                        rates.Add(SetupRate(p, pp, 472.00, 50.00, quote.Coverages.Id));
                    }
                    else if (quote.Coverages.Id == "Basic")
                    {
                        PayPlan pp = new PayPlan("0:100:0.00");
                        rates.Add(SetupRate(p, pp, 540.00, 50.00, quote.Coverages.Id));

                        pp = new PayPlan("5:20:5.50");
                        rates.Add(SetupRate(p, pp, 587.00, 50.00, quote.Coverages.Id));
                    }
                    else
                    {
                        PayPlan pp = new PayPlan("0:100:0.00");
                        rates.Add(SetupRate(p, pp, 753.00, 50.00, quote.Coverages.Id));

                        pp = new PayPlan("5:20:5.50");
                        rates.Add(SetupRate(p, pp, 801.00, 50.00, quote.Coverages.Id));
                    }

                    quote.Rates = rates;
                    
                    QuoteRepository quoteRepo = new QuoteRepository(quote.CarrierName);
                    string quoteNumber = await quoteRepo.Upsert(quote, agencyUser);
                    quote.QuoteNumber = quoteNumber;
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
        

        private Rate SetupRate(ProgramInfo p, PayPlan pp, double Premium, double Fees, string coverageLevel)
        {
            
            Rate rate = new Rate();
            
            rate.Program = p;
            rate.PayPlan = pp;
            rate.Premium = Premium;
            rate.Fees = Fees;
            rate.CoverageLevel = coverageLevel;
            rate.Installments = new List<Installment>();
            
            double premiumAndFees = rate.Premium + rate.Fees;

            rate.PayPlan.DownPaymentAmount = Math.Round((premiumAndFees) * ((double)rate.PayPlan.DownPaymentPercent / 100.00), 2);
            if (rate.PayPlan.NumberOfInstallments == 0)
            {
                rate.PayPlan.InstallmentAmount = 0.00;
            }
            else
            {
                rate.PayPlan.InstallmentAmount = Math.Round((premiumAndFees - rate.PayPlan.DownPaymentAmount) / rate.PayPlan.NumberOfInstallments);
            }
            
            rate.PayPlan.InstallmentPlusFeeAmount = rate.PayPlan.InstallmentAmount + rate.PayPlan.InstallmentFeeAmount;
            double lastInstallAmount = Math.Round(premiumAndFees - (rate.PayPlan.DownPaymentAmount + ((rate.PayPlan.NumberOfInstallments - 1) * rate.PayPlan.InstallmentAmount)), 2);

            for (int a = 0; a < rate.PayPlan.NumberOfInstallments; a++)
            {
                Installment i = new Installment();
                i.InstallmentNumber = a + 1;
                if (i.InstallmentNumber == 1)
                {
                    i.Amount = rate.PayPlan.DownPaymentAmount;
                    i.DueDate = DateTime.Now;
                    i.InstallmentFee = 0F;
                }
                else
                {
                    i.DueDate = DateTime.Now.AddDays(35);
                    if (i.InstallmentNumber > 1)
                    {
                        if (a == rate.PayPlan.NumberOfInstallments - 1)
                        {
                            i.Amount = lastInstallAmount;
                        }
                        else
                        {
                            i.Amount = rate.PayPlan.InstallmentAmount;
                        }
                        i.DueDate = i.DueDate.AddMonths(i.InstallmentNumber - 1);
                    }
                    i.InstallmentFee = rate.PayPlan.InstallmentFeeAmount;
                }

                rate.Installments.Add(i);
            }

            return rate;
        }
    
        [Authorize]
        [HttpPost]
        [Route("v1/bind", Name= "bindQuote")]
        public async Task<IHttpActionResult> BindQuote(Quote quote)
        {
            ExtendedIdentityUser user = await _authRepo.FindCurrentUser(User.Identity.GetUserName());
            if (user != null && user.UserType.UserTypeId == (int)UserTypes.Agency)
            {
                AgencyUser agencyUser = await getCurrentAgencyUser(user);
                if (agencyUser != null)
                {
                    PPAPolicy policy = new PPAPolicy();
                    policy.AgencyId = quote.AgencyId;
                    policy.BankInfo = quote.BankInfo;
                    policy.ChangeEffectiveDate = new DateTimeSortFormat();
                    policy.ChangeEffectiveDate.DateValue = DateTime.Now;
                    policy.Coverages = quote.Coverages;
                    policy.CoveredUnits = quote.CoveredUnits;
                    policy.CreateDate = DateTime.Now;
                    policy.CreatedByUserId = agencyUser.UserId;
                    policy.CreditCard = quote.CreditCard;
                    policy.DownPaymentInfo = quote.DownPaymentInfo;
                    policy.EffectiveDate = quote.EffectiveDate;
                    policy.eNotify = quote.eNotify;
                    policy.eSignature = quote.eSignature;
                    policy.HouseholdMembers = quote.HouseholdMembers;
                    policy.Insured = quote.Insured;
                    policy.Lienholders = quote.Lienholders;
                    policy.ModifiedByUserId = agencyUser.UserId;
                    policy.ModifyDate = DateTime.Now;
                    policy.PolicyIssuanceDate = new DateTimeSortFormat();
                    policy.PolicyIssuanceDate.DateValue = DateTime.Now;
                    policy.PolicyStatus = "Issued";
                    policy.PolicyUniqueId = Guid.NewGuid().ToString();
                    policy.ProducerUserId = agencyUser.UserId;
                    policy.QuoteNumber = quote.QuoteNumber;
                    policy.QuoteUniqueId = quote.QuoteUniqueId;
                    policy.Rate = quote.Rate;
                    policy.RateDate = quote.RateDate;
                    policy.TermMonths = quote.TermMonths;
                    policy.TransactionNumber = 1;
                    policy.UnderwritingQuestions = quote.UnderwritingQuestions;

                    PolicyRepository policyRepo = new PolicyRepository(quote.CarrierName.ToLower());
                    policy = await policyRepo.AgentPPAPolicyUpsert(policy, agencyUser);
                    return Ok(policy);
                }
            }
            return InternalServerError();
        }
    }
}
