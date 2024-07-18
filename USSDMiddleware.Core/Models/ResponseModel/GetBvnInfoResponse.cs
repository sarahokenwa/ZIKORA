namespace USSDMiddleware.Core.Models.ResponseModel;

public class GetBvnInfoResponse
{
    public string validationReference { get; set; }
    public bool bvnValid { get; set; }

    public GetBvnInfoResponse(string validationReference, bool bvnValid)
    {
        this.validationReference = validationReference;
        this.bvnValid = bvnValid;
    }
}