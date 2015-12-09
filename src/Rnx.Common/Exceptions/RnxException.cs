using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Exceptions
{
    public class RnxException : Exception
    {
        public RnxException(string message)
            : base(message)
        {}
    }
}
