// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Inflo.WebApi.Models
{
    public class EngagementRequestCompleted
    {
        /// <summary>
        /// Unique ID of the push notification message to identify duplicate messages
        /// </summary>
        [Required]
        public Guid MessageId { get; set; }


        [Required]
        public long EngagementRequestId { get; set; }

        [Required]
        public DateTime NotificationUtcDate { get; set; }
    }
}
