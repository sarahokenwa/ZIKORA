using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USSDMiddleware.Core.Entities
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string CustomerID{ get; set; }
        public string? LastName { get; set; }
        public string? OtherNames { get; set; }
        public string? BVN { get; set; }
        public string? AccountName { get; set; }
        public string? AccountNumber { get; set; }
        public string? PhoneNo { get; set; }
        public int? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public string? Address { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public Account()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
