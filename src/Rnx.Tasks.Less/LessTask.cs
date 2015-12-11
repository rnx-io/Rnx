using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Tasks.Core.FileSystem;

namespace Rnx.Tasks.Less
{
    public class LessTask : RnxTask
    {
        private string _newExtension = ".css";

        public LessTask WithExtension(string newExtension)
        {
            _newExtension = newExtension;
            return this;
        }

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            foreach (var e in input.Elements)
            {
                e.Text = dotless.Core.Less.Parse(e.Text);

                if (e.Data.Exists<WriteFileData>())
                {
                    e.Data.Get<WriteFileData>().Extension = _newExtension;
                }

                output.Add(e);
            }
        }
    }
}