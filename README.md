# vNext.Jan2016Web
ASP.NET vNext project to test out Areas in isolated projects


## 1. Command Line
>cd %SolutionDirectory%/src/WebApplication1  
>gulp watch

This will copy over the static assets from dependant projects.  *.cshtml, etc.  
This also watches those dependant projects asset folders and copies them on every change to the hosting web application.

## 2. Build and run.  
>set your startup project to *WebApplication1*


## 3. IdentityServer4
new command window.
>cd %SolutionDirectory%/src/Web.IdentityServer4.Host  
>gulp watch


NOTE: I want an all-in-one, but due to the following issue;
https://github.com/IdentityServer/IdentityServer4/issues/8
I have to have 2 servers.  This is claimed to be fixed when Microsoft release RC2 or IdentityModel.

## 4. ConsoleClientCredentialsFlow
This is a IdentityServer4 console client that test that the client credentials are all working when making calls to published APIS.


So, you need to run both the WebApplications, and then run the client to test the apis.



