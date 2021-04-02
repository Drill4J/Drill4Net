namespace Drill4Net.Target.Tests
{
    internal class TargetTests_Net45 : AbstractInjectTargetTests
    {
        protected override void LoadTarget()
        {
            _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_NET45);
        }
    }
}
