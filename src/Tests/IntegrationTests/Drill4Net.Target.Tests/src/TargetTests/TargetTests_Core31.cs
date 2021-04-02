namespace Drill4Net.Target.Tests
{
    internal class TargetTests_Core31 : AbstractInjectTargetTests
    {
        protected override void LoadTarget()
        {
            _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_CORE31);
        }
    }
}
