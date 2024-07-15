using Newtonsoft.Json;

namespace USSDMiddleware.Core
{
    public class Constants
    {
     
        public static class ValidationRegex
        {
            public const string General = "^.{0,255}$";
            public const string BusinessCode = "^.{0,255}$";
            public const string Reference = "^.{0,255}$";
        }
        
        public static class Policies
        {
            public const string Cors = "CorsPolicy";
        }
        public static class AuthScheme
        {
            public const string Cookie = "cookie";
            public const string OIDC = "OpenIdConnect";
            public const string JWT = "jwt";
        }
        
        public static class ResponseCodes
        {
            public const string Successful = "200";
            public const string NoContent = "204";
            public const string UnAuthorized = "401";
            public const string BadRequest = "400";
            public const string NotFound = "404";
            public const string InternalServerError = "500";
        }
    }

}
