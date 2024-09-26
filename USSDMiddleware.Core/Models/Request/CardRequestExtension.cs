namespace USSDMiddleware.Core.Models.Request
{
    public class CardRequestExtension : CardRequest
    {
        public string BIN { get; set; }
        public string RequestType { get; set; }
        public string DeliveryOption { get; set; }
        public string Identifier { get; set; }
        public string NameOnCard { get; set; }
    }
}
