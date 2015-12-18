using Rnx.Abstractions.Execution;
using Rnx.Core.Buffers;
using Rnx.Tasks.Core.Content;
using Rnx.Tasks.Core.Control;
using System.Linq;
using Xunit;

namespace Rnx.Tasks.Core.Tests.Control
{
    public class BlockWiseTaskTest : TestBase
    {
        [Theory]
        [InlineData(9, 5, false, false, 2, new string[] { "0(1)1(1)2(1)3(1)4", "5(2)6(2)7(2)8" } )]
        [InlineData(9, 5, false, true, 2, new string[] { "0(1)1(1)2(1)3(1)4", "5(2)6(2)7(2)8" })]
        [InlineData(9, 5, true, false, 2, new string[] { "0(1)1(1)2(1)3(1)4", "5(2)6(2)7(2)8" })]
        [InlineData(9, 5, true, true, 2, new string[] { "0(1)1(1)2(1)3(1)4", "5(2)6(2)7(2)8" })]
        [InlineData(10, 5, false, false, 2, new string[] { "0(1)1(1)2(1)3(1)4", "5(2)6(2)7(2)8(2)9" })]
        [InlineData(1, 5, false, false, 1, new string[] { "0" })]
        [InlineData(2, 1, false, false, 2, new string[] { "0", "1" })]
        public void Test_That_Blocks_Are_Created(int inputBufferSize, int blockSize, bool requiresDetailedBlockInfo, bool allowParallel,
                        int expectedNumberOfBlocks, string[] expectedElementTexts)
        {
            // Arrange
            var inputBuffer = GenerateContentBuffer(inputBufferSize);
            var taskDescriptor = new BlockWiseTaskDescriptor(blockSize, 
                                                block => new ConcatTextTaskDescriptor("bla", $"({block.CurrentBlockIndex})"),
                                                requiresDetailedBlockInfo)
                                                .AllowParallelExecution(allowParallel);
            var executionContext = new ExecutionContext(taskDescriptor, baseDirectory: "NotUsed");
            var outputBuffer = new BlockingBuffer();
            
            // Act
            ExecuteTask(taskDescriptor, inputBuffer, outputBuffer, executionContext);

            // Assert
            var elements = outputBuffer.Elements.ToArray();

            Assert.Equal(expectedNumberOfBlocks, elements.Length);

            foreach(var expectedText in expectedElementTexts)
            {
                Assert.True(elements.Any(f => f.Text == expectedText));
            }
        }
    }
}