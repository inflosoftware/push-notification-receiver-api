// Copyright (c) Inflo Limited. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Inflo.EntityModels
{
    public class User : EntityBase<long>
    {
        public string Forename { get; set; }

        public string Surname { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }
    }
}
