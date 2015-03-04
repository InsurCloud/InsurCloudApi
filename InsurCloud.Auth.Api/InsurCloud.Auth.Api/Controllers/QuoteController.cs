using CoreCommon.Attributes;
using CoreQuote.Model;
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
        public static List<Quote> quotes = new List<Quote>();

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

                Quote quote = quotes.Where(c => c.QuoteUniqueId == id).FirstOrDefault();
                if (quote == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(quote);
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
                var resultSet = quotes.Where(c => c.QuoteUniqueId == searchText || c.Insured.FirstName.Contains(searchText) || c.Insured.LastName.Contains(searchText) || c.Insured.PhoneNumber.Contains(searchText) || c.Insured.EmailAddress.Contains(searchText)).ToList();
                List<QuoteSearchResult> result = getSearchResults(resultSet);
                return Ok(result);
            }
            catch
            {
                return InternalServerError();
            }
        }

        private List<QuoteSearchResult> getSearchResults(List<Quote> items)
        {
            List<QuoteSearchResult> result = new List<QuoteSearchResult>();

            foreach (Quote view in quotes)
            {
                QuoteSearchResult n = new QuoteSearchResult();
                n.QuoteNumber = view.QuoteUniqueId;
                n.InsuredFullName = string.Concat(view.Insured.FirstName, " ", view.Insured.LastName);
                n.InsuredPhoneNumber = view.Insured.PhoneNumber;
                n.LastRateDate = view.RateDate;
                n.LastRateDateFormatted = n.LastRateDate.ToString("MM/dd/yyyy");
                n.QuoteStatus = view.QuoteStatus;
                n.RateAmount = 0.00;
                if (view.Rates != null && view.Rates.Count > 0)
                {
                    n.RateAmount = view.Rates[0].Premium + view.Rates[0].Fees;
                }
                result.Add(n);
            }

            return result;

        }       

        [Authorize]
        [HttpGet]
        [Route("v1/quotes", Name = "quoteSearchAll")]
        public async Task<IHttpActionResult> QuoteSearchAll()
        {
            try
            {

                List<QuoteSearchResult> result = new List<QuoteSearchResult>();
                result = getSearchResults(quotes);
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
                Quote quoteView = new Quote();
                quoteView.QuoteUniqueId = Guid.NewGuid().ToString();
                quoteView.QuoteStatus = "Lead";
                quoteView.EffectiveDate = DateTime.Now;
                quoteView.EffectiveDateFormatted = quoteView.EffectiveDate.ToString("MM/dd/yyyy");
                quoteView.RateDate = DateTime.Now;
                quoteView.RateDateFormatted = quoteView.RateDate.ToString("MM/dd/yyyy");
                quoteView.Insured = new Insured();
                quoteView.Insured.FirstName = request.Insured.FirstName;
                quoteView.Insured.LastName = request.Insured.LastName;
                quoteView.Insured.PhoneNumber = request.Insured.PhoneNumber;
                quoteView.Insured.EmailAddress = request.Insured.EmailAddress;
                quoteView.Insured.Address.PostalCode = request.PostalCode;
                quoteView.Insured.Address.City = "Dallas";
                quoteView.Insured.Address.State = "TX";
                quoteView.Insured.DiscountInfo.CurrentlyInsured = request.PriorCoverage;
                quoteView.Insured.DiscountInfo.Homeowner = request.Homeowner;
                if (request.Married)
                {
                    quoteView.Insured.MaritalStatus = "M";
                }
                quoteView.Vehicles = new List<Vehicle>();
                for (int i = 0; i < request.NumberOfVehicles; i++)
                {
                    Vehicle veh = new Vehicle();
                    veh.Number = 1;
                    veh.CommuteMiles = 10;
                    veh.CommuteDaysPerWeek = 5;
                    veh.GaragingZipCode = request.PostalCode;
                    veh.PhotoSrc = "img/IC_finalBUG.png";
                    veh.Drivers = new List<Driver>();
                    quoteView.Vehicles.Add(veh);
                    if (i == 0)
                    {
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
                        if (request.Married)
                        {
                            drv.MaritalStatus = "M";
                        }
                        veh.Drivers.Add(drv);
                        quoteView.Drivers = new List<Driver>();
                        quoteView.Drivers.Add(drv);
                    }
                }
                quotes.Add(quoteView);
                QuoteSearchResult quote = new QuoteSearchResult();
                quote.InsuredFullName = string.Concat(quoteView.Insured.FirstName, " ", quoteView.Insured.LastName);
                quote.QuoteNumber = quoteView.QuoteUniqueId;
                quote.QuoteStatus = quoteView.QuoteStatus;
                return Ok(quote);
            }
            catch
            {
                return InternalServerError();
            }
            
        }

        [Authorize]
        [HttpPost]
        [Route("v1/quote", Name = "saveQuote")]
        public async Task<IHttpActionResult> SaveQuote(Quote quote)
        {
            quote.QuoteStatus = "Quote";

            foreach (Driver drv in quote.Drivers)
            {
                DateTime dt;
                DateTime.TryParse(drv.BirthDateFormatted, out dt);
                drv.BirthDate = dt;       
            }
            foreach (Vehicle veh in quote.Vehicles)
            {
                foreach (Driver drv in veh.Drivers)
                {
                    DateTime dt;
                    DateTime.TryParse(drv.BirthDateFormatted, out dt);
                    drv.BirthDate = dt;                    
                }
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

            for (int i = 0; i < quotes.Count; i++)
            {
                if (quotes[i].QuoteUniqueId == quote.QuoteUniqueId)
                {
                    quotes[i] = quote;
                }
            }

            return Ok(quote);
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
    
    }
}
