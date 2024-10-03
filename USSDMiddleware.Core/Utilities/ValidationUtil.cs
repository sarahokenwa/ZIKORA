using PhoneNumbers;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Models;

namespace USSDMiddleware.Core.Utilities;

public class ValidationUtil
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    public static string Validate(ValidationModel request)
    {
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            request.PhoneNumber = $"234{request.PhoneNumber.GetLast(10)}"; 
            ValidatePhone(request.PhoneNumber);
        }

        return request.PhoneNumber; 
    }

    private static void ValidatePhone(string? phone)
    {
        try
        {
            var parsedNumber = PhoneUtil.Parse(phone, "NG");
            if (PhoneUtil.IsValidNumber(parsedNumber))
            {
                return;
            }
        }
        catch (NumberParseException)
        {
            throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST, $"Phone number parsing failed");
        }

        throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST, "Phone number is invalid!");
    }
}

public static class StringExtensions
{
    public static string GetLast(this string source, int tailLength)
    {
        if (tailLength >= source.Length)
            return source;
        return source.Substring(source.Length - tailLength);
    }
}

