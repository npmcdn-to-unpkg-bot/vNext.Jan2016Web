using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.Content != null)
                response.Content.Dispose();
            throw new SimpleHttpResponseException(response.StatusCode, content);
        }
    }

    public class SimpleHttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
        {
            StatusCode = statusCode;
        }
    }
    public class Program
    {
        static void Main()
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            var response = RequestToken();
            ShowResponse(response);
            Console.ReadLine();
            CallService(response.AccessToken, "identity4");
            Console.ReadLine();

            CallService(response.AccessToken, "apiv1/Identity4Auth");
            Console.ReadLine();

            CallService(response.AccessToken, "Sports/Work");
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
        static async void CallService(string token, string path)
        {
            try
            {
                var baseAddress = Constants.AspNetWebApiSampleApi;
                using (var client = new HttpClient
                {
                    BaseAddress = new Uri(baseAddress)
                })
                {
                    client.SetBearerToken(token);
                    HttpResponseMessage response = await client.GetAsync(path);
                    Console.WriteLine("StatusCode:{0}", response.StatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        "\n\nService claims:".ConsoleGreen();
                        Console.WriteLine(JArray.Parse(result));
                    }
                }

            }
            catch (HttpRequestException he)
            {
                
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.GetType() == typeof (HttpRequestException))
                {
                    HttpRequestException he = (HttpRequestException) e;
                    
                }
            }

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
