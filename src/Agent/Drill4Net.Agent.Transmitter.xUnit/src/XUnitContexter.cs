using Drill4Net.Common;

namespace Drill4Net.Agent.Transmitter.xUnit
{
    // https://github.com/xunit/xunit/issues/621 - they say, no test context in xUnit 2.4.x now. It is sad.
    // but in the discussion above and in the source xUnit 3.x (as silly class) it exists (not in NuGet package - commit on 23 Jule, 2021):
    // https://github.com/xunit/xunit/blob/32a168c759e38d25931ee91925fa75b6900209e1/src/xunit.v3.core/Sdk/Frameworks/TestContextAccessor.cs

    public class XUnitContexter : IContexter
    {
        public string GetContextId()
        {
            return null;
        }
    }
}
