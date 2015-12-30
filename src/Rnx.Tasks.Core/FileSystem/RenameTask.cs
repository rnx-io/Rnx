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
    public class RenameTaskDescriptor : TaskDescriptorBase<RenameTask>
    {
        internal Action<IBufferElement, WriteFileData> Action { get; }

        public RenameTaskDescriptor(Action<WriteFileData> action)
            : this(new Action<IBufferElement, WriteFileData>((e,f) => action(f)))
        { }

        public RenameTaskDescriptor(Action<IBufferElement, WriteFileData> action)
        {
            Action = action;
        }
    }

    public class RenameTask : RnxTask
    {
        private readonly RenameTaskDescriptor _renameTaskDescriptor;

        public RenameTask(RenameTaskDescriptor renameTaskDescriptor)
        {
            _renameTaskDescriptor = renameTaskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Data.WhenExists<WriteFileData>(f => _renameTaskDescriptor.Action(e, f));
                output.Add(e);
            }
        }
    }
}