using System.ComponentModel.DataAnnotations;

namespace POSLibrary.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(15)]
        public string ContactNumber { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }
    }
}
