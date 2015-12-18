using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Abstractions.Exceptions
{
    /// <summary>
    /// Generell exception for Rnx-specific exceptions
    /// </summary>
    public class RnxException : Exception
    {
        public RnxException(string message)
            : base(message)
        {}
    }
}
