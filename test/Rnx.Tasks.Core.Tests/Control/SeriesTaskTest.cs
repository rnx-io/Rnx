using Rnx.Abstractions.Execution;
using Rnx.Core.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Rnx.Tasks.Core.Control;
using Rnx.Core.Execution;
using Rnx.Tasks.Core.Internal;

namespace Rnx.Tasks.Core.Tests.Control
{
    public class SeriesTaskTest : TestBase
    {
        [Fact]
        public void Test_Series()
        {
            // Arrange
            var taskDescriptor = new SeriesTaskDescriptor(new GenerateContentTaskDescriptor(5), new AllowEvenNumbersTaskDescriptor());
            var executionContext = new ExecutionContext(taskDescriptor, baseDirectory: "NotUsed");
            var outputBuffer = new BlockingBuffer();

            // Act
            ExecuteTask(taskDescriptor, new NullBuffer(), outputBuffer, executionContext);

            // Assert
            var elements = outputBuffer.Elements.ToArray();

            Assert.Equal(3, elements.Length);
            Assert.True(elements.Any(f => f.Text == "0"));
            Assert.True(elements.Any(f => f.Text == "2"));
            Assert.True(elements.Any(f => f.Text == "4"));
        }

        [Theory]
        [InlineData(1, 1500, int.MaxValue)] // 1500 => (5 * 200) + (5 * 100)
        [InlineData(2, 0, 1499)]
        public void Test_That_Max_Degree_Of_Parallelism_Is_Working(int maxParallelDegree, int minRangeInclusive, int maxRangeInclusive)
        {
            // Arrange
            var taskDescriptor = new SeriesTaskDescriptor(maxParallelDegree, 
                            new GenerateContentTaskDescriptor(5),new SleepTaskDescriptor(200), new SleepTaskDescriptor(100));
            var executionContext = new ExecutionContext(taskDescriptor, baseDirectory: "NotUsed");
            var outputBuffer = new BlockingBuffer();

            // Act
            var stopwatch = Stopwatch.StartNew();
            ExecuteTask(taskDescriptor, new NullBuffer(), outputBuffer, executionContext);
            stopwatch.Stop();

            // Assert
            Assert.InRange(stopwatch.ElapsedMilliseconds, minRangeInclusive, maxRangeInclusive);
        }

        [Fact]
        public void Test_Nested_Series_With_Single_Task()
        {
            // Arrange
            var nestedSeriesTaskDescriptor = new SeriesTaskDescriptor(new GenerateContentTaskDescriptor(5));
            var taskDescriptor = new SeriesTaskDescriptor(nestedSeriesTaskDescriptor, new SleepTaskDescriptor(10), new AllowEvenNumbersTaskDescriptor());
            var executionContext = new ExecutionContext(taskDescriptor, baseDirectory: "NotUsed");
            var outputBuffer = new BlockingBuffer();

            // Act
            ExecuteTask(taskDescriptor, new NullBuffer(), outputBuffer, executionContext);

            // Assert
            var elements = outputBuffer.Elements.ToArray();

            Assert.Equal(3, elements.Length);
            Assert.True(elements.Any(f => f.Text == "0"));
            Assert.True(elements.Any(f => f.Text == "2"));
            Assert.True(elements.Any(f => f.Text == "4"));
        }

        [Theory]
        [InlineData(false, 100, int.MaxValue)]
        [InlineData(true, 0, 99)]
        public void Test_That_No_Thread_Is_Launched_For_A_Task_That_Requires_A_Completed_Input_Buffer(bool requiresCompletedInputBuffer, int minRangeInclusive, int maxRangeInclusive)
        {
            // Arrange
            var checkDescriptor = new CheckGetElementsDurationTaskDescriptor(requiresCompletedInputBuffer);
            var taskDescriptor = new SeriesTaskDescriptor(new GenerateContentTaskDescriptor(6), new SleepTaskDescriptor(20), checkDescriptor);
            var executionContext = new ExecutionContext(taskDescriptor, baseDirectory: "NotUsed");
            var outputBuffer = new BlockingBuffer();

            // Act
            ExecuteTask(taskDescriptor, new NullBuffer(), outputBuffer, executionContext);

            // Assert
            // CheckGetElementsDurationTask just starts when the first element was added, i.e. after 20 ms because of sleep task and
            // therefore check lower than 5 * 20 instead of 6 * 20
            Assert.InRange(checkDescriptor.ElapsedMilliseconds, minRangeInclusive, maxRangeInclusive);
        }

        private class CheckGetElementsDurationTaskDescriptor : TaskDescriptorBase<CheckGetElementsDurationTask>
        {
            public long ElapsedMilliseconds { get; set; }

            public CheckGetElementsDurationTaskDescriptor(bool requiresCompletedInputBuffer)
            {
                RequiresCompletedInputBuffer = requiresCompletedInputBuffer;
            }
        }

        private class CheckGetElementsDurationTask : RnxTask
        {
            private CheckGetElementsDurationTaskDescriptor _taskDescriptor;

            public CheckGetElementsDurationTask(CheckGetElementsDurationTaskDescriptor taskDescriptor)
            {
                _taskDescriptor = taskDescriptor;
            }

            public override void Execute(IBuffer input, IBuffer output, IExecutionContext executionContext)
            {
                var stopwatch = Stopwatch.StartNew();
                // this should be very short when requiresCompletedInputBuffer is set to true, because all elements must be present
                // otherwise it blocks until previous elements were processed
                var allElements = input.Elements.ToArray();
                stopwatch.Stop();

                _taskDescriptor.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            }
        }
    }
}