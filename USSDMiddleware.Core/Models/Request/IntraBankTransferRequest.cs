using System.ComponentModel.DataAnnotations;

namespace USSDMiddleware.Core.Models.Request;

public class IntraBankTransferRequest
{
    [StringLength(10, MinimumLength = 10, ErrorMessage = "FromAccountNumber must be exactly 10 characters long.")]
    public string FromAccountNumber { get; set; }

    [StringLength(10, MinimumLength = 10, ErrorMessage = "ToAccountNumber must be exactly 10 characters long.")]
    public string ToAccountNumber { get; set; }
    public decimal? Fee { get; set; }
    public string? RetrievalReference { get; set; }
    public string Narration { get; set; }
    public decimal Amount { get; set; }
    public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;

}