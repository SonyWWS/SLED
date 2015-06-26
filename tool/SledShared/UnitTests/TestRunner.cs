/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.IO;
using System.Reflection;

using NUnit.Core;

namespace Sce.Sled.Shared.UnitTests
{
    class SledSharedEventListener : EventListener
    {
        public SledSharedEventListener()
        {
            TestCount = 0;
            FailedTestCount = 0;
        }

        public void RunFinished(TestResult result) { }
        public void RunFinished(Exception e) { }
        public void RunStarted(String s, int i) { }
        public void SuiteFinished(TestSuiteResult result) { }
        public void SuiteStarted(TestName name) { }
        public void TestOutput(TestOutput output) { }
        public void TestStarted(TestName name) { }
        public void UnhandledException(Exception e) { }

        public void TestFinished(TestCaseResult result)
        {
            ++TestCount;

            if (result.IsFailure == false)
                return;

            String failureText;

            var stackTrace = result.StackTrace;
            if (String.IsNullOrEmpty(stackTrace) == false)
            {
                var stackTraceSplit =
                    stackTrace.Split(new[] { "  at " }, StringSplitOptions.RemoveEmptyEntries);

                var failSiteIndex = (stackTraceSplit.Length - 1);
                var failSite = stackTraceSplit[failSiteIndex];

                var failSiteSplit =
                    failSite.Split(new[] { " in " }, StringSplitOptions.RemoveEmptyEntries);

                var fileLine = failSiteSplit[1];
                var fileLineSplit =
                    fileLine.Split(new[] { ":line " }, StringSplitOptions.None);

                var failingTest = failSiteSplit[0];
                var file = fileLineSplit[0];
                var line = fileLineSplit[1];

                var message = String.Empty;
                if (String.IsNullOrEmpty(result.Message) == false)
                    message = "\r\n" + result.Message;

                failureText = file + "(" + line + "): error UT0000: " + failingTest + message;
            }
            else
                failureText = result.Name + ": " + result.Message;

            m_consoleOut.WriteLine(failureText);
            m_consoleOut.Flush();

            ++FailedTestCount;
        }

        public int TestCount { get; private set; }

        public int FailedTestCount { get; private set; }

        public TextWriter ConsoleOut
        {
            set { m_consoleOut = value; }
        }

        private TextWriter m_consoleOut;
    }

    public static class TestRunner
    {
        public static int RunAllTests()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyFilename = executingAssembly.Location;

            var consoleOut = Console.Out;

            var testPackage =
                new TestPackage(assemblyFilename) {AutoBinPath = true};
            testPackage.Settings["ShadowCopyFiles"] = false;
            testPackage.Settings["UseThreadedRunner"] = false;

            var listener =
                new SledSharedEventListener {ConsoleOut = consoleOut};

            var testRunner = new RemoteTestRunner();
            testRunner.Load(testPackage);
            testRunner.Run(listener);

            if (listener.FailedTestCount == 0)
                consoleOut.WriteLine("Success: " + listener.TestCount + " test(s) passed.");

            return listener.FailedTestCount;
        }
    }
}
