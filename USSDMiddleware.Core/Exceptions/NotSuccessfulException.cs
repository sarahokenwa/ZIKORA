using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Exceptions
{
    public class NotSuccessfulException : Exception
    {
        public NotSuccessfulException(string message) : base(message) { }
    }
}
