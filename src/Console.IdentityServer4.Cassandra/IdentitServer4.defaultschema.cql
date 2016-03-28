CREATE KEYSPACE IF NOT EXISTS identityserver4
WITH replication = {'class':'SimpleStrategy', 'replication_factor' : 1};
use identityserver4;

CREATE TYPE IF NOT EXISTS Secret (
  Description text,
  Expiration timestamp,
  Type text,
  Value text
);

CREATE TYPE IF NOT EXISTS ClaimsIdentity (
	Name text
	
);
CREATE TYPE IF NOT EXISTS Claim (
	Type text,
	Value text,
	ValueType text
);

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

  Claims list<FROZEN<Claim>>,
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

CREATE TABLE IF NOT EXISTS Clients (
	Id uuid,
	Client Frozen<Client>,
 	PRIMARY KEY (Id)
);

