using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace USSDMiddleware.Core.Exceptions
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,
                BadRequestException => (int)HttpStatusCode.BadRequest,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                ForbiddenException => (int)HttpStatusCode.Forbidden,
                PhoneNumberValidationException => (int)HttpStatusCode.BadRequest, 
                NotSuccessfulException => (int)HttpStatusCode.BadRequest,
                BvnAlreadyUsedException => (int)HttpStatusCode.BadRequest,
                AlreadyExistException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            string message = GetMessageFromRequest(context);

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = !string.IsNullOrEmpty(message) ? message : (exception switch
                {
                    NotFoundException notFoundException => notFoundException.Message,
                    BadRequestException badRequestException => badRequestException.Message,
                    NotSuccessfulException notSuccessfulException => notSuccessfulException.Message,
                    UnauthorizedException unauthorizedException => unauthorizedException.Message,
                    ForbiddenException forbiddenException => forbiddenException.Message,
                    PhoneNumberValidationException phoneNumberValidationException => phoneNumberValidationException.Message,
                    BvnAlreadyUsedException bvnAlreadyUsedException => bvnAlreadyUsedException.Message,
                    AlreadyExistException alreadyExistException => alreadyExistException.Message,
                    _ => "An unexpected error occurred. Please try again later."
                })
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            };

            string jsonResponse = System.Text.Json.JsonSerializer.Serialize(response, jsonOptions);

            using (TextWriter writer = new StringWriter())
            {
                await writer.WriteAsync(jsonResponse);
                await writer.FlushAsync();

                await context.Response.WriteAsync(writer.ToString());
            }
        }

        private string GetMessageFromRequest(HttpContext context)
        {
            return context.Request.Query["message"];
        }

    }
}
