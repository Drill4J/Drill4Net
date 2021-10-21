using System;
using System.IO;
using System.Linq;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drill4Net.Target.Common.VB;
//[assembly: InternalsVisibleTo("Drill4Net.Target.Tests.Net50")]

//add this in project's csproj file: 
//<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

/*                                             *
 *   DON'T OPTIMIZE CODE BY REFACTORING !!!!   *
 *   It's needed AS it IS !!!                  *
 *                                             */

namespace Drill4Net.Target.Common
{

    internal class User
    {
        internal string Name { get; set; }
        internal bool IsAdmin { get; set; }
        internal string Language { get; set; }

        public User(string name, bool isAdmin, string language)
        {
            Name = name;
            IsAdmin = isAdmin;
            Language = language;
        }
    }
}
