namespace Drill4Net.Target.Tests
{
    internal class Target_Core22 : AbstractInjectTargetTests
    {
        protected override void LoadTarget()
        {
            _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_CORE22);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_CORE22);
        }
    }
}
