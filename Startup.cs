using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using ReportScheduler.BackgroundJobs;
using ReportScheduler.Data;
using Microsoft.EntityFrameworkCore;
using ReportScheduler.EmailService;

namespace ReportScheduler
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
            services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer().UseDefaultTypeSerializer().UseMemoryStorage()
            );
            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.Configure<ConnectionString>(Configuration.GetSection("ConnectionStrings"));
            services.AddScoped<IEmailService, EmailService.EmailService>();
            services.AddScoped<ScheduledReports>();
            services.AddDbContext<Hs_LiveContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("database")));
            services.AddRazorPages();
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
            app.UseHangfireDashboard();
            //backgroundJobClient.Enqueue<ScheduledReports>(x => x.SendScheduledReportsAsync());
            recurringJobManager.AddOrUpdate<ScheduledReports>("scheduledReports", x => x.SendScheduledReportsAsync(), Cron.Hourly);
        }
    }
}
