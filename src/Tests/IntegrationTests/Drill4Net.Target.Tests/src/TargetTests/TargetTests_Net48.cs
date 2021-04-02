namespace Drill4Net.Target.Tests
{
    internal class TargetTests_Net48 : AbstractInjectTargetTests
    {
        protected override void LoadTarget()
        {
            _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_NET48);
        }
    }
}
