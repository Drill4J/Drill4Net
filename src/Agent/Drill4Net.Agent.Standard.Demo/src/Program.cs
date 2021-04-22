using System;

namespace Drill4Net.Agent.Standard.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var pointUid = "7848799f-77ee-444d-9a9d-6fd6d90f5d82"; //must be real from Injected Tree
                var asmName = $"Drill4Net.Target.Common.dll";
                const string funcSig = "System.Void Drill4Net.Agent.Standard.StandardAgent::Register(System.String)";
                StandardAgent.RegisterStatic($"{pointUid}^{asmName}^{funcSig}^If_6");
                //
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey(true);
        }
    }
}