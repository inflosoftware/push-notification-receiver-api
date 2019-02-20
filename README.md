# 1) Setup of Inflo Push Notification Receiving Server
To make integration and setup as easy as possible the Inflo Development Team have produced an application that you can host to receive the push notifications from the Inflo ecosystem.
The application is built on ASP.Net Core and can be hosted on any platform that supports it. By default, the code is configured to run on Windows IIS Server but can be altered to run on any platform ASP.Net Core supports. 

## 1.1) Configure App Settings

Within the Inflo.WebApi project there is two app settings files
* appsettings.json 
    *	The app settings used on production servers
*	appsettings.development.json
    *	The app settings used for development use on localhost

The main sections within these settings files that are of relevance are:
* ConnectionStrings
    * DefaultConnection
      * The connection string for the SQL server.
      * 	Example: ``` “Data Source={ServerLocation};Initial Catalog={DatabaseName};User Id={User};Password={Password};App=Inflo.WebApi;Encrypt=True;TrustServerCertificate=True;Connection Timeout=60;”  ```
      Replacing the { } parts with your servers relevant information.
* InfloOptions
  * FullyQualifiedDomainName
    * The public facing domain the API will be running on. This value is used for locking down the Json Web Tokens valid issuer and audience. This is also useful if the application will be running behind a firewall or proxy server.
    * Example: "https://example.com"
  * KeyRotationDays
    * The frequency that RSA Signing Credentials for Json Web Tokens are retired (defaults to 30). Retired credentials will remain active for 2 days after being retired for validating tokens during the rotation period but will not be used to sign any new tokens.
    * Example: 30
    
## 1.2) Plugin your Data Store (SQL Server)

The application uses Entity Framework Core for accessing the backing data store and by default is setup to use SQL Server. The connection string is located in the app settings json file, please see section 4.1 Configure App Settings for more information on the connection string setup. 

The Inflo.Data project within the solution has the nuget package Microsoft.EntityFrameworkCore.SqlServer installed and you can swop this package for one that better suites your data storage needs if you do not intend to use SQL Server.  

> #### Please Note: 
> If you do change the nuget package you will also need to change the code located within the Startup.cs file inside the Inflo.WeApi project that configures the DataContext to use SQL Server.

```C#
public void ConfigureServices(IServiceCollection services)
{
    //... Removed for brevity


    // Configure EF Core to use SQL server and retrive 
    // the connection string from appsettings.json
    services.AddDbContext<DataContext>(
        builder => 
            builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
}
```

## 1.3) Push Notification Storage
By default, the application has no build in Message Queue (MQ) System and logs the push notification messages to the SQL database table QueueMessages. 

Inflo highly recommend the message is also put into some form of MQ System to be actioned by a separate process that listens for messages arriving in the queue and not process the message on the same HTTP request that receives the message.

An MQ System can easily be integrated by altering the implementation of the QueueService.cs file within the Inflo.Services project.
The code snippet below is the main entry point for the massage logging and passing onto your preferred MQ System:

```C#
public void AddToQueue<TModel>(string actionName, TModel model) where TModel : class
{
    var queueMessage = new QueueMessage
    {
        ActionName = actionName,
        JsonMetadata = JsonConvert.SerializeObject(model),
        DateTimeCreated = DateTime.UtcNow
    };

    // Log notification into DB 
    DataAccess.Create(queueMessage);


    // TODO: Insert code here to push the queueMessage into your preferred Message Queue (MQ) system.
}
```

After the call to ```DataAccess.Create(queueMessage)```, which will log the message into the database, plugin whatever MQ System code you require, where the TODO comment is, that will allow the message to be put into a queue for processing.

> #### Please Note: 
> When receiving a Push Notification message, you should not process the notification on the same HTTP request that receives the message. As mentioned above the message should be put onto a MQ System for processing outside of the HTTP request otherwise this could potentially cause the Inflo Push API that send the original message to timeout and assume message delivery was unsuccessful and will retry sending the push notification again.


## 1.4)	Host and Deploy
There are many ways to host and deploy an ASP.Net Core application and this is outside of the scope of this document. Microsoft have detailed the many options available to you on the online docs:

* https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-2.2
