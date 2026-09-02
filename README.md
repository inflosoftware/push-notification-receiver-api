# 1) Setup of Inflo Push Notification Receiving Server
To make integration and setup as easy as possible the Inflo Development Team have produced an application that you can host to receive the push notifications from the Inflo ecosystem.

The application is built on ASP.Net Core and can be hosted on any platform that supports it. By default, the code is configured to run on Windows IIS Server but can be altered to run on any platform ASP.Net Core supports. 

This code is a general example of how to receive Inflo Push Notifications. We recommend that the code inside this repo is not used in production explicitly without changes to the provided authentication located in `OAuthController.cs` and `Program.cs`.

## 1.1) Configure App Settings

Within the Inflo.WebApi project there is two app settings files
* appsettings.json 
    *	The app settings used on production servers
*	appsettings.development.json
    *	The app settings used for development use on localhost

The main sections within these settings files that are of relevance are:
* OAuth
  * Issuer
    * The address of the services that 
  * Audience
    * Clients whom this authentication is valid for. Such as "inflo-push-notifications"
  * Signing Key
    * The key used to sign the token that gets generated. This should be rotated frequently and **must be cryptographically sound and not predictable**
    

## 1.3)	Host and Deploy
There are many ways to host and deploy an ASP.Net Core application and this is outside of the scope of this document. Microsoft have detailed the many options available to you on the online docs:

* https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-2.2
