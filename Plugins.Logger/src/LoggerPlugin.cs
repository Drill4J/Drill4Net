using Serilog;

namespace Plugins.Logger
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    public static class LoggerPlugin
    {
        static LoggerPlugin()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File("log.txt")
               .CreateLogger();
        }

        /*********************************************************/

        public static void Process(string data)
        {
            Log.Information(data);
        }
    }
}
