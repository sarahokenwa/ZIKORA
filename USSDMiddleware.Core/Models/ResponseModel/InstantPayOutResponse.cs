namespace USSDMiddleware.Core.Models.ResponseModel
{
    public class InstantPayOutResponse
    {
            public string Code { get; set; }
            public string? Message { get; set; }
            public bool Succeeded { get; set; }
            public DataResponse Data { get; set; }
        

            public class DataResponse
            {
                public string SessionId { get; set; }
                public bool Succeeded { get; set; }
                public string Code { get; set; }
                public string Message { get; set; }
                public string Data { get; set; }
            }
    }
}
