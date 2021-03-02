namespace Target.Common
{
    public class GenStr : AbstractGen<string>
    {
        public GenStr(string prop) : base(prop)
        {
        }

        public string GetShortDesc()
        {
            return GetDesc(false);
        }
    }
}
