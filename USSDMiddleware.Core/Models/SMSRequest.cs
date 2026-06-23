namespace USSDMiddleware.Core.Models
{
    public class SMSRequest
    {
        public string PhoneNumber { get; set; } = default!;
        public string Message { get; set; } = default!;
    }

    public class ZikoraSMSRequest
    {
        public string api_key { get; set; }
        public string to { get; set; }
        public string from { get; set; }
        public string sms { get; set; }
        public string type { get; set; }
        public string channel { get; set; }
    }

    public class SMSResponse
    {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; } = default!;
    }

    public class ZikoraSMSResponse
    {
        public string code { get; set; }
        public float balance { get; set; }
        public string message_id { get; set; }
        public string message { get; set; }
        public string user { get; set; }
        public string message_id_str { get; set; }
    }
}
