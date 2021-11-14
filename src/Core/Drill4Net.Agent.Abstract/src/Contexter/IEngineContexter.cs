namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Interface for Engine test frameworks' contexters: xUnit, NUnit, MsTest, etc
    /// </summary>
    public interface IEngineContexter
    {
        TestEngine GetTestEngine();
    }
}
