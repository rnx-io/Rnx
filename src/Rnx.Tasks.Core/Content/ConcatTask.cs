using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.FileSystem;
using System.Linq;

namespace Rnx.Tasks.Core.Content
{
    public class ConcatTask : RnxTask
    {
        private string _targetFilepath;
        private string _separator;

        public ConcatTask(string targetFilepath, string separator = "")
        {
            _targetFilepath = targetFilepath;
            _separator = separator;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferElementFactory = GetBufferElementFactory(executionContext);
            var inputElements = input.Elements.ToArray();
            var concatedText = string.Join(_separator, inputElements.Select(f => f.Text).ToArray());

            var newElement = bufferElementFactory.Create(concatedText);
            newElement.Data.Add(new WriteFileData(_targetFilepath));
            output.Add(newElement);
        }

        protected virtual IBufferElementFactory GetBufferElementFactory(IExecutionContext ctx) => RequireService<IBufferElementFactory>(ctx);
    }
}