using System.ComponentModel.DataAnnotations;

namespace USSDMiddleware.Core.Models.IdentityModel
{
    public class UserRegistrationModel : Model
    {
        [StringLength(50)]
        public string UserId { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(50)]
        public string Gender { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }


        public string FullName => $"{FirstName} {LastName}";

        [Range(int.MinValue, int.MaxValue)]
        public int BusinessTypeId { get; set; }

        [StringLength(50)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }

        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = false;
    }

}
