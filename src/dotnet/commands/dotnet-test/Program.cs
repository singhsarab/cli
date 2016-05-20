// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.DotNet.Tools.Test
{
    public class TestCommand
    {
        private readonly IDotnetTestRunnerFactory _dotnetTestRunnerFactory;

        public TestCommand(IDotnetTestRunnerFactory testRunnerFactory)
        {
            _dotnetTestRunnerFactory = testRunnerFactory;
        }

        public int DoRun(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            var dotnetTestParams = new DotnetTestParams();

            dotnetTestParams.Parse(args);

            try
            {
                if (dotnetTestParams.Help)
                {
                    return 0;
                }

                // Register for parent process's exit event
                if (dotnetTestParams.ParentProcessId.HasValue)
                {
                    RegisterForParentProcessExit(dotnetTestParams.ParentProcessId.Value);
                }

                var projectContexts = CreateProjectContexts(dotnetTestParams.ProjectPath, dotnetTestParams.Runtime);

                var projectContext = projectContexts.First();

                var testRunner = projectContext.ProjectFile.TestRunner;

                IDotnetTestRunner dotnetTestRunner = _dotnetTestRunnerFactory.Create(dotnetTestParams.Port);

                return dotnetTestRunner.RunTests(projectContext, dotnetTestParams);
            }
            catch (InvalidOperationException ex)
            {
                TestHostTracing.Source.TraceEvent(TraceEventType.Error, 0, ex.ToString());
                return -1;
            }
            catch (Exception ex)
            {
                TestHostTracing.Source.TraceEvent(TraceEventType.Error, 0, ex.ToString());
                return -2;
            }
        }

        public static int Run(string[] args)
        {
            var testCommand = new TestCommand(new DotnetTestRunnerFactory());

            return testCommand.DoRun(args);
        }

        private static void RegisterForParentProcessExit(int id)
        {
            var parentProcess = Process.GetProcesses().FirstOrDefault(p => p.Id == id);

            if (parentProcess != null)
            {
                parentProcess.EnableRaisingEvents = true;
                parentProcess.Exited += (sender, eventArgs) =>
                {
                    TestHostTracing.Source.TraceEvent(
                        TraceEventType.Information,
                        0,
                        "Killing the current process as parent process has exited.");

                    Process.GetCurrentProcess().Kill();
                };
            }
            else
            {
                TestHostTracing.Source.TraceEvent(
                    TraceEventType.Information,
                    0,
                    "Failed to register for parent process's exit event. " +
                    $"Parent process with id '{id}' was not found.");
            }
        }

        public static string[] ConvertToVsTestArgs(string[] args)
        {
            var translatedArgs = new List<string>();
            if (args == null || args.Length == 0)
            {
                return translatedArgs.ToArray();
            }

            if (!(args[0].StartsWith("--") || args[0].StartsWith("-")))
            {
                translatedArgs.Add(args[0]);
                args = args.Skip(1).ToArray();
            }

            string commandOption = null;
            var commandOptionArgs = new List<string>();

            foreach (var arg in args)
            {
                // arg is either an commandOption or its arguments
                if (arg.StartsWith("--") || arg.StartsWith("-"))
                {
                    if (!string.IsNullOrEmpty(commandOption))
                    {
                        // Add the previous commandOption with its arguments
                        translatedArgs.Add(
                            commandOptionArgs.Count > 0 ? string.Concat(commandOption, ":", string.Join(",", commandOptionArgs)) : commandOption);
                    }

                    // We have a new commandOption, clear the args list
                    commandOption = arg.Replace("--", "/").Replace("-", "/");
                    commandOptionArgs.Clear();
                }
                else
                {
                    // Add arg to list
                    commandOptionArgs.Add(arg);
                }
            }

            // Add the last commandOption with its arguments
            if (!string.IsNullOrEmpty(commandOption))
            {
                translatedArgs.Add(
                    commandOptionArgs.Count > 0 ? string.Concat(commandOption, ":", string.Join(",", commandOptionArgs)) : commandOption);
            }

            return translatedArgs.ToArray();
        }

        private static IEnumerable<ProjectContext> CreateProjectContexts(string projectPath, string runtime)
        {
            projectPath = projectPath ?? Directory.GetCurrentDirectory();

            if (!projectPath.EndsWith(Project.FileName))
            {
                projectPath = Path.Combine(projectPath, Project.FileName);
            }

            if (!File.Exists(projectPath))
            {
                throw new InvalidOperationException($"{projectPath} does not exist.");
            }

            var runtimeIdentifiers = !string.IsNullOrEmpty(runtime) ?
                new[] { runtime } :
                PlatformServices.Default.Runtime.GetAllCandidateRuntimeIdentifiers();

            return ProjectContext.CreateContextForEachFramework(projectPath).Select(context => context.CreateRuntimeContext(runtimeIdentifiers));
        }
    }
}