// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Inflo.Services
{
    public interface IQueueService
    {
        void AddToQueue<TModel>(string actionName, TModel model) where TModel : class;
    }
}
