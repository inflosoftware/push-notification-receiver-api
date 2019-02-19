// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Services;
using Inflo.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inflo.WebApi.Controllers
{
    [JwtAuth, Route("api/[controller]")]
    public class PushNotificationsController : Controller
    {
        public PushNotificationsController(IQueueService queueService)
        {
            QueueService = queueService;
        }
        private IQueueService QueueService { get; }


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
        /// </remarks>
        [HttpPost("{actionName}")]
        public IActionResult Notification([FromRoute]string actionName, [FromBody]object model)
        {
            // Store the payload in a queue to be processed by a another worker process that can
            // process items in the queue not on the same request as the push notification being received.
            // Building the processing of the queue messages is outside the scope of this project.
            QueueService.AddToQueue(actionName, model);

            // Respond with success indicating the notification was recieved and will be processed later.
            return Ok();
        }


        /*
         * Alternatively you can create seperate endpoints for each action and model accepted.
         * The code below is an example of an endpoint for a notification action of EngagementRequestCompleted.
         * 
         */
        //// POST: api/PushNotifications/EngagementRequestCompleted
        //[HttpPost("EngagementRequestCompleted")]
        //public IActionResult EngagementRequestCompleted([FromBody]EngagementRequestCompleted model)
        //{
        //    //Store the payload in a queue to be processed by a another worker process that can
        //    //process items in the queue not on the same request as the push notification being received.
        //    //Building the processing of the queue messages is outside the scope of this project.
        //    QueueService.AddToQueue("EngagementRequestCompleted", model);

        //    //Respond with success indicating the notification was recieved and will be processed later.
        //    return Ok();
        //}

    }
}
