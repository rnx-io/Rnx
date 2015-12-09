using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Common.Execution;
using Rnx.Common.Buffers;

namespace Rnx.Tasks.Core.Content
{
    public class PrependTask : RnxTask
    {
        private string _textToPrepend;

        public PrependTask(string textToPrepend)
        {
            _textToPrepend = textToPrepend;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Text = _textToPrepend + e.Text;
                output.Add(e);
            }
        }
    }
}