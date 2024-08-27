namespace USSDMiddleware.Core.Models.ResponseModel
{
    public class CardResponse
    {
        public bool IsSuccessful { get; set; }
        public string ResponseMessage { get; set; }
        public string BatchNo { get; set; }
        public string Identifier { get; set; }
    }
}
