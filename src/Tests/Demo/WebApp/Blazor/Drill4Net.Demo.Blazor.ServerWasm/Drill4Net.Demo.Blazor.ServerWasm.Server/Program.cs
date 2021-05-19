using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Drill4Net.Demo.Blazor.WasmApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //for debugging (you need to have time to hook up with the debugger to the process)
            System.Threading.Thread.Sleep(5000); 

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
