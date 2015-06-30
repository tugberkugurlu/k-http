using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Logging;
using System.Threading;
using Microsoft.AspNet.Hosting.Internal;

namespace KHttp
{
    public class Program
    {
        private const string WebServerAssemblyName = "Microsoft.AspNet.Server.WebListener";
        private readonly IApplicationEnvironment _appEnv;
        private readonly IServiceProvider _hostServiceProvider;

        public Program(IApplicationEnvironment appEnv, IServiceProvider hostServiceProvider)
        {
            _appEnv = appEnv;
            _hostServiceProvider = hostServiceProvider;
        }

        public void Main(string[] args)
        {
            var configurationBuilder = new InternalConfigurationBuilder(args);
            var config = configurationBuilder.Build();
            var webHostBuilder = new WebHostBuilder(_hostServiceProvider, config)
                .UseServer(WebServerAssemblyName)
                .UseStartup(typeof(Startup));

            IHostingEngine engine = webHostBuilder.Build();
            var serverShutdown = engine.Start();
            var loggerFactory = engine.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var appShutdownService = engine.ApplicationServices.GetRequiredService<IApplicationShutdown>();
            var shutdownHandle = new ManualResetEvent(false);

            appShutdownService.ShutdownRequested.Register(() =>
            {
                try
                {
                    serverShutdown.Dispose();
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
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