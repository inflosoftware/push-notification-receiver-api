// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;

namespace Inflo.Data
{
    public interface IDataAccess
    {
        int SaveChanges();

        IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

        void Create<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : class;

        void Update<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : class;

        void Delete<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : class;
    }
}
