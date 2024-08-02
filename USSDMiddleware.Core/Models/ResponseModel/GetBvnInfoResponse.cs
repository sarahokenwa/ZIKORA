namespace USSDMiddleware.Core.Models.ResponseModel;

public class GetBvnInfoResponse
{
    public string validationReference { get; set; }
    public bool bvnValid { get; set; }
    public string dob { get; set; }

    public GetBvnInfoResponse(string validationReference, bool bvnValid, string dob)
    {
        this.validationReference = validationReference;
        this.bvnValid = bvnValid;
        this.dob = dob;
    }
}