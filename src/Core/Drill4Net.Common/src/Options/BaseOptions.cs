namespace Drill4Net.Common
{
    public class BaseOptions
    {
        public TargetOptions Target { get; set; }

        /// <summary>
        /// Path to the Tree data file, if empty, system will try find it 
        /// by another ways using "redirect cfg", current dir, etc
        /// </summary>
        public string TreePath { get; set; }
        public string Description { get; set; }
    }
}
