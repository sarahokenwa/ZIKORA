using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace USSDMiddleware.Core.Entities
{
    public class IntraBankTransfer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [StringLength(60)]
        public string FromAccountNumber { get; set; }
        [StringLength(60)]
        public string ToAccountNumber { get; set; }
        public decimal? Fee { get; set; }
        [StringLength(60)]
        public string RetrievalReference { get; set; }
        [StringLength(60)]
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public bool IsSuccessful { get; set; }
        [StringLength(60)]
        public string ResponseMessage { get; set; }
        [StringLength(60)]
        public string ResponseCode { get; set; }
        [StringLength(60)]
        public string Reference { get; set; }
        public string ProcessorRef { get; set; }
        public string ProviderId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public IntraBankTransfer()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
