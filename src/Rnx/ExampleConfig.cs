#if DEBUG

using Rnx.Common.Buffers;
using Rnx.Common.Execution;
using Rnx.Common.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Rnx.Tasks.Core.FileSystem;
using Rnx.Tasks.Core.Composite;
using Rnx.Tasks.Core.Control;
using Rnx.Tasks.Core.Threading;
using Rnx.Tasks.Core.Compression;
using static Rnx.Tasks.Core.Tasks;

namespace Rnx
{
    public class ExampleConfig
    {
        public ITask PrintConsole => Series(
            //Async(new AsyncCompleteTestTask(), "asynctest"),
            //Parallel(new AsyncCompleteTestTask(), new AsyncCompleteTestTask()),
            new MyTask(),
            If(f => int.Parse(f.Text) % 2 == 0, new MySpecialTask()),
            new MyModifierTask()//,
            //CopyFiles("*.json", "tmp/output")
            //Await("asynctest", (e, i, o, c) => { Console.WriteLine("Await task says: " + e.AsyncTask.Name + " completed. Element: " + e.OutputBuffer.Elements.First().Text); })
        );

        public ITask ZipTest => Series(
            ReadFiles("*.cs"),
            Zip("test.zip"),
            WriteFiles("tmp/output")
        );

        public ITask UnzipTest => Series(
            ReadFiles("tmp/output/*.zip"),
            Unzip(),
            Rename(f => { f.Extension += ".unzipped"; }),
            WriteFiles("tmp/unzip")
        );

        public class MyTask : RnxTask
        {
            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                var logger = GetService<ILoggerFactory>(executionContext).CreateLogger(executionContext.UserDefinedTaskName);

                for (int i = 0; i < 10; ++i)
                {
                    output.Add(new MyElement(i.ToString()));
                    //System.Threading.Thread.Sleep(50);
                }

                logger.LogWarning($"Hello Rnx! This task was executed inside the user defined task '{executionContext.UserDefinedTaskName}'");
            }
        }

        public class AsyncCompleteTestTask : RnxTask
        {
            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                System.Threading.Thread.Sleep(1000);
                output.Add(new MyElement("Hello Async"));
            }
        }

        public class MySpecialTask : RnxTask
        {
            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                output.Add(new MyElement(string.Join(", ", input.Elements.Select(f => f.Text).ToArray())));
            }
        }

        public class MyModifierTask : RnxTask
        {
            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                var logger = GetService<ILoggerFactory>(executionContext).CreateLogger(executionContext.UserDefinedTaskName);

                //var x = input.ToArray();

                foreach (IBufferElement e in input.Elements)
                {
                    logger.LogInformation("Processing item " + e.Text);
                }
            }
        }

        public class MyElement : IBufferElement
        {
            public MyElement(string text)
            {
                Text = text;
            }

            public IBufferElementData Data { get; private set; }

            public bool HasText => true;

            public Stream Stream { get; set; }

            public string Text { get; set; }

            public IBufferElement Clone()
            {
                return null;
            }
        }
    }
}
#endif