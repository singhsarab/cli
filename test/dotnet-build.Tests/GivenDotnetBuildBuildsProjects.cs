// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.Tools.Compiler;
using Moq;
using NuGet.Frameworks;
using Xunit;

namespace Microsoft.DotNet.Tools.Builder.Tests
{
    public class GivenDotnetBuildBuildsProjects
    {
        [Fact]
        public void It_builds_projects_with_Unicode_characters_in_the_path_when_CWD_is_the_project_directory()
        {
            var testInstance = TestAssetsManager
                .CreateTestInstance("TestAppWithUnicodéPath", identifier: testIdentifer)
                .WithLockFiles();

            var testProjectDirectory = testInstance.TestRoot;

            new BuildCommand("")
                .WorkingDirectory(testProjectDirectory)
                .ExecuteWithCapturedOutput()
                .Should()
                .Pass();
        }

        [Fact]
        public void It_builds_projects_with_Unicode_characters_in_the_path_when_CWD_is_the_project_directory()
        {
            var testInstance = TestAssetsManager
                .CreateTestInstance("TestAppWithUnicodéPath", identifier: testIdentifer)
                .WithLockFiles();

            var testProject = Path.Combine(testInstance.TestRoot, "project.json");

            new BuildCommand(testProject)
                .ExecuteWithCapturedOutput()
                .Should()
                .Pass();
        }
    }
}
