using System.Data.Entity;

using SharpTools.Database.EntityFramework;

namespace SharpTools.Test.Testing.EntityFramework.SampleContext
{
    public class SampleContext : BaseDbContext, ISampleContext
    {
        public IDbSet<User> Users { get; set; }

        public IDbSet<Role> Roles { get; set; }
    }
}
