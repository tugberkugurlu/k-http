using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Logging;
using System.Threading;
using Microsoft.AspNet.Hosting.Internal;

namespace KHttp
{
    public class Program
    {
        private const string WebListenerAssemblyName = "Microsoft.AspNet.Server.WebListener";
        private const string KestrelAssemblyName = "Microsoft.AspNet.Server.Kestrel";
        private readonly IApplicationEnvironment _appEnv;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly IServiceProvider _hostServiceProvider;

        public Program(IApplicationEnvironment appEnv, IRuntimeEnvironment runtimeEnvironment, IServiceProvider hostServiceProvider)
        {
            _appEnv = appEnv;
            _runtimeEnvironment = runtimeEnvironment;
            _hostServiceProvider = hostServiceProvider;
        }

        public void Main(string[] args)
        {
            var configurationBuilder = new InternalConfigurationBuilder(args);
            var config = configurationBuilder.Build();
            var isWindows = _runtimeEnvironment.OperatingSystem == "Windows";
            var serverNameToUse = isWindows ? WebListenerAssemblyName : KestrelAssemblyName;
            var webHostBuilder = new WebHostBuilder(_hostServiceProvider, config)
                .UseServer(serverNameToUse)
                .UseStartup(typeof(Startup));

            IHostingEngine engine = webHostBuilder.Build();
            var serverShutdown = engine.Start();
            var loggerFactory = engine.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();

            if(!isWindows)
            {
                logger.LogInformation("k-http has started, hit ENTER to stop...");
            }

            var appShutdownService = engine.ApplicationServices.GetRequiredService<IApplicationShutdown>();
            var shutdownHandle = new ManualResetEvent(false);

            appShutdownService.ShutdownRequested.Register(() =>
            {
                logger.LogInformation("k-http is shutting down...");

                try
                {
                    serverShutdown.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogError("Dispose threw an exception.", ex);
                }
                shutdownHandle.Set();
            });

            var ignored = Task.Run(() =>
            {
                Console.ReadLine();
                appShutdownService.RequestShutdown();
            });

            shutdownHandle.WaitOne();
        }
    }
}
