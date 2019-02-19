// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.Data;

namespace Inflo.Services
{
    public abstract class ServiceBase : IServiceBase
    {
        public ServiceBase(IDataAccess dataAccess)
        {
            DataAccess = dataAccess;
        }

        protected IDataAccess DataAccess { get; }
    }
}
