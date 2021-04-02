namespace Drill4Net.Target.Tests
{
    internal class Target_Net48 : AbstractInjectTargetTests
    {
        protected override void LoadTarget()
        {
            _testsRep.LoadTargetIntoMemory(TestConstants.MONIKER_NET48);
        }

        protected override void UnloadTarget()
        {
            _testsRep.UnloadTarget(TestConstants.MONIKER_NET48);
        }
    }
}
