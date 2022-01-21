namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Interface for test frameworks generating tests: SpecFlow (BDD), etc -
    /// for concrete test engines (xUnit, NUnit, etc)
    /// </summary>
    public interface IGeneratorContexter
    {
        string Name { get; }
        TestGenerator GetTestGenerator();
    }
}
