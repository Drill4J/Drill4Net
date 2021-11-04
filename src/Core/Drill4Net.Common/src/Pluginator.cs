using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Drill4Net.Common
{
    public class Pluginator
    {

        /*********************************************************************/

        public Pluginator()
        {

        }

        /*********************************************************************/

        public List<object> GetByInterface(string dir, Type intrfc) //TODO: + using Filter
        {
            if (string.IsNullOrWhiteSpace(dir))
                throw new ArgumentNullException(nameof(dir));
            if (!Directory.Exists(dir))
                throw new Exception($"Directory does not exists: [{dir}]");
            //
            ConcurrentDictionary<string, string> asms = new();
            return SearchByInterface(dir, intrfc, asms);
        }

        internal List<object> SearchByInterface(string dir, Type intrfc, ConcurrentDictionary<string, string> asms)
        {
            var list = new List<object>();

            //files
            var files = Directory.GetFiles(dir)
                .Where(a => Path.GetExtension(a) == ".dll");
            Parallel.ForEach(files, (file) =>
            {
                var dirTypes = GetTypes(file, intrfc, asms);
                list.AddRange(dirTypes);
            });

            //subdirectories
            var dirs = Directory.GetDirectories(dir)
                .AsParallel();
            foreach (var curDir in dirs)
            {
                var dirTypes = SearchByInterface(curDir, intrfc, asms);
                list.AddRange(dirTypes);
            }

            return list;
        }

        internal IEnumerable<Type> GetTypes(string asmPath, Type intrfc, ConcurrentDictionary<string, string> asms)
        {
            if(!asms.TryAdd(Path.GetFileName(asmPath), null))
                return new List<Type>();
            //
            try
            {
                var assembly = Assembly.LoadFrom(asmPath);
                var types = assembly.GetTypes();
                var list = new List<Type>();
                Parallel.ForEach(types, (type) =>
                {
                    var intr = Array.Find(type.GetInterfaces(), a => a.Name == intrfc.Name);
                    if (intr != null)
                        list.Add(type);
                });
                return list;
            }
            catch
            {
                return new List<Type>();
            }
        }
    }
}
