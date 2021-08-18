using System;
using System.IO;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Serialization.Checker
{
    class Program
    {
        private const string FILE_ORIG = @"data\injected.tree";
        private const string FILE_REWRITTEN = @"data\injected_rewritten.tree";

        /*********************************************************************************/

        static void Main(string[] args)
        {
            // get byte arrays of the tree's files and check
            byte[] rewrBytes = null;
            try
            {
                var origBytes = File.ReadAllBytes(FILE_ORIG);
                rewrBytes = File.ReadAllBytes(FILE_REWRITTEN);

                bool isEqual = origBytes.SequenceEqual(rewrBytes);
                Console.WriteLine($"Difference in bytes exists: {!isEqual}");

                if (!isEqual)
                {
                    for (var i = 0; i < origBytes.Length; i++)
                    {
                        if (origBytes[i] != rewrBytes[i])
                            Console.WriteLine($"Difference in element {i}: orig {origBytes[i]} -> rewritten {rewrBytes[i]}");
                    }
                }
            }
            catch (IOException iex)
            {
                Console.WriteLine($"Can't read the Tree's file:\n{iex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:\n{ex}");
            }

            // try to deserialize tree's file from another site (for example, OS version)
            if (rewrBytes != null)
            {
                try
                {
                    var tree = Serializer.FromArray<InjectedSolution>(rewrBytes);

                    // view Tree's info
                    Console.WriteLine($"Tree Name: {tree?.Name}");
                    Console.WriteLine($"Tree Description: {tree?.Description}");
                    Console.WriteLine($"Tree StartTime: {tree?.StartTime}");
                    Console.WriteLine($"Tree FinishTime: {tree?.FinishTime}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Tree data is not deserialized: [{FILE_REWRITTEN}].\n{ex}");
                }
            }
        }
    }
}
