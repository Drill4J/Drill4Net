using Serilog;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.RnD
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    public class LoggerAgent : AbsractAgent
    {
        static LoggerAgent()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File("log.txt")
               .CreateLogger();
        }

        /*********************************************************/

        public static void RegisterStatic(string data)
        {
            Log.Information(data);
        }

        public override void Register(string data)
        {
            RegisterStatic(data);
        }
    }
}
