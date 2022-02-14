namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Engine of test generator framework (SpecFlow, xUnit, NUmit, etc).
    /// </summary>
    public class TestGenerator
    {
        public string Name { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Type name of Generator (in most cases it is the Hooks class)
        /// </summary>
        public string TypeName { get; set; }

        /********************************************************************/

        public override string ToString()
        {
            return Name;
        }
    }
}
