namespace Drill4Net.BanderLog.Sinks
{
    public class LogSerializedInfo<TState>
    {
        public TState State { get; set; }
        public string Exception { get; set; }
    }
}
