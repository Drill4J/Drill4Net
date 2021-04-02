namespace Drill4Net.Target.Tests
{
    internal class TargetTests_Core22 : AbstractInjectTargetTests
    {
        protected override void LoadTarget()
        {
            _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_CORE22);
        }
    }
}
