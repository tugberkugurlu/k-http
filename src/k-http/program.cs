using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using System.Threading;

public class Program
{
    private readonly IApplicationEnvironment _appEnv;
    private readonly IServiceProvider _hostServiceProvider;
    
    public Program(IApplicationEnvironment appEnv, IServiceProvider hostServiceProvider)
    {
        _appEnv = appEnv;
        _hostServiceProvider = hostServiceProvider;
    }
    
    public void Main(string[] args)
    {
        var config = new Configuration();
        config.AddIniFile(Path.Combine(_appEnv.ApplicationBasePath, "config.ini"));
        // config.AddCommandLine(args);
        
        IHostingEngine engine = WebHost.CreateEngine(_hostServiceProvider, config);
        
        engine.UseServer("Microsoft.AspNet.Server.WebListener")
              .UseStartup(app => 
                          {
                              app.Run(async ctx => 
                                      {
                                          await ctx.Response.WriteAsync("Foo");
                                      });
                          });
        
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
            Console.WriteLine("Started");
            Console.ReadLine();
            appShutdownService.RequestShutdown();
        });

        shutdownHandle.WaitOne();
        
        // foreach(var arg in args)
        // {
        //     Console.WriteLine(arg);
        // }
        
        // Console.WriteLine("Hello World");
        // Console.WriteLine(_appEnv.ApplicationBasePath);
        // Console.WriteLine(Environment.CurrentDirectory);
    }
}