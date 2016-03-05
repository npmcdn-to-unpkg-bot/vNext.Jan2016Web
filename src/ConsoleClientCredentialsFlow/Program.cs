using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.Extensions;
using Newtonsoft.Json.Linq;
using WebApplication1.IdentityServerClients.Configuration;

namespace ConsoleClientCredentialsFlow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var response = RequestToken();
            ShowResponse(response);
            Console.ReadLine();
            CallService(response.AccessToken, "identity4");
            Console.ReadLine();
            CallService(response.AccessToken,"Sports/Work");
            Console.ReadLine();
        }

        static TokenResponse RequestToken()
        {
            var client = new TokenClient(
                Constants.TokenEndpoint,
                "client",
                "secret");

            return client.RequestClientCredentialsAsync("api1").Result;
        }
        static void CallService(string token, string path)
        {
            var baseAddress = Constants.AspNetWebApiSampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = client.GetStringAsync(path).Result;

            "\n\nService claims:".ConsoleGreen();
            Console.WriteLine(JArray.Parse(response));
        }
        private static void ShowResponse(TokenResponse response)
        {
            if (!response.IsError)
            {
                "Token response:".ConsoleGreen();
                Console.WriteLine(response.Json);

                if (response.AccessToken.Contains("."))
                {
                    "\nAccess Token (decoded):".ConsoleGreen();

                    var parts = response.AccessToken.Split('.');
                    var header = parts[0];
                    var claims = parts[1];

                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
                }
            }
            else
            {
                if (response.IsHttpError)
                {
                    "HTTP error: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorStatusCode);
                    "HTTP error reason: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorReason);
                }
                else
                {
                    "Protocol error response:".ConsoleGreen();
                    Console.WriteLine(response.Json);
                }
            }
        }
    }
}
