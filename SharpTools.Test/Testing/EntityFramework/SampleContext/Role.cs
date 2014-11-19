using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace SharpTools.Test.Testing.EntityFramework.SampleContext
{
    public class Role
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }

        public Role()
        {
            Users = new Collection<User>();
        }
    }
}
