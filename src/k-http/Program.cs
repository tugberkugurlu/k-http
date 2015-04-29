using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Logging;
using System.Threading;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Runtime.Common.CommandLine;
using System.Reflection;

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
            var app = new StaticServerConsoleApplication(args);

            var configurationBuilder = new ConfigurationBuilder(args);
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

    public class StaticServerConsoleApplication
    {
        private readonly string[] _args;
        private readonly Configuration _configuration;
        private readonly StaticServerAppOptions _options;
        private readonly CommandLineApplication _app;

        public StaticServerConsoleApplication(string[] args)
        {
            if(args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var options = new StaticServerAppOptions();
            var app = new CommandLineApplication();
            app.Name = "k-http";
            app.FullName = app.Name;
            app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", GetVersion);
            var pathArgument = app.Argument("[path]", "Path to directory to be served through HTTP, default is current directory", c => 
            {
                c.Value = Environment.CurrentDirectory;
            });
            var portOption = app.Option("-p|--port <PORT>", "TCP port to use, default is 5000", CommandOptionType.SingleValue);

            app.Execute(args);

            ushort port = 5000;
            if (portOption.HasValue())
            {
                if(!ushort.TryParse(portOption.Value(), out port))
                {
                    throw new ArgumentException("TODO: Port number is not valid");
                }
            }

            options.Path = pathArgument.Value;
            options.Port = port;

            _app = app;
            _options = options;
        }

        public Configuration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        public StaticServerAppOptions Options
        {
            get
            {
                return _options;
            }
        }

        public void OutputHelp()
        {
            _app.ShowHelp();
        }

        private static string GetVersion()
        {
            var assembly = typeof(StaticServerConsoleApplication).GetTypeInfo().Assembly;
            var assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return assemblyInformationalVersionAttribute.InformationalVersion;
        }
    }

    public class StaticServerAppOptions
    {
        public string Path { get; set; }
        public int Port { get; set; }
    }

    // Things happening here
    // 1-) Commandline help/version/info thing
    // 3-) Cmdline argument validation
    // 4-) 
}