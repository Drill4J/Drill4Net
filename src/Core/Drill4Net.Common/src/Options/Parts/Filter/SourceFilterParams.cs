using System.Collections.Generic;

namespace Drill4Net.Common
{
    public class SourceFilterParams
    {
        public List<string> Directories { get; set; }
        public List<string> Files { get; set; }
        public List<string> Namespaces { get; set; }
        public List<string> Classes { get; set; }
        public List<string> Attributes { get; set; }
    }
}
