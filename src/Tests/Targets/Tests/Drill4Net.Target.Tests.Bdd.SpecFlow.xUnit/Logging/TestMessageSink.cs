using System;
using Xunit.Abstractions;

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.xUnit.Logging
{
    public class TestMessageSink : IMessageSink
    {
        public bool OnMessage(IMessageSinkMessage message)
        {
            // Do what you want to in response to events here.
            // 
            // Each event has a corresponding implementation of IMessageSinkMessage.
            // See examples here: https://github.com/xunit/abstractions.xunit/tree/master/src/xunit.abstractions/Messages
            if (message is ITestPassed)
            {
                // Beware that this message won't actually appear in the Visual Studio Test Output console.
                // It's just here as an example. You can set a breakpoint to see that the line is hit.
                Console.WriteLine("Execution time was an awesome " + ((ITestPassed)message).ExecutionTime);
            }

            // Return `false` if you want to interrupt test execution.
            return true;
        }
    }
}
