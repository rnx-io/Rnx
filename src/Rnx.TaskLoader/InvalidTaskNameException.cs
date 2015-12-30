using Rnx.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.TaskLoader
{
    public class InvalidTaskNameException : RnxException
    {
        public InvalidTaskNameException(string message)
            : base(message)
        {}
    }
}
