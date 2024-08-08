using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Models
{
    public class BankResponse
    {
        public string Code { get; set; }
        public bool Succeeded { get; set; }
        public BankResponseDto[] Data { get; set; }
    }
}
