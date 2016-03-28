using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console.IdentityServer4.Cassandra.Models;

namespace Console.IdentityServer4.Cassandra.Models
{
    /*
    CREATE TYPE IF NOT EXISTS Client (
  AbsoluteRefreshTokenLifetime int,
  AccessTokenLifetime int,
  AccessTokenType int,
  AllowAccessToAllCustomGrantTypes boolean,
  AllowAccessToAllScopes boolean,
  AllowClientCredentialsOnly boolean,
  AllowedCorsOrigins list<text>,
  AllowedCustomGrantTypes list<text>,
  AllowedScopes list<text>,
  AllowPromptNone boolean,
  AllowRememberConsent boolean,
  AlwaysSendClientClaims boolean,
  AuthorizationCodeLifetime int,

  ClientId text,
  ClientName text,
  ClientSecrets list<FROZEN<Secret>>,
  ClientUri text,

  Enabled boolean,
  EnableLocalLogin boolean,

  Flow int,

  IdentityProviderRestrictions list<text>,
  IdentityTokenLifetime int,
  IncludeJwtId boolean,
  LogoUri text,
  LogoutSessionRequired boolean,
  LogoutUri text,
  PostLogoutRedirectUris list<text>,
  PrefixClientClaims boolean,
  RedirectUris list<text>,
  
  RefreshTokenExpiration int,
  RefreshTokenUsage int,
  
  RequireConsent boolean,
  SlidingRefreshTokenLifetime int,
  
  UpdateAccessTokenClaimsOnRefresh boolean
);

    */
    // Colors colorValue = (Colors)Enum.Parse(typeof(Colors), colorString);
    //
    // Summary:
    //     Access token types.
    public enum AccessTokenType
    {
        //
        // Summary:
        //     Self-contained Json Web Token
        Jwt ,
        //
        // Summary:
        //     Reference token
        Reference
    }
    //
    // Summary:
    //     OpenID Connect flows.
    public enum Flows
    {
        //
        // Summary:
        //     authorization code flow
        AuthorizationCode = 0,
        //
        // Summary:
        //     implicit flow
        Implicit = 1,
        //
        // Summary:
        //     hybrid flow
        Hybrid = 2,
        //
        // Summary:
        //     client credentials flow
        ClientCredentials = 3,
        //
        // Summary:
        //     resource owner password credential flow
        ResourceOwner = 4,
        //
        // Summary:
        //     custom grant
        Custom = 5
    }
    //
    // Summary:
    //     Token expiration types.
    public enum TokenExpiration
    {
        //
        // Summary:
        //     Sliding token expiration
        Sliding = 0,
        //
        // Summary:
        //     Absolute token expiration
        Absolute = 1
    }
    //
    // Summary:
    //     Token usage types.
    public enum TokenUsage
    {
        //
        // Summary:
        //     Re-use the refresh token handle
        ReUse = 0,
        //
        // Summary:
        //     Issue a new refresh token handle every time
        OneTimeOnly = 1
    }

    public class Client
    {
       
        //
        // Summary:
        //     Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds /
        //     30 days
        public int? AbsoluteRefreshTokenLifetime { get; set; }
        //
        // Summary:
        //     Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
        public int? AccessTokenLifetime { get; set; }

        public int? _AccessTokenType { get; set; }

        //
        // Summary:
        //     Specifies whether the access token is a reference token or a self contained JWT
        //     token (defaults to Jwt).
        public AccessTokenType? AccessTokenType
        {
            get
            {
                if (_AccessTokenType == null)
                    return null;
                return (AccessTokenType) _AccessTokenType;
            }
            set { _AccessTokenType = (int) value; }
        }

