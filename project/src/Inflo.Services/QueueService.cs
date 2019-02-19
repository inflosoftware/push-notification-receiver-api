// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Data;
using Inflo.EntityModels;
using Newtonsoft.Json;
using System;

namespace Inflo.Services
{
    public class QueueService : ServiceBase, IQueueService
    {
        public QueueService(IDataAccess dataAccess) : base(dataAccess) { }


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
    }
}
