namespace USSDMiddleware.Core.Models.V2.Response
{
    public class UdaraValidateBvnResponseModel
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UdaraValidateBvnData? Data { get; set; }
    }

    public class UdaraValidateBvnData
    {
        public UdaraBvnResult? Result { get; set; }
    }

    public class UdaraBvnResult
    {
        public string Bvn { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string PhoneNumber1 { get; set; } = string.Empty;
        public string PhoneNumber2 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
