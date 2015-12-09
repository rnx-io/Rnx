using Rnx.Core.Configuration;
using Rnx.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Rnx.Common.Tasks;
using Rnx.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Extensions;
using static Rnx.Core.Tests.TestUtil;
using Rnx.Core.Execution;
using Rnx.Common.Execution;

namespace Rnx.Core.Tests.Configuration
{
    public class RnxTaskLoaderTest
    {
        [Theory, MemberData(nameof(TestData))]
        public void Test_That_Tasks_Are_Found_And_Returned(string[] taskNames, int expectedTasksCount)
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loader = new DefaultTaskLoader(serviceProvider);

            // Act
            var tasks = loader.Load(new Type[] { typeof(SimpleConfig1) }, taskNames).ToArray();

            // Assert
            Assert.Equal(expectedTasksCount, tasks.Count());
            Assert.Equal(expectedTasksCount, tasks.OfType<UserDefinedTask>().Count());
            Assert.Equal(expectedTasksCount, tasks.Cast<UserDefinedTask>().Sum(f => taskNames.Count(x => string.Equals(f.Name, x, StringComparison.OrdinalIgnoreCase))));
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return Params(new[] { "minify" }, 1);
            yield return Params(new[] { "minify", "dostuff" }, 2);
            yield return Params(new[] { "not", "defined" }, 0);
        }
    }
}
