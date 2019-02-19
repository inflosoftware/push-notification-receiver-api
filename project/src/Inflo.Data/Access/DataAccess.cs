// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System;

namespace Inflo.Data
{
    public class DataAccess : IDataAccess
    {
        public DataAccess(DataContext dataContext)
        {
            DataContext = dataContext;
        }

        private DataContext DataContext { get; }


        /// <summary>
        ///  Saves all changes made in th context to the database. 
        ///  Returns the number of state entries written to the database.
        /// </summary>
        public int SaveChanges()
        {
            return DataContext.SaveChanges();
        }


        #region " CRUD operations "

        // READ
        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return DataContext.Set<TEntity>();
        }

        // CREATE
        public void Create<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DataContext.Add(entity);

            if (saveChanges)
            {
                SaveChanges();
            }
        }

        // UPDATE
        public void Update<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DataContext.Update(entity);

            if (saveChanges)
            {
                SaveChanges();
            }
        }


        // DELETE
        public void Delete<TEntity>(TEntity entity, bool saveChanges = true) where TEntity : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            DataContext.Remove(entity);

            if (saveChanges)
            {
                SaveChanges();
            }
        }

        #endregion
    }
}
