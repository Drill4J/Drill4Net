using System;

namespace TestNF
{
    public abstract class AbstractGen<T>
    {
        public T Prop { get; set; }

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
