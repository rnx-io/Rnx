using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;

namespace Rnx.Tasks.Core.FileSystem
{
    public class SetFilePathTaskDescriptor : TaskDescriptorBase<SetFilePathTask>
    {
        public Func<IBufferElement, string> ElementFilePath { get; }
        public Func<IBufferElement, bool> WhereCondition { get; private set; } = e => true;

        public SetFilePathTaskDescriptor(Func<IBufferElement, string> elementFilePath)
        {
            ElementFilePath = elementFilePath;
        }

        public SetFilePathTaskDescriptor Where(Func<IBufferElement,bool> whereCondition)
        {
            WhereCondition = whereCondition ?? (e => true);
            return this;
        }
    }

    public class SetFilePathTask : RnxTask
    {
        private readonly SetFilePathTaskDescriptor _taskDescriptor;

        public SetFilePathTask(SetFilePathTaskDescriptor taskDescriptor)
        {
            _taskDescriptor = taskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                if (_taskDescriptor.WhereCondition(e))
                {
                    e.Data.Add(new WriteFileData(_taskDescriptor.ElementFilePath(e)));
                }

                output.Add(e);
            }
        }
    }
}