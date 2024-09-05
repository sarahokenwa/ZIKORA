namespace USSDMiddleware.Core.Models.Providers.Zikora
{
    public class BvnInfoResponse
    {
        public bool RequestStatus { get; set; }
        public string ResponseMessage { get; set; }
        public bool isBvnValid { get; set; }
        public BvnDetails bvnDetails { get; set; }
    }

    public class BvnDetails
    {
        public string BVN { get; set; }
        public string phoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? OtherNames { get; set; }
        public string DOB { get; set; }
        public string Email { get; set; }
    }
}
