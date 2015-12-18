using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.FileSystem;

namespace Rnx.Tasks.Markdown
{
    public class MarkdownTaskDescriptor : TaskDescriptorBase<MarkdownTask>
    {
        public string NewExtension { get; private set; } = ".html";

        public MarkdownTaskDescriptor WithExtension(string newExtension)
        {
            NewExtension = newExtension;
            return this;
        }
    }

    public class MarkdownTask : RnxTask
    {
        private readonly MarkdownTaskDescriptor _taskDescriptor;

        public MarkdownTask(MarkdownTaskDescriptor taskDescriptor)
        {
            _taskDescriptor = taskDescriptor;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Text = CommonMark.CommonMarkConverter.Convert(e.Text);

                if(e.Data.Exists<WriteFileData>())
                {
                    e.Data.Get<WriteFileData>().Extension = _taskDescriptor.NewExtension;
                }

                output.Add(e);
            }
        }
    }
}