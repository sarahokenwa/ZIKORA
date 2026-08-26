namespace USSDMiddleware.Core.Models.V2.Request
{
    public class UdaraValidateBvnRequestModel
    {
        public string Bvn { get; set; } = string.Empty;
        public int IncludeData { get; set; } = 1;
    }
}
