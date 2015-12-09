using Rnx.Core.Util;
using Rnx.Tasks.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rnx.Tasks.Core.Tests.FileSystem
{
    public class FileSystemTest
    {
        [Fact]
        public void Test_Relative_Path_Include()
        {
            // Arrange
            var baseDirectory = @"e:\test\sub";
            var globPatterns = new string[] { @"**\*.txt", @"e:\bla\sub2\*.csv" };

            // Act
            var infos = DefaultFileSystem.GetMatcherInfos(baseDirectory, globPatterns).ToArray();

            // Assert
            Assert.Equal(1, infos.Length);
            Assert.Equal(baseDirectory, infos[0].BaseDirectory);
            Assert.Equal(0, infos[0].ExcludePatterns.Count());
            Assert.Equal(0, infos[0].ExcludePatterns.Count());
            Assert.True(infos[0].IncludePatterns.Contains(@"**\*.txt"));
            Assert.True(infos[0].IncludePatterns.Contains(@"..\..\bla\sub2\*.csv"));
        }

        [Fact]
        public void Test_Absolute_Path_Include()
        {
            // Arrange
            var baseDirectory = @"e:\test\sub";
            var globPatterns = new string[] { @"**\*.txt", @"c:\bla\sub2\*.csv" };

            // Act
            var infos = DefaultFileSystem.GetMatcherInfos(baseDirectory, globPatterns).ToArray();

            // Assert
            Assert.Equal(2, infos.Length);
            Assert.Equal(baseDirectory, infos[0].BaseDirectory);
            Assert.Equal(@"c:\", infos[1].BaseDirectory);
            Assert.Equal(0, infos[0].ExcludePatterns.Count());
            Assert.Equal(0, infos[1].ExcludePatterns.Count());
            Assert.True(infos[0].IncludePatterns.Contains(@"**\*.txt"));
            Assert.True(infos[1].IncludePatterns.Contains(@"bla\sub2\*.csv"));
        }

        [Fact]
        public void Test_Absolute_Path_Include_Exclude()
        {
            // Arrange
            var baseDirectory = @"e:\test\sub";
            var globPatterns = new string[] { @"!**\*.txt", @"c:\bla\sub2\*.csv" };

            // Act
            var infos = DefaultFileSystem.GetMatcherInfos(baseDirectory, globPatterns).ToArray();

            // Assert
            Assert.Equal(2, infos.Length);
            Assert.Equal(baseDirectory, infos[0].BaseDirectory);
            Assert.Equal(@"c:\", infos[1].BaseDirectory);
            Assert.Equal(0, infos[0].IncludePatterns.Count());
            Assert.Equal(1, infos[1].IncludePatterns.Count());
            Assert.True(infos[0].ExcludePatterns.Contains(@"**\*.txt"));
            Assert.True(infos[1].IncludePatterns.Contains(@"bla\sub2\*.csv"));
        }

        [Fact]
        public void Test_Multiple_Absolute_Paths_Include_Exclude()
        {
            // Arrange
            var baseDirectory = @"e:\test\sub";
            var globPatterns = new string[] { @"!**\*.txt", @"c:\bla\sub2\*.csv", @"!c:\somedir\*.exe" };

            // Act
            var infos = DefaultFileSystem.GetMatcherInfos(baseDirectory, globPatterns).ToArray();

            // Assert
            Assert.Equal(2, infos.Length);
            Assert.Equal(baseDirectory, infos[0].BaseDirectory);
            Assert.Equal(@"c:\", infos[1].BaseDirectory);
            Assert.Equal(0, infos[0].IncludePatterns.Count());
            Assert.Equal(1, infos[1].IncludePatterns.Count());
            Assert.True(infos[0].ExcludePatterns.Contains(@"**\*.txt"));
            Assert.True(infos[1].ExcludePatterns.Contains(@"somedir\*.exe"));
            Assert.True(infos[1].IncludePatterns.Contains(@"bla\sub2\*.csv"));
        }

        [Fact]
        public void Test_Expand_Glob_Pattern_Ignores_Escaped_Braces()
        {
            // Arrange
            var baseDirectory = @"e:\test\sub";
            var globPatterns = new string[] { @"**\{test,abc\}.txt", "*.{jpg,png}" };

            // Act
            var infos = DefaultFileSystem.GetMatcherInfos(baseDirectory, globPatterns).ToArray();

            // Assert
            Assert.Equal(1, infos.Length);
            Assert.Equal(3, infos[0].IncludePatterns.Count());
            Assert.Equal(@"**\{test,abc\}.txt", infos[0].IncludePatterns.First());
            Assert.Equal(@"*.jpg", infos[0].IncludePatterns.ElementAt(1));
            Assert.Equal(@"*.png", infos[0].IncludePatterns.ElementAt(2));
        }
    }
}
