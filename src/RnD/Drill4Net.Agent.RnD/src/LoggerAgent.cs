using Serilog;

namespace Drill4Net.Agent.RnD
{
    //add this in project's csproj file: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    public class LoggerAgent
    {
        static LoggerAgent()
        {
            #pragma warning disable DF0037 // Marks undisposed objects assinged to a property, originated from a method invocation.
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.File("agent_log.txt")
               .CreateLogger();
            #pragma warning restore DF0037 // Marks undisposed objects assinged to a property, originated from a method invocation.
        }

        ~LoggerAgent()
        {
            Log.CloseAndFlush();
        }

        /*********************************************************/

        public static void RegisterStatic(string data)
        {
            //we just write to the file
            #pragma warning disable Serilog004 // Constant MessageTemplate verifier
            Log.Information(data);
            #pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }
    }
}
