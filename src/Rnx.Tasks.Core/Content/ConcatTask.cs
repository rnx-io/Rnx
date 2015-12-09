using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Common.Execution;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Common.Buffers;
using Rnx.Tasks.Core.FileSystem;

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
            var elementFactory = GetService<IBufferElementFactory>(executionContext);
            var inputElements = input.Elements.ToArray();
            var concatedText = string.Join(_separator, inputElements.Select(f => f.Text).ToArray());

            var newElement = elementFactory.Create(concatedText);
            newElement.Data.Add(new WriteFileData(_targetFilepath));
            output.Add(newElement);
        }
    }
}