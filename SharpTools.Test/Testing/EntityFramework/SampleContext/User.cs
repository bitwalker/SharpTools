using System.ComponentModel.DataAnnotations;

namespace SharpTools.Test.Testing.EntityFramework.SampleContext
{
    public class User
    {
        public int Id { get; set; }
        public Role UserRole { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string HashedPassword { get; set; }
    }
}
