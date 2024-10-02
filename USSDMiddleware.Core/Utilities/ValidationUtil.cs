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
            // Handle number parse exception
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

//public class ValidationUtil
//{
//    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

//    public static void Validate(ValidationModel request)
//    {
//        if (string.IsNullOrEmpty(request.PhoneNumber))
//        {
//            ValidatePhone(request.PhoneNumber);
//        }


//        if (string.IsNullOrEmpty(request.Bvn))
//        {
//            if (!IsValidBvn(request.Bvn))
//            {
//                throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST,
//                    "Invalid BVN format!, BVN must be 11 digits");
//            }
//        }
//    }


//    private static void ValidatePhone(string? phone)
//    {
//        try
//        {
//            var parsedNumber = PhoneUtil.Parse(phone, "NG");
//            if (PhoneUtil.IsValidNumber(parsedNumber))
//            {
//                return;
//            }
//        }
//        catch (NumberParseException)
//        {
//            //todo
//        }

//        throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST, "Phone number is invalid!");
//    }


//    private static bool IsValidBvn(string? bvn)
//    {
//        // Check length
//        if (bvn.Length != 11)
//        {
//            return false;
//        }

//        // Check if all characters are digits
//        for (int i = 0; i < bvn.Length; i++)
//        {
//            if (Char.IsDigit(bvn[i]))
//            {
//                return false;
//            }
//        }

//        return true;
//    }
//}