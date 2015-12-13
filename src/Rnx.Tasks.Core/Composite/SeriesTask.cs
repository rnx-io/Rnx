using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Tasks.Core.Composite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rnx.Tasks.Core.Composite
{
    public class SeriesTask : MultiTask
    {
        public SeriesTask(params ITask[] tasks) : base(tasks)
        {}

        public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
        {
            var bufferFactory = GetBufferFactory(executionContext);
            var taskExecuter = GetTaskExecuter(executionContext);

            var taskFactory = new TaskFactory(TaskCreationOptions.None, TaskContinuationOptions.None);
            var stages = new List<Task>();
            var buffers = new List<IBuffer>();
            Action firstAction = () => { };
            buffers.Add(input);

            for (int i = 0; i < Tasks.Length; ++i)
            {
                var t = Tasks[i];
                IBuffer taskInputBuffer = buffers.LastOrDefault();
                IBuffer taskOutputBuffer = i == (Tasks.Length - 1) ? output : bufferFactory.Create();
                Action executeAction = () => taskExecuter.Execute(t, taskInputBuffer, taskOutputBuffer, executionContext);

                if (i == 0)
                {
                    firstAction = executeAction;
                }
                else
                {
                    taskInputBuffer.Ready += (s, e) =>
                    {
                        var nextStage = taskFactory.StartNew(executeAction);
                        stages.Add(nextStage);
                    };
                }

                buffers.Add(taskOutputBuffer);
            }

            // Run the first action
            stages.Add(taskFactory.StartNew(firstAction));

            while (true)
            {
                var stage = stages.FirstOrDefault();

                if (stage == null)
                {
                    break;
                }

                stage.Wait();
                stages.RemoveAt(0);
            }

            // we dispose only our created buffers (i.e. we do not dispose the incoming input and output buffers)
            foreach (var buffer in buffers.Where(f => f != input && f != output))
            {
                buffer.Dispose();
            }
        }

        protected virtual IBufferFactory GetBufferFactory(IExecutionContext ctx) => RequireService<IBufferFactory>(ctx);
    }
}