        public bool? AllowAccessToAllCustomGrantTypes { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether the client has access to all scopes.
        //     Defaults to false. You can set the allowed scopes via the AllowedScopes list.
        public bool? AllowAccessToAllScopes { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether this client is allowed to request token
        //     using client credentials only. This is e.g. useful when you want a client to
        //     be able to use both a user-centric flow like implicit and additionally client
        //     credentials flow
        public bool? AllowClientCredentialsOnly { get; set; }

        public IEnumerable<string> AllowedCorsOrigins { get; set; }
        //
        // Summary:
        //     Gets or sets a list of allowed custom grant types when Flow is set to Custom.
        public IEnumerable<string> AllowedCustomGrantTypes { get; set; }
        //
        // Summary:
        //     Specifies the scopes that the client is allowed to request. If empty, the client
        //     can't access any scope
        public IEnumerable<string> AllowedScopes { get; set; }
        //
        // Summary:
        //     Gets or sets if client is allowed to use prompt=none OIDC parameter value.
        public bool? AllowPromptNone { get; set; }
        //
        // Summary:
        //     Specifies whether user can choose to store consent decisions (defaults to true)
        public bool? AllowRememberConsent { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether client claims should be always included
        //     in the access tokens - or only for client credentials flow.
        public bool? AlwaysSendClientClaims { get; set; }
        //
        // Summary:
        //     Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
        public int? AuthorizationCodeLifetime { get; set; }
        //
        // Summary:
        //     Unique ID of the client
        public string ClientId { get; set; }
        //
        // Summary:
        //     Client display name (used for logging and consent screen)
        public string ClientName { get; set; }
        //
        // Summary:
        //     Client secrets - only relevant for flows that require a secret
        public IEnumerable<Secret> ClientSecrets { get; set; }

        public IEnumerable<Claim> Claims { get; set; }

        //
        // Summary:
        //     URI to further information about client (used on consent screen)
        public string ClientUri { get; set; }
        //
        // Summary:
        //     Specifies if client is enabled (defaults to true)
        public bool? Enabled { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether the local login is allowed for this client.
        //     Defaults to true.
        public bool? EnableLocalLogin { get; set; }

        public int? _Flow { get; set; }

        //
        // Summary:
        //     Specifies allowed flow for client (either AuthorizationCode, Implicit, Hybrid,
        //     ResourceOwner, ClientCredentials or Custom). Defaults to Implicit.
        public Flows? Flow
        {
            get
            {
                if (_Flow == null)
                    return null;
                return (Flows)_Flow;
            }
            set { _Flow = (int)value; }
        }
        //
        // Summary:
        //     Specifies which external IdPs can be used with this client (if list is empty
        //     all IdPs are allowed). Defaults to empty.
        public IEnumerable<string> IdentityProviderRestrictions { get; set; }
        //
        // Summary:
        //     Lifetime of identity token in seconds (defaults to 300 seconds / 5 minutes)
        public int? IdentityTokenLifetime { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether JWT access tokens should include an identifier
        public bool? IncludeJwtId { get; set; }
        //
        // Summary:
        //     URI to client logo (used on consent screen)
        public string LogoUri { get; set; }
        //
        // Summary:
        //     Specifies is the user's session id should be sent to the LogoutUri. Defaults
        //     to true.
        public bool? LogoutSessionRequired { get; set; }
        //
        // Summary:
        //     Specifies logout URI at client for HTTP based logout.
        public string LogoutUri { get; set; }
        //
        // Summary:
        //     Specifies allowed URIs to redirect to after logout
        public IEnumerable<string> PostLogoutRedirectUris { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether all client claims should be prefixed.
        public bool? PrefixClientClaims { get; set; }
        //
        // Summary:
        //     Specifies allowed URIs to return tokens or authorization codes to
        public IEnumerable<string> RedirectUris { get; set; }
        //
        // Summary:
        //     Absolute: the refresh token will expire on a fixed point in time (specified by
        //     the AbsoluteRefreshTokenLifetime) Sliding: when refreshing the token, the lifetime
        //     of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime).
        //     The lifetime will not exceed AbsoluteRefreshTokenLifetime.
        public TokenExpiration? RefreshTokenExpiration
        {
            get
            {
                if (_RefreshTokenExpiration == null)
                    return null;
                return (TokenExpiration)_RefreshTokenExpiration;
            }
            set { _RefreshTokenExpiration = (int)value; }
        }
        public int? _RefreshTokenExpiration { get; set; }
        //
        // Summary:
        //     ReUse: the refresh token handle will stay the same when refreshing tokens OneTime:
        //     the refresh token handle will be updated when refreshing tokens
        public TokenUsage? RefreshTokenUsage
        {
            get
            {
                if (_RefreshTokenUsage == null)
                    return null;
                return (TokenUsage)_RefreshTokenUsage;
            }
            set { _RefreshTokenUsage = (int)value; }
        }
        public int? _RefreshTokenUsage { get; set; }
        //
        // Summary:
        //     Specifies whether a consent screen is required (defaults to true)
        public bool? RequireConsent { get; set; }
        //
        // Summary:
        //     Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds /
        //     15 days
        public int? SlidingRefreshTokenLifetime { get; set; }
        //
        // Summary:
        //     Gets or sets a value indicating whether the access token (and its claims) should
        //     be updated on a refresh token request.
        public bool? UpdateAccessTokenClaimsOnRefresh { get; set; }
    }
}
