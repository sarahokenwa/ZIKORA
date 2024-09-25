namespace USSDMiddleware.Core.Models.Request;

using System.ComponentModel.DataAnnotations;
using USSDMiddleware.Core.Enums;
public class BvnInfoRequest
{
    [RegularExpression(@"^\d{11}$", ErrorMessage = "Invalid BVN: BVN must be exactly 11 digits.")]
    public string Bvn { get; set; }
    public string PhoneNumber { get; set; }
    public Providers Provider { get; set; } = Providers.ZIKORA;
}