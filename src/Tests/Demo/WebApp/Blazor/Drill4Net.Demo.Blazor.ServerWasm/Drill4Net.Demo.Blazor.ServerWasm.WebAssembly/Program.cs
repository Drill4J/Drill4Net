using System;
using System.Net.Http;
using System.Threading.Tasks;
using Drill4Net.Demo.Blazor.WasmApp.Shared;
using Drill4Net.Demo.Blazor.WasmApp.Shared.Data;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Drill4Net.Demo.Blazor.WasmApp.WebAssembly
{
    // Uses Directory.Build.props because need default path for obj folder
    // Otherwise build fails with error 
    // This is a known (opened) issue https://github.com/dotnet/aspnetcore/issues/25959

    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // 增加 BootstrapBlazor 组件
            builder.Services.AddBootstrapBlazor();

            builder.Services.AddSingleton<WeatherForecastService>();

            // 增加 Table 数据服务操作类
            builder.Services.AddTableDemoDataService();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
