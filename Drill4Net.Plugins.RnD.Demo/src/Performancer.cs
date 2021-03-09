using Drill4Net.Plugins.RnD;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Drill4Net.Plugins.Demo
{
    public delegate void ProcDlgType(long cnt);

    public class Performancer
    {
        private readonly MethodInfo _methInfo;
        private readonly ProcDlgType _dlg;

        /**********************************************************/

        public Performancer()
        {
            var profPath = @"d:\Projects\EPM-D4J\!!_exp\Drill4Net\Drill4Net.Plugins.RnD\bin\Debug\netstandard2.0\Drill4Net.Plugins.RnD.dll";
            var asm = Assembly.LoadFrom(profPath);
            var type = asm.GetType("Drill4Net.Plugins.RnD.TestPlugin");
            _methInfo = type.GetMethod("Process");
            //
            _dlg = (ProcDlgType)Delegate.CreateDelegate(typeof(ProcDlgType), null, _methInfo); //for static method
        }

        /**********************************************************/

        //TODO: to do normal!!!

        public void Calc(long cnt)
        {
            var sw = new Stopwatch();

            sw.Start();
            CalcDirect(cnt);
            sw.Stop();
            Console.WriteLine($"CalcDirect: {sw.Elapsed}");

            sw.Reset();
            sw.Start();
            CalcMethodInfo(cnt);
            sw.Stop();
            Console.WriteLine($"CalcMethodInfo: {sw.Elapsed}");

            sw.Reset();
            sw.Start();
            CalcDelegate(cnt);
            sw.Stop();
            Console.WriteLine($"CalcDelegate: {sw.Elapsed}");

            sw.Reset();
            sw.Start();
            CalcDynamic(cnt);
            sw.Stop();
            Console.WriteLine($"CalcDynamic: {sw.Elapsed}");
        }

        public void CalcDirect(long cnt)
        {
            PerfPlugin.Do(cnt);
        }

        public void CalcMethodInfo(long cnt)
        {
            _methInfo.Invoke(null, new object[] { cnt });
        }

        public void CalcDelegate(long cnt)
        {
            _dlg(cnt);
        }

        public void CalcDynamic(long cnt)
        {
            dynamic test = new PerfPlugin();
            test.Process(cnt);
        }
    }
}
