using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace KHttp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileProvider>(_ => new PhysicalFileProvider(Environment.CurrentDirectory));
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IFileProvider fileProvider)
        {
            ConfigureLogging(loggerFactory);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = fileProvider
            });
        }

        private void ConfigureLogging(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Verbose);
        }
    }
}