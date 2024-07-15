namespace USSDMiddleware.Core.Models.IdentityModel
{
    public class IdentityOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string GrantType { get; set; }
        public string ClientType { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

    }
}
