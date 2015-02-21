using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Rate.Models;
using RatingTester;

namespace RatingTester
{
    class Program
    {
        static void Main(string[] args)
        {

            RunAsync().Wait();
            
        }

        static async Task RunAsync()
        {
            Policy pol = ClientHelper.GetPolicyObject();
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri("https://imperial.insurcloud.com/");                
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));                

                DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(typeof(Policy));
                MemoryStream ms = new MemoryStream();
                jsonSer.WriteObject(ms, pol);
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                StringContent content = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage resp = await client.PostAsync("https://imperial.insurcloud.com/ppa/rate?subscription-key=e904337757e546d4a34f66876cb9411d", content); //
                if (resp.IsSuccessStatusCode)
                {
                    Quote quote = await resp.Content.ReadAsAsync<Quote>();                    
                    Console.WriteLine("Premium = " + quote.Options[0].FullTermPremium.ToString());
                }
                else
                {
                    string failureMsg = "HTTP Status: " + resp.StatusCode.ToString() + " - Reason: " + resp.ReasonPhrase;
                    Console.WriteLine(failureMsg);
                }
                Console.ReadLine();
            }
        }
    }
}
