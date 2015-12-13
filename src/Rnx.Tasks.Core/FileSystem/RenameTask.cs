using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Util;
using Rnx.Abstractions.Buffers;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.FileSystem
{
    public class RenameTask : RnxTask
    {
        private Action<IBufferElement, WriteFileData> _action;

        public RenameTask(Action<WriteFileData> action)
            : this(new Action<IBufferElement, WriteFileData>((e,f) => action(f)))
        { }

        public RenameTask(Action<IBufferElement, WriteFileData> action)
        {
            _action = action;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Data.WhenExists<WriteFileData>(f => _action(e, f));
                output.Add(e);
            }
        }
    }
}