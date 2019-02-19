// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace Inflo.EntityModels
{
    public class AsymmetricKey : EntityBase<Guid>
    {
        public string JsonMetadata { get; set; }

        public bool IsSigningCredential { get; set; }

        public DateTime DateTimeCreated { get; set; }
    }
}
