﻿using System;
using System.Data.Entity;

using SharpTools.Database.EntityFramework;

namespace SharpTools.Test.Testing.EntityFramework.SampleContext
{
    public interface ISampleContext : IDbContext, IDisposable
    {
        IDbSet<User> Users { get; set; }
        IDbSet<Role> Roles { get; set; }
    }
}
