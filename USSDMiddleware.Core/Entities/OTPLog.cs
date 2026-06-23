using System.ComponentModel.DataAnnotations;

namespace USSDMiddleware.Core.Entities
{
    public class OTPLog
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = default!;
        [MaxLength(10)]
        public string OTP { get; set; } = default!;
        [MaxLength(50)]
        public string ProviderId { get; set; } = default!;
        public DateTimeOffset CreatedOn { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTimeOffset? UsedOn { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
    }
}
