// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using Microsoft.DotNet.Tools.Test;

namespace Microsoft.Dotnet.Tools.Test.Tests
{
    public class TestCommandTests
    {
        [Fact]
        public void ConvertToVsTestArgsForNullArgsReturnsEmptyArgsArray()
        {
            Assert.Equal(new string[0], TestCommand.ConvertToVsTestArgs(null));
        }

        [Fact]
        public void ConvertToVsTestArgsForEmptyArgsArrayReturnsEmptyArgsArray()
        {
            Assert.Equal(new string[0], TestCommand.ConvertToVsTestArgs(new string[0]));
        }

        [Fact]
        public void ConvertToVsTestArgsForSingleArgReturnsArrayWithArg()
        {
            var args = new[] { "sourceArg" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "sourceArg" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForLongCommandOptionWithoutArg()
        {
            var args = new[] { "--optionWithoutArg" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "/optionWithoutArg" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForSmallCommandOptionWithoutArg()
        {
            var args = new[] { "-owa" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "/owa" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForLongCommandOptionWithOneArg()
        {
            var args = new[] { "--optionWithOneArg", "arg1" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "/optionWithOneArg:arg1" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForSmallCommandOptionWithOneArg()
        {
            var args = new[] { "-owoa", "arg1" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "/owoa:arg1" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForLongCommandOptionWithMultipleArgs()
        {
            var args = new[] { "-optionWithMultipleArgs", "arg1", "arg2", "arg3" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "/optionWithMultipleArgs:arg1,arg2,arg3" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForSmallCommandOptionWithMultipleArgs()
        {
            var args = new[] { "-owma", "arg1", "arg2", "arg3" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "/owma:arg1,arg2,arg3" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForMixedArgs()
        {
            var args = new[] { "sourceArg", "-owma", "arg1", "arg2", "arg3", "--optionWithOneArg", "arg" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "sourceArg", "/owma:arg1,arg2,arg3", "/optionWithOneArg:arg" };
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConvertToVsTestArgsForMixedArgs2()
        {
            var args = new[] { "sourceArg", "-owma", "arg1", "arg2", "arg3", "--optionWithOneArg", "arg", "--helloWorld", "foo", "bar" };
            var result = TestCommand.ConvertToVsTestArgs(args);
            var expectedResult = new[] { "sourceArg", "/owma:arg1,arg2,arg3", "/optionWithOneArg:arg", "/helloWorld:foo,bar" };
            Assert.Equal(expectedResult, result);
        }
    }
}
