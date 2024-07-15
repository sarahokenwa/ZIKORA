namespace USSDMiddleware.Core.Models.IdentityModel
{
    public class TwoFactorAuthenticateModel
    {
        public string Id { get; set; }
        public string Otp { get; set; }
        public bool RememberMe { get; set; }
        public TokenModel[] Token { get; set; }
    }
}
