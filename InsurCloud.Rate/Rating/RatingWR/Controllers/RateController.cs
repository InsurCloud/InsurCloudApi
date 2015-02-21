using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CorPolicy;
using System.Data;
using Helpers;
using RatingWR.QuoteService;
using RatingWR.Models;
using Helpers.Model;


namespace RatingWR.Controllers
{
    public class RateController : ApiController
    {
        
        private HttpResponseMessage GetHeaderData()
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, Request.Headers.GetValues("X-Forwarded-For"));
            //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "{ uri=" + Request.RequestUri.AbsoluteUri + ", header=" + Request.Headers.Referrer + "}");
            return response;
        }

        [RequireHttps]
        public HttpResponseMessage GetPolicy()
        {
            Policy policy = QuoteExchangeHelpers.GetPolicy();
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, policy);
            return response;
        }
                
        [RequireHttps]
        public HttpResponseMessage Post(Policy policy)
        {
            try
            {
                if (policy != null)
                {
                    if (ModelState.IsValid)
                    {
                        HttpResponseMessage response = ConvertQuote(policy);                         
                        return response;
                    }
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            
        }
        
        private HttpResponseMessage ConvertQuote(Policy policy)
        {            
            
            HttpResponseMessage response = null;

            try
            {
                CorPolicy.clsPolicyPPA pol = QuoteExchangeHelpers.MapPolicy(policy);

                pol = RunQuickValidation(pol);

                string statusMessage = "";                
                if (QuoteExchangeHelpers.IsEnoughToRate(pol, out statusMessage))
                {

                    pol = ValidateFullRisk(pol);


                    pol = Rate(pol);                    

                    Quote quote = QuoteExchangeHelpers.MapQuote(pol);
                    response = Request.CreateResponse(HttpStatusCode.OK, quote);
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, statusMessage);
                }

            }
            catch(Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {                
            }

            return response;
        }

        private static clsPolicyPPA Rate(CorPolicy.clsPolicyPPA pol)
        {
            QuoteServiceClient quoteClient = new QuoteServiceClient();
            pol = quoteClient.QuotePersonalAuto(pol);
            quoteClient.Close();
            quoteClient = null;
            return pol;
        }

        private static clsPolicyPPA ValidateFullRisk(CorPolicy.clsPolicyPPA pol)
        {
            QuoteServiceClient quoteClient = new QuoteServiceClient();
            pol = quoteClient.ValidRisk(pol);
            quoteClient.Close();
            quoteClient = null;
            return pol;
        }

        private static clsPolicyPPA RunQuickValidation(clsPolicyPPA pol)
        {
            try
            {
                QuoteServiceClient quoteClient = new QuoteServiceClient();
                pol = quoteClient.EnoughToRate(pol);
                quoteClient.Close();
                quoteClient = null;
                return pol;
            }
            catch
            {
                return null;
            }
        }        
        
        
    }
}
