// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace Inflo.WebApi.Controllers;

[Route("api/[controller]")]
public class PushNotificationsController : ControllerBase
{
    /// <summary>
    ///  Generic catch all notifications posted from Inflo Push Notifications API
    /// </summary>
    /// <remarks>
    /// This approach saves you having to build a new endpoint for each notification type Inflo sends via the Push API.
    /// 
    /// *** Plese Note *** 
    /// Push notifications should not trigger any events (like making an API call to Inflo APIs) as 
    /// the notification should be stored in a queue or database for background processing. 
    /// The Inflo Push Notification system will terminate if a response is not done 
    /// within the documented time frame and will asume a retry/failure needs to occur.
    ///
    /// </remarks>
    [HttpPost("{actionName}")]
    public IActionResult Notification([FromRoute] string actionName, [FromBody] object model)
    {
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
    /// *** Plese Note *** 
    /// Push notifications should not trigger any events (like making an API call to Inflo APIs) as 
    /// the notification should be stored in a queue or database for background processing. 
    /// The Inflo Push Notification system will terminate if a response is not done 
    /// within the documented time frame and will asume a retry/failure needs to occur.
    ///
    /// </remarks>
    [HttpPost("EngagementArchived")]
    public IActionResult AnonymousEngagementArchived([FromRoute] string actionName, [FromBody] object model)
    {
        // Respond with success indicating the notification was recieved and will be processed later.
        return Ok("This is a specific event endpoint");
    }
}