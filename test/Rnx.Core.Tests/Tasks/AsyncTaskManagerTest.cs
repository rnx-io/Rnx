using Rnx.Common.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rnx.Common.Execution;
using Rnx.Core.Tasks;
using Rnx.Core.Execution;

namespace Rnx.Core.Tests.Tasks
{
    public class AsyncTaskManagerTest
    {
        [Fact]
        public void Test_That_No_Registered_Tasks_Work()
        {
            // Arrange
            var asyncTaskManager = new DefaultAsyncTaskManager();

            // Act

            // Assert
            Assert.False(asyncTaskManager.HasUncompletedTasks);
            asyncTaskManager.WaitAll();
            Assert.True(true);
        }

        [Fact]
        public void Test_That_WaitForCompletion_For_All_Tasks_Works()
        {
            // Arrange
            var asyncTaskManager = new DefaultAsyncTaskManager();
            var executionContext = new ExecutionContext("Clean", null, null);

            // Act
            var asyncTask = new AsyncTestTask();
            asyncTaskManager.RegisterAsyncExecution(asyncTask, executionContext);
            asyncTask.Execute(null, null, executionContext);

            // Assert
            Assert.True(asyncTaskManager.HasUncompletedTasks);
            asyncTaskManager.WaitAll();
            Assert.True(true);
        }

        [Theory]
        [InlineData("Clean")]
        [InlineData(null)]
        [InlineData("UnknownExecutionId")]
        public void Test_That_WaitForCompletion_For_Single_Task_Works(string executionId)
        {
            // Arrange
            var asyncTaskManager = new DefaultAsyncTaskManager();
            var executionContext = new ExecutionContext("Clean", null, null);

            // Act
            var asyncTask = new AsyncTestTask();
            asyncTaskManager.RegisterAsyncExecution(asyncTask, executionContext);
            asyncTask.Execute(null, null, executionContext);

            // Assert
            asyncTaskManager.WaitForTaskCompletion(executionContext, executionId);
            Assert.True(true);
        }
    }
}
