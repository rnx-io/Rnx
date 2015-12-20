#if DEBUG

using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Rnx.Tasks.Core.FileSystem;
using Rnx.Tasks.Core.Control;
//using Rnx.Tasks.Core.Threading;
//using static Rnx.Tasks.Core.Tasks;
using Rnx.Tasks.Core.Compression;
using Rnx.Abstractions.Util;
using Rnx.Tasks.Core.Process;

namespace Rnx
{
    public class ExampleConfig
    {
        public ITaskDescriptor PrintConsole => new SeriesTaskDescriptor(
            //Async(new AsyncCompleteTestTask(), "asynctest"),
            //Parallel(new AsyncCompleteTestTask(), new AsyncCompleteTestTask()),
            new MyTaskDescriptor(),
            new IfTaskDescriptor(f => int.Parse(f.Text) % 2 == 0, new MySpecialTaskDescriptor()),
            new MyPrinterTaskDescriptor()
            //new MyModifierTask()//,
            //CopyFiles("*.json", "tmp/output")
            //Await("asynctest", (e, i, o, c) => { Console.WriteLine("Await task says: " + e.AsyncTask.Name + " completed. Element: " + e.OutputBuffer.Elements.First().Text); })
        );

        public ITaskDescriptor RunProcess => new StartProcessTaskDescriptor(@"C:\Users\dan\Documents\visual studio 2015\Projects\ConsoleApplication11\ConsoleApplication11\bin\Debug\ConsoleApplication11.exe", "Daniel");

        //public ITask ZipTest => Series(
        //    ReadFiles("*.cs"),
        //    Zip("test.zip"),
        //    WriteFiles("tmp/output")
        //);

        //public ITask UnzipTest => Series(
        //    ReadFiles("tmp/output/*.zip"),
        //    Unzip(),
        //    Rename(f => { f.Extension += ".unzipped"; }),
        //    WriteFiles("tmp/unzip")
        //);

        public class MyTaskDescriptor : TaskDescriptorBase<MyTask>
        { }

        public class MyTask : RnxTask
        {
            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                //var logger = GetService<ILoggerFactory>(executionContext).CreateLogger(executionContext.UserDefinedTaskName);

                for (int i = 0; i < 10; ++i)
                {
                    output.Add(new MyElement(i.ToString()));
                    //System.Threading.Thread.Sleep(50);
                }

                //logger.LogWarning($"Hello Rnx! This task was executed inside the user defined task '{executionContext.UserDefinedTaskName}'");
            }
        }

        public class MySpecialTaskDescriptor : TaskDescriptorBase<MySpecialTask>
        { }

        public class MySpecialTask : RnxTask
        {
            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                output.Add(new MyElement(string.Join(", ", input.Elements.Select(f => f.Text).ToArray())));
            }
        }

        public class MyPrinterTaskDescriptor : TaskDescriptorBase<MyPrinterTask>
        { }

        public class MyPrinterTask : RnxTask
        {
            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                //var x = input.ToArray();

                foreach (IBufferElement e in input.Elements)
                {
                    Console.WriteLine("Processing item " + e.Text);
                }
            }
        }

        public class MyElement : IBufferElement
        {
            public MyElement(string text)
            {
                Text = text;
            }

            public IDataStore Data { get; private set; }

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