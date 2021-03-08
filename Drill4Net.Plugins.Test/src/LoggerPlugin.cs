using Serilog;
using Drill4Net.Plugins.Abstract;

namespace Drill4Net.Plugins.Test
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    public class LoggerPlugin : AbsractPlugin
    {
        static LoggerPlugin()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File("log.txt")
               .CreateLogger();
        }

        /*********************************************************/

        public static void ProcessStatic(string data)
        {
            Log.Information(data);
        }

        public override void Process(string data)
        {
            ProcessStatic(data);
        }
    }
}
