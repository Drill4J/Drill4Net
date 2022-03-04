using System.Reflection;

// automatic version tagger including Git info - https://github.com/devlooped/GitInfo
// semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
// the most common format is v0.0 (or just 0.0 is enough)
// to change semVer it is nesseccary to create appropriate tag and push it to remote repository
// patches'(commits) count starts with 0 again after new tag pushing
// For file version format exactly is digit
[assembly: AssemblyFileVersion($"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}")]
[assembly: AssemblyInformationalVersion($"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}-{ThisAssembly.Git.Branch}+{ThisAssembly.Git.Commit}")]

namespace Drill4Net.Target.Frameworks.Common
{
    public class Calculator
    {
        public int FirstNumber { get; set; }
        public int SecondNumber { get; set; }

        /*************************************************************/

        public int Add()
        {
            return FirstNumber + SecondNumber;
        }

        public int Substract()
        {
            return FirstNumber - SecondNumber;
        }

        //public int Multiply()
        //{
        //    return FirstNumber * SecondNumber;
        //}
    }
}
