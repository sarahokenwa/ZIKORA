using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace USSDMiddleware.Core.Entities
{
    public class BlockAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [StringLength(60)]
        public string OwnersPhoneNumber { get; set; }
        [StringLength(60)]
        public string RequestPhoneNumber { get; set; }
        [StringLength(60)]
        public string AccountNo { get; set; }
        public bool RequestStatus { get; set; }
        [StringLength(60)]
        public string ResponseDescription { get; set; }
        [StringLength(60)]
        public string ResponseStatus { get; set; }
        public string ProviderId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public BlockAccount()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
