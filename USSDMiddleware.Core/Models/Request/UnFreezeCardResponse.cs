namespace USSDMiddleware.Core.Models.Request
{
    public class UnFreezeCardResponse
    {
        public bool IsSuccessful { get; set; }
        public string ResponseMessage { get; set; }
        public string Reference {  get; set; }
    }
}
