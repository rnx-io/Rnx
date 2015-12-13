using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Content
{
    public class AppendTask : RnxTask
    {
        private string _textToAppend;

        public AppendTask(string textToAppend)
        {
            _textToAppend = textToAppend;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach (var e in input.Elements)
            {
                e.Text = e.Text + _textToAppend;
                output.Add(e);
            }
        }
    }
}