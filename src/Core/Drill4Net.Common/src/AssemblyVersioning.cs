using System;

namespace Drill4Net.Common
{
    /// <summary>
    /// Structure for the assembly version's metadata
    /// </summary>
    [Serializable]
    public class AssemblyVersioning
    {
        /// <summary>
        /// Type of the assembly's version
        /// </summary>
        public AssemblyVersionType Target { get; set; }

        /// <summary>
        /// The string representation of the assembly's version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Raw string version form the assembly's metadata (appropriate attribute)
        /// </summary>
        public string RawVersion { get; set; }

        /// <summary>
        /// Does the assembly contains a strong name?
        /// </summary>
        public bool IsStrongName { get; set; }

        /*************************************************************************/

        public AssemblyVersioning()
        {
        }

        public AssemblyVersioning(string rawVersion)
        {
            if (string.IsNullOrWhiteSpace(rawVersion))
                return;

            RawVersion = rawVersion;
            var ar = rawVersion.Split(',');
            if (ar.Length != 2)
                throw new ArgumentException(nameof(rawVersion));

            Target = ar[0] switch
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
