using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Entities
{
    public class CustomerDebit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [StringLength(60)]
        public string RetrievalReference { get; set; }
        [StringLength(60)]
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        [StringLength(60)]
        public string TransactionPin { get; set; }
        [StringLength(60)]
        public string Narration { get; set; }
        [StringLength(60)]
        public string GLCode { get; set; }
        [StringLength(60)]
        public string NibssCode { get; set; }
        [StringLength(60)]
        public string BankCode { get; set; }
        public decimal Fee { get; set; }
        [StringLength(60)]
        public string ProcessorRef { get; set; }
        public Provider Provider { get; set; }
        public string ProviderId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public CustomerDebit()
        {
            Id = Guid.NewGuid().ToString();
        }

    }
}
