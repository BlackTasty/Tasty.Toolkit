using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Tests.Base
{
    public static class TestRunner
    {
        private static int testCount = 0;
        private static int failedTests = 0;

        public static int TestCount => testCount;

        public static int FailedTests = failedTests;

        /// <summary>
        /// Main function to run tests
        /// </summary>
        /// <param name="testMethod">The method which runs the actual test. Always has to return a <see cref="bool"/>!</param>
        /// <param name="testTitle">The title of the test. Will be printed to console.</param>
        /// <param name="testResultText">The result text.</param>
        public static void RunTest(Func<bool> testMethod, string testTitle)
        {
            string testName = testMethod.Method.Name.Replace("Test_", "");

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("Test {0} - ", testCount + 1);
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.Write(testMethod.Method.Name);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write(": Testing ");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.Write(testName);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine(" method...");
            //Console.WriteLine("Test {0} ({1}): {2}...", testCount + 1, testName, testTitle);
            System.Console.ForegroundColor = ConsoleColor.Gray;

            bool testSuccess = testMethod();
            if (!testSuccess)
            {
                System.Console.WriteLine("Test aborted!");
            }
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine_Status(string.Format("{0} completed", testName), testSuccess);
            System.Console.ForegroundColor = ConsoleColor.Gray;
            System.Console.WriteLine();
            testCount++;
            if (!testSuccess)
            {
                failedTests++;
            }
        }
    }
}
