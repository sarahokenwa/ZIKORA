namespace USSDMiddleware.Core.Models.V2.Request
{
    public class UdaraUpdateCardStatusRequestModel
    {
        public string CardId { get; set; } = string.Empty;
        public int Status { get; set; }   // 1=Block, 2=Unblock, 3=Hotlist, 4=Activate
        public string? Reason { get; set; }
    }
}
