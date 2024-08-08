namespace USSDMiddleware.Core.Models.ResponseModel
{
    public class NameEnquiryResponse
    {
        public string RspCode { get; set; }
        public string AccountName { get; set; }
        public string NameEnquiryRef { get; set; }
        public string BVN { get; set; }
        public int KycLevel { get; set; }
    }
}
