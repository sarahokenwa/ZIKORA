using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Entities
{
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [StringLength(60)]
        public string AccountNumber { get; set; }

        [StringLength(60)]
        public string TransactionPin { get; set; }
        [StringLength(60)]
        public string? BIN { get; set; }

        [StringLength(60)]
        public string? ProcessorRef { get; set; }

        [StringLength(60)]
        public string? RequestType { get; set; }

        [StringLength(60)]
        public string? DeliveryOption { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string NameOnCard { get; set; }

        [StringLength(60)]
        public string? BatchNo { get; set; }

        public bool IsSuccessful { get; set; }

        [StringLength(200)]
        public string? ResponseMessage { get; set; }

        [StringLength(60)]
        public string? Identifier { get; set; }
        public Provider Provider { get; set; }
        public string ProviderId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public Card()
        {
            Id = Guid.NewGuid().ToString();
        }

    }
}
