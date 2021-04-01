namespace Drill4Net.Target.Common
{
    public abstract class AbstractGen<T>
    {
        public T Prop { get; set; }

        protected AbstractGen(T prop)
        {
            Prop = prop;
        }

        public string GetDesc(bool isFull)
        {
            var name = typeof(T).Name;
            return isFull ? $"Type of {nameof(Prop)} is {name}" : name;
        }
    }
}
