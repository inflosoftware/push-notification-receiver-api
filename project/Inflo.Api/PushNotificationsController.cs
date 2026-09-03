// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Inflo.Api;

[Route("api/[controller]")]
public class PushNotificationsController(ILogger<PushNotificationsController> logger) : ControllerBase
{
    private readonly ILogger<PushNotificationsController> _logger = logger;

    /// <summary>
    ///  Generic catch all notifications posted from Inflo Push Notifications API
    /// </summary>
    /// <remarks>
    /// This approach saves you having to build a new endpoint for each notification type Inflo sends via the Push API.
    /// 
    /// *** Please Note *** 
    /// Push notifications should not trigger any events (like making an API call to Inflo APIs) as 
    /// the notification should be stored in a queue or database for background processing. 
    /// The Inflo Push Notification system will terminate if a response is not done 
    /// within the documented time frame and will assume a retry/failure needs to occur.
    ///
    /// </remarks>
    [HttpPost("{actionName}")]
    public IActionResult Notification([FromRoute] string actionName, [FromBody] object model)
    {
        _logger.LogInformation("Received catchall notification");
        
        // Respond with success indicating the notification was recieved and will be processed later.
        return Ok("This is a catchall endpoint");
    }

    /// <summary>
    /// Specific event endpoint
    /// </summary>
    /// <remarks>
    /// If you want to perform any specific actions on 1 event, then you can create an endpoint like below to
    /// capture that specific event.
    /// 
    /// *** Please Note *** 
    /// Push notifications should not trigger any events (like making an API call to Inflo APIs) as 
    /// the notification should be stored in a queue or database for background processing. 
    /// The Inflo Push Notification system will terminate if a response is not done 
    /// within the documented time frame and will assume a retry/failure needs to occur.
    ///
    /// </remarks>
    [HttpPost("EngagementArchived")]
    public IActionResult AnonymousEngagementArchived([FromRoute] string actionName, [FromBody] object model)
    {
        _logger.LogInformation("Received event specific notification");
        
        // Respond with success indicating the notification was recieved and will be processed later.
        return Ok("This is a specific event endpoint");
    }

    /// <summary>
    /// Example of a catchall endpoint that requires authentication
    /// </summary>
    /// <remarks>
    /// If you want to perform any specific actions on 1 event, then you can create an endpoint like below to
    /// capture that specific event.
    /// 
    /// *** Please Note *** 
    /// Push notifications should not trigger any events (like making an API call to Inflo APIs) as 
    /// the notification should be stored in a queue or database for background processing. 
    /// The Inflo Push Notification system will terminate if a response is not done 
    /// within the documented time frame and will assume a retry/failure needs to occur.
    ///
    /// </remarks>
    [Authorize]
    [HttpPost("authenticated/{actionName}")]
    public IActionResult Authenticated([FromRoute] string actionName, [FromBody] object model)
    {
        
        _logger.LogInformation("Received authenticated catchall notification");
        return Ok("This endpoint requires authentication.");
    }
}