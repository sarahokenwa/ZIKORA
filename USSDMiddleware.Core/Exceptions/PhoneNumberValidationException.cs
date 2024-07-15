using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Exceptions
{
    public class PhoneNumberValidationException : Exception
    {
        public PhoneNumberValidationException(string message) : base(message)
        {
        }
    }
}
