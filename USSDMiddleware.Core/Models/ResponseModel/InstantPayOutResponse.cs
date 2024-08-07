namespace USSDMiddleware.Core.Models.ResponseModel
{
    public class InstantPayOutResponse
    {
        public string sessionId { get; set; }
        public bool Succeeded { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}
