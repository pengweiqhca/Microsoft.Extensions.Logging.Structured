using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Structured;
using Microsoft.Extensions.Logging.Structured.Kafka;
using Microsoft.Extensions.Options;

namespace testapi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(lb => lb.AddStructuredKafkaLog());
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }

    public static class WebLogExtensions
    {
        public static IStructuredLoggingBuilder<KafkaLoggingOptions> AddStructuredKafkaLog(this ILoggingBuilder services)
        {
            IStructuredLoggingBuilder<KafkaLoggingOptions> loggingBuilder = services.AddKafka();
            loggingBuilder
                .AddLayout("datetime", new DateTimeLayout())
                .AddLayout("level", new LogLevelLayout())
                .AddLayout("message",new RenderedMessageLayout())
                .AddLayout("exception", new ExceptionLayout());
            return loggingBuilder;
        }
    }
}
