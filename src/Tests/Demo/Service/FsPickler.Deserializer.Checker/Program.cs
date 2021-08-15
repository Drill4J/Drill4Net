using System;
using System.IO;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace FsPickler.Deserializer.Checker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                InjectedSolution tree = null;
                var bytes2 = File.ReadAllBytes(@"C:\Docker\injected_rewritten.tree");
                var bytes1 = File.ReadAllBytes(@"C:\Docker\injected.tree");
                bool isEqual = Enumerable.SequenceEqual(bytes1, bytes2);
                Console.WriteLine($"Difference in bytes:");
                for (int i=0;i<bytes1.Length; i++)
                {
                    if (bytes1[i]!=bytes2[i])
                        Console.WriteLine($"Difference in element: {i}   injected_rewritten.tree {bytes2[i]}  injected.tree {bytes1[i]}");
                }
                try
                {
                    tree = Serializer.FromArray<InjectedSolution>(bytes2);
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Tree data not serialized: [C:\\Docker\\injected_rewritten.tree].\n{ex}");

                }
                Console.WriteLine($"Tree Description {tree?.Description}");
                Console.WriteLine($"Tree Count {tree?.Count.ToString()}");
                Console.WriteLine($"Tree Name {tree?.Name}");
                Console.WriteLine($"Tree StartTime {tree?.StartTime.ToString()}");
                Console.WriteLine($"Tree StartTime {tree?.StartTime.ToString()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can't deserialize Tree data:  { ex}");
            }
        }
    }
}
