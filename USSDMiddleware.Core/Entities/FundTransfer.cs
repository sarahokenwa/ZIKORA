using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Entities
{
    public class FundTransfer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [StringLength(60)]
        public string? WalletCode { get; set; }
        [StringLength(60)]
        public string BeneficiaryName { get; set; }
        [StringLength(60)]
        public string SenderName { get; set; }
        [StringLength(10)]
        public string SenderAccountNumber { get; set; }
        [StringLength(60)]
        public string AccountNumber { get; set; }
        [StringLength(60)]
        public string PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        [StringLength(60)]
        public string TransactionPin { get; set; }
        [StringLength(60)]
        public string? BankCode { get; set; }
        [StringLength(60)]
        public string WebHook { get; set; }
        [StringLength(60)]
        public string MerchantRef { get; set; }
        [StringLength(60)]
        public string Narration { get; set; }
        [StringLength(60)]
        public string WalletType { get; set; }
        [StringLength(60)]
        public string ProcessorRef { get; set; }
        [StringLength(60)]
        public string Code { get; set; }
        [StringLength(60)]
        public string Message { get; set; }
        [StringLength(60)]
        public string SessionId { get; set; }
        [StringLength(60)]
        public string Data { get; set; }
        public bool Succeeded { get; set; }
        public decimal MerchantCharge { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public Provider Provider { get; set; }
        public string ProviderId { get; set; }

        public FundTransfer()
        {
            Id = Guid.NewGuid().ToString();
        }

    }
}
