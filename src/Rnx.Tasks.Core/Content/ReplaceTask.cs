using Rnx.Abstractions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Buffers;

namespace Rnx.Tasks.Core.Content
{
    public class ReplaceTask : RnxTask
    {
        private string _searchText;
        private string _replacement;

        public ReplaceTask(string searchText, string replacement)
        {
            _searchText = searchText;
            _replacement = replacement;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach(var e in input.Elements)
            {
                e.Text = e.Text.Replace(_searchText, _replacement);
                output.Add(e);
            }
        }
    }
}