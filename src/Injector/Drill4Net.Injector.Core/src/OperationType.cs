namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Type for method's workflow
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Need linear workflow, that is, next operand
        /// </summary>
        NextOperand,

        /// <summary>
        /// Need next cycle ('continue' operand)
        /// </summary>
        CycleContinue,

        /// <summary>
        /// Need cycle return
        /// </summary>
        BreakCycle,

        /// <summary>
        /// Need return from the processing method
        /// </summary>
        Return,
    }
}