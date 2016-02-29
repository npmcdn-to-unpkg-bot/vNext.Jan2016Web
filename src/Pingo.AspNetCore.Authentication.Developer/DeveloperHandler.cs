using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.Extensions.Logging;

namespace Pingo.AspNetCore.Authentication.Developer
{
    public class DeveloperHandler: RemoteAuthenticationHandler<DeveloperOptions>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal const string StateCookie = "__DeveloperState";
        private const string AuthenticationEndpoint = "/Developer/Home/Authenticate?accessToken=";

        private readonly HttpClient _httpClient;

        public string RequestTokenCookie = "__RequestToken";

        public DeveloperHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected override async Task<AuthenticateResult> HandleRemoteAuthenticateAsync()
        {
            AuthenticationProperties properties = null;
            var query = Request.Query;
            var protectedRequestToken = Request.Cookies[StateCookie];

            var requestToken = DeveloperOptions.StateDataFormat.Unprotect(protectedRequestToken);

            if (requestToken == null)
            {
                return AuthenticateResult.Failed("Invalid state cookie.");
            }

            properties = requestToken.Properties;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };

            Response.Cookies.Delete(StateCookie, cookieOptions);
            return AuthenticateResult.Failed("Missing oauth_token");
            //   var accessToken = await ObtainAccessTokenAsync(Options.ConsumerKey, Options.ConsumerSecret, requestToken, oauthVerifier);

        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var properties = new AuthenticationProperties(context.Properties)
            {
                 
            };

            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = CurrentUri;
            }


            var callBackUri = BuildRedirectUri(Options.CallbackPath);
            var requestToken = await ObtainRequestTokenAsync(callBackUri, properties);
            var developerAuthenticationEndpoint = AuthenticationEndpoint + requestToken.Token;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };

            var data = DeveloperOptions.StateDataFormat.Protect(requestToken);
            Response.Cookies.Append(StateCookie, data, cookieOptions);

            var redirectContext = new DeveloperRedirectToAuthorizationEndpointContext(
                           Context, Options,
                           properties, developerAuthenticationEndpoint);
            await Options.Events.RedirectToAuthorizationEndpoint(redirectContext);
            return true;
        }
        private async Task<RequestToken> ObtainRequestTokenAsync(string callBackUri, AuthenticationProperties properties)
        {
            var nonce = Guid.NewGuid().ToString("N");
            var requestToken= new RequestToken()
            {
                CallBackUri = callBackUri,
                Token = nonce,
                Properties = properties,
                TokenSecret = nonce,
                CallbackConfirmed = true
            };
            // this simulates the remote service storing the token.
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };
            var data = DeveloperOptions.StateDataFormat.Protect(requestToken);
            var data2 = DeveloperOptions.StateDataFormat.Unprotect(data);
            // this is basically a database record entry that stores the access token.
            Response.Cookies.Append(RequestTokenCookie, data, cookieOptions);
            return requestToken;
        }

    }
}
