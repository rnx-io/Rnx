using Rnx.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Core.Exceptions
{
    public class CodeCompilationException : RnxException
    {
        public CodeCompilationException(string message)
            : base(message)
        {}
    }
}
