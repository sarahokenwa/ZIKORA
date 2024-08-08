using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;
using USSDMiddleware.Core.Services;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using USSDMiddleware.Core;

namespace USSDMiddleware.Api
{
    public static class SecurityExtensions
    {
        public static void AddSecurityServices(this IServiceCollection services, IConfiguration Configuration)
        {
            var corsOrigins = Configuration.GetValue<string>("CorsOrigins").Split(",");
            services.AddCors(options => options.AddPolicy(Constants.Policies.Cors,
                builder =>
                {
                    builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                           .WithOrigins(corsOrigins)
                           .AllowAnyMethod()
                           .AllowCredentials()
                           .AllowAnyHeader()
                           .Build();
                }));

            var host = Configuration.GetValue<string>("Identity:Authority");
            var idoptions = Configuration.GetSection("Identity").Get<IdentityOptions>();
             
            services.AddScoped<SecurityService>();


            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
         .AddJwtBearer(x =>
         {
             x.RequireHttpsMetadata = false;
             x.SaveToken = true;
             x.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuerSigningKey = true,
                 IssuerSigningKey = GetSecurityKey(Configuration.GetValue<string>("Identity:IdentityPublicKey")),
                 ValidateIssuer = false,
                 ValidateAudience = false
             };
         })
         .AddCookie(Constants.AuthScheme.Cookie, options =>
         {
             options.LoginPath = "/account";
             options.Cookie = new CookieBuilder { HttpOnly = true, SecurePolicy = CookieSecurePolicy.Always, SameSite = SameSiteMode.Strict };

             options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
         });

        }




        private static SecurityKey GetSecurityKey(string key)
        {
            string publicKey = $"-----BEGIN PUBLIC KEY-----\r\n{key}\r\n-----END PUBLIC KEY-----";
            RSACryptoServiceProvider _rsaProviderPublic = GetRSAProviderFromPem(publicKey);
            return new RsaSecurityKey(_rsaProviderPublic.ExportParameters(false));
        }
        private static RSACryptoServiceProvider GetRSAProviderFromPem(string pemstr)
        {
            CspParameters cspParameters = new CspParameters();
            RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(cspParameters);
            Func<RSACryptoServiceProvider, RsaKeyParameters, RSACryptoServiceProvider> MakePublicRCSP = (RSACryptoServiceProvider rcsp, RsaKeyParameters rkp) =>
            {
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            };

            Func<RSACryptoServiceProvider, RsaPrivateCrtKeyParameters, RSACryptoServiceProvider> MakePrivateRCSP = (RSACryptoServiceProvider rcsp, RsaPrivateCrtKeyParameters rkp) =>
            {
                RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            };

            PemReader reader = new PemReader(new StringReader(pemstr));

            object kp = reader.ReadObject();

            return (kp.GetType().GetProperty("Private") != null) ? MakePrivateRCSP(rsaKey, (RsaPrivateCrtKeyParameters)(((AsymmetricCipherKeyPair)kp).Private)) : MakePublicRCSP(rsaKey, (RsaKeyParameters)kp);
        }

    }
}


