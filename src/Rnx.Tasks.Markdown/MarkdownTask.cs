using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using Rnx.Tasks.Core.FileSystem;

namespace Rnx.Tasks.Markdown
{
    public class MarkdownTask : RnxTask
    {
        private string _newExtension = ".html";

        public MarkdownTask WithExtension(string newExtension)
        {
            _newExtension = newExtension;
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Text = CommonMark.CommonMarkConverter.Convert(e.Text);

                if(e.Data.Exists<WriteFileData>())
                {
                    e.Data.Get<WriteFileData>().Extension = _newExtension;
                }

                output.Add(e);
            }
        }
    }
}