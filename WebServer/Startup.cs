using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Scraper
{
    public class Startup
    {
	ILogger _logger;
	DownloaderUtil.Downloader _downloader;

        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
	    _logger = loggerFactory.CreateLogger("Test");
	    _logger.LogInformation("Starting Jacks logging");
            
	    if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler();

            app.UseStaticFiles();
	    app.UseSignalR2();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
	    
	    _downloader = DownloaderUtil.Downloader.Instance;
	    _downloader.WebDriverProgress += (s,e) => { _logger.LogInformation(e.Message); };
	    _downloader.WebDriverError += (s,e) => { _logger.LogError(e.Message); };
	    _downloader.DownloaderProgress += (s,e) => { _logger.LogInformation("Progress"); };
	    _downloader.DownloaderError += (s,e) => { _logger.LogError(e.Message); };
	    DownloaderUtil.Downloader.Instance.Go("http://kinoman.tv/film/karbala-2");
        }

        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }
}
