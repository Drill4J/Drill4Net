using System;

namespace Injector.Core
{
    public class AssemblyVersion
    {
        public AssemblyVersionType Target { get; set; }
        public string Version { get; set; }
        public bool IsStrongName { get; set; }

        /*************************************************************************/

        public AssemblyVersion()
        {
        }

        public AssemblyVersion(string rawVersion)
        {
            if (string.IsNullOrWhiteSpace(rawVersion))
                return;

            var ar = rawVersion.Split(',');
            if (ar.Length != 2)
                throw new ArgumentException(nameof(rawVersion));

            Target = (ar[0]) switch
            {
                ".NETCoreApp" => AssemblyVersionType.NetCore,
                ".NETStandard" => AssemblyVersionType.NetStandard,
                _ => AssemblyVersionType.NetFramework,
            };

            var versionAr = ar[1].Split('=');
            if (versionAr.Length != 2)
                throw new ArgumentException(nameof(rawVersion));
            Version = $"{versionAr[1].Remove(0, 1)}.0";
        }

        /*************************************************************************/

        public override string ToString()
        {
            return Target == AssemblyVersionType.Unknown ? Target.ToString() : $"{Target} {Version}";
        }
    }
}
