using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Security.Claims;
using USSDMiddleware.Core.Models.Security;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core;
using USSDMiddleware.Api.Extensions;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;

namespace USSDMiddleware.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IdentityOptions _idOptions;
        private readonly SecurityService _security;
        private readonly IConfiguration _configuration;
        public AuthController(SecurityService security,
            IdentityOptions idOptions, IConfiguration configuration)
        {
            _idOptions = idOptions;
            _security = security;
            _configuration = configuration;
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserInfo model)
        {
            var response = new Response<object>();

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    var message = string.Join(',', errors);
                    throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST, message);

                }

                var tokens = await _security.GetTokens(model);

                if (tokens.FirstOrDefault().TwoFactorEnabled)
                {
                    var twofactorModel = new TwoFactorAuthenticateModel
                    {
                        Id = tokens.FirstOrDefault().Id.ToString(),
                        RememberMe = model.RememberMe
                    };

                    response.Data = new
                    {
                        TwoFactorEnabled = true,
                        TwoFactorModel = twofactorModel
                    };


                    response.Succeeded = true;
                }
                else
                {
                    var authProps = new AuthenticationProperties();

                    authProps.StoreTokens(tokens.Select(t => new AuthenticationToken { Name = t.Type.ToString(), Value = t.Token }));
                    authProps.IsPersistent = model.RememberMe;
                    var claimsIdentity = new ClaimsIdentity(Constants.AuthScheme.Cookie);
                   
                    var principal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(Constants.AuthScheme.Cookie, principal, authProps);

                    response.Data = new
                    {
                        TwoFactorEnabled = false,
                        TokenModel = tokens
                    };
                    response.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                foreach (var item in ex.Data.Values)
                    response = response.ToModel(item.ToString());



                response.Message = ex.Message;
                response.Succeeded = false;
            }
            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }



    }

}
