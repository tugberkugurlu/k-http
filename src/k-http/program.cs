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
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.StaticFiles;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

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
        var defaultConfigData = new List<KeyValuePair<string, string>> 
        {
            new KeyValuePair<string, string>("server.urls", "http://localhost:5000") 
        };
        
        config.AddInMemoryConfig(defaultConfigData);
        config.AddCommandLine(args);
        
        IHostingEngine engine = WebHost.CreateEngine(_hostServiceProvider, config);        
        engine.UseServer("Microsoft.AspNet.Server.WebListener")
              .UseStartup(app => 
                          {
                              app.UseStaticFiles(new StaticFileOptions 
                                                 {
                                                     FileProvider = new PhysicalFileProvider(Environment.CurrentDirectory)
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
    }
}

internal static class ConfigurationExtensions
{
    public static IConfigurationSourceRoot AddInMemoryConfig(this IConfigurationSourceRoot configuration, IEnumerable<KeyValuePair<string, string>> initialData)
    {
        if(initialData == null)
        {
            throw new ArgumentNullException(nameof(initialData));
        }

        configuration.Add(new MemoryConfigurationSource(initialData));
        return configuration;
    }
}