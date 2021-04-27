namespace Drill4Net.Agent.Abstract
{
    public class BusinessNamespacer
    {
        public string[] PackagesPrefixes { get; set; }

        public BusinessNamespacer()
        {
            PackagesPrefixes = new string[0];
        }
    }
}
