using Rnx.Common.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Common.Execution
{
    public interface IExecutable
    {
        void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext);
    }
}
