using Rnx.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader.Compilation
{
    public class CodeCompilationException : RnxException
    {
        public CodeCompilationException(string message)
            : base(message)
        {}
    }
}