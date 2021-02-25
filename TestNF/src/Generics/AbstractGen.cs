using System;

namespace TestNF
{
    public abstract class AbstractGen<T> where T : class
    {
        public T Prop { get; }

        protected AbstractGen(T prop)
        {
            Prop = prop ?? throw new ArgumentNullException(nameof(prop));
        }

        public string GetDesc(bool isFull)
        {
            var name = Prop.GetType().Name;
            return isFull ? $"Type of {nameof(Prop)} is {name}" : name;
        }
    }
}
