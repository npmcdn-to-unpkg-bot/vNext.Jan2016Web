// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.P6Common.Messages;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Authentication.DeveloperAuth
{
    internal class DeveloperAuthHandler : RemoteAuthenticationHandler<DeveloperAuthOptions>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public const string StateCookie = "__DeveloperAuthState";
        public const string RequestTokenCookie = "__RequestToken";
        private const string AuthenticationEndpoint = "/Developer/Home/Authenticate?accessToken=";
        private readonly HttpClient _httpClient;

        public DeveloperAuthHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected override async Task<AuthenticateResult> HandleRemoteAuthenticateAsync()
        {
            AuthenticationProperties properties = null;
            var query = Request.Query;
            var protectedRequestToken = Request.Cookies[StateCookie];

            var requestToken = DeveloperAuthOptions.StateDataFormat.Unprotect(protectedRequestToken);

            if (requestToken == null)
            {
                return AuthenticateResult.Failed("Invalid state cookie.");
            }

            properties = requestToken.Properties;

            // REVIEW: see which of these are really errors

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };

            Response.Cookies.Delete(StateCookie, cookieOptions);

            var accessToken = new AccessToken()
            {
                CallBackUri = requestToken.CallBackUri,
                CallbackConfirmed = requestToken.CallbackConfirmed,
                Properties = requestToken.Properties,
                ScreenName = Request.Form["_email"],
                UserId = Request.Form["_userId"]
            };

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, accessToken.UserId, ClaimValueTypes.String, Options.ClaimsIssuer),
                new Claim(ClaimTypes.Name, accessToken.ScreenName, ClaimValueTypes.String, Options.ClaimsIssuer),
             },
            Options.ClaimsIssuer);

            if (Options.SaveTokensAsClaims)
            {
                identity.AddClaim(new Claim("access_token", accessToken.Token, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            return AuthenticateResult.Success(await CreateTicketAsync(identity, properties, accessToken));
        }

        protected virtual async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, AccessToken token)
        {
            var context = new DeveloperAuthCreatingTicketContext(Context, Options, token.UserId, token.ScreenName, token.Token, token.TokenSecret)
            {
                Principal = new ClaimsPrincipal(identity),
                Properties = properties
            };

            await Options.Events.CreatingTicket(context);

            if (context.Principal?.Identity == null)
            {
                return null;
            }

            return new AuthenticationTicket(context.Principal, context.Properties, Options.AuthenticationScheme);
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var properties = new AuthenticationProperties(context.Properties);
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = CurrentUri;
            }
            var callBackUri = BuildRedirectUri(Options.CallbackPath);
            var requestToken = await ObtainRequestTokenAsync(callBackUri, properties);
            // If CallbackConfirmed is false, this will throw
            //var requestToken = await ObtainRequestTokenAsync(Options.ConsumerKey, Options.ConsumerSecret, BuildRedirectUri(Options.CallbackPath), properties);
            var authenticationEndpoint = AuthenticationEndpoint + requestToken.Token;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };

            Response.Cookies.Append(StateCookie, DeveloperAuthOptions.StateDataFormat.Protect(requestToken), cookieOptions);

            var redirectContext = new DeveloperAuthRedirectToAuthorizationEndpointContext(
                Context, Options,
                properties, authenticationEndpoint);
            await Options.Events.RedirectToAuthorizationEndpoint(redirectContext);
            return true;
        }

        private async Task<RequestToken> ObtainRequestTokenAsync(string callBackUri, AuthenticationProperties properties)
        {
            var nonce = Guid.NewGuid().ToString("N");
            var requestToken = new RequestToken()
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
            var data = DeveloperAuthOptions.StateDataFormat.Protect(requestToken);
            var data2 = DeveloperAuthOptions.StateDataFormat.Unprotect(data);
            // this is basically a database record entry that stores the access token.
            Response.Cookies.Append(RequestTokenCookie, data, cookieOptions);
            return requestToken;
        }

        private static string GenerateTimeStamp()
        {
            var secondsSinceUnixEpocStart = DateTime.UtcNow - Epoch;
            return Convert.ToInt64(secondsSinceUnixEpocStart.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private string ComputeSignature(string consumerSecret, string tokenSecret, string signatureData)
        {
            using (var algorithm = new HMACSHA1())
            {
                algorithm.Key = Encoding.ASCII.GetBytes(
                    string.Format(CultureInfo.InvariantCulture,
                        "{0}&{1}",
                        UrlEncoder.UrlEncode(consumerSecret),
                        string.IsNullOrEmpty(tokenSecret) ? string.Empty : UrlEncoder.UrlEncode(tokenSecret)));
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(signatureData));
                return Convert.ToBase64String(hash);
            }
        }
    }
}