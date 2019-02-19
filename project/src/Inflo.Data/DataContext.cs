// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Inflo.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace Inflo.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<IdentityClient> IdentityClients { get; set; }

        public DbSet<QueueMessage> QueueMessages { get; set; }

        public DbSet<AsymmetricKey> AsymmetricKeys { get; set; }
    }
}
