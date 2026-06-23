namespace USSDMiddleware.Core.Models.Accounts
{
    public class PinResetRequest
    {
        public string PhoneNumber { get; set; } = default!;
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }

    public class PinResetResponse
    {
        public string Message { get; set; } = default!;
        public bool Success { get; set; }
    }

    public class CompletePinResetRequest
    {
        public string PhoneNumber { get; set; } = default!;
        public string OTP { get; set; } = default!;
        public string NewPin { get; set; } = default!;
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
    }

    public class CompletePinResetResponse
    {
        public string Message { get; set; } = default!;
        public bool Success { get; set; }
    }

    public class CompletePinResetModel
    {
        public string PhoneNumber { get; set; } = default!;
        public Enums.Providers Provider { get; set; } = Enums.Providers.ZIKORA;
        public int OTPLogId { get; set; }
        public string EncryptedPin { get; set; } = default!;
    }
}
