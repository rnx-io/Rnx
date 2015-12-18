using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Execution;
using Rnx.Abstractions.Tasks;
using Rnx.Core.Buffers;
using Rnx.Tasks.Core.Content;
using Rnx.Tasks.Core.Control;
using Rnx.Tasks.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rnx.Tasks.Core.Tests.Control
{
    public class ParallelTaskTest : TestBase
    {
        [Theory]
        [InlineData(ParallelTaskOutputStrategy.ConcatToInput, 15, new string[] { "{0}", "a{0}", "{0}z" })]
        [InlineData(ParallelTaskOutputStrategy.ReplaceInput, 10, new string[] { "a{0}", "{0}z" })]
        [InlineData(ParallelTaskOutputStrategy.DoNotChangeInput, 5, new string[] { "{0}" })]
        public void Test_That_Parallel_Task_Output_Strategy_Works(ParallelTaskOutputStrategy strategy, int expectedCount, string[] expectedFormatStrings)
        {
            // Arrange
            var taskDescriptor = new ParallelTaskDescriptor(strategy,
                                new PrependTextTaskDescriptor("a"), new AppendTextTaskDescriptor("z"));
            var executionContext = new ExecutionContext(taskDescriptor, baseDirectory: "NotUsed");
            var dummyBuffer = new BlockingBuffer();

            // pre-fill outputBuffer (creates 5 elements from "0" to "4")
            const int ELEMENTS_TO_GENERATE = 5;
            ExecuteTask(new GenerateContentTaskDescriptor(ELEMENTS_TO_GENERATE), new NullBuffer(), dummyBuffer, executionContext);
            var outputBuffer = new BlockingBuffer();

            // Act
            ExecuteTask(taskDescriptor, dummyBuffer, outputBuffer, executionContext);
            var elements = outputBuffer.Elements.ToArray();

            // Assert
            Assert.Equal(expectedCount, elements.Length);

            for (int i = 0; i < ELEMENTS_TO_GENERATE; ++i)
            {
                for(int k = 0; k < expectedFormatStrings.Length; ++k)
                {
                    Assert.Equal(1, elements.Count(f => f.Text == string.Format(expectedFormatStrings[k], i)));
                }
            }
        }
    }
}