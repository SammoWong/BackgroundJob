using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangfire;
using Hangfire.MemoryStorage;

namespace BackgroundJobs.Hangfire
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
            services.AddControllersWithViews();
            // Add Hangfire services.基于内存，也可以基于Redis，SQLSERVER,MySQL
            services.AddHangfire(x => x.UseStorage(new MemoryStorage()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            var jobOptions = new BackgroundJobServerOptions
            {
                Queues = new[] { "back", "front", "default" },//队列名称，只能为小写
                WorkerCount = Environment.ProcessorCount * 5, //并发任务数
                ServerName = "conference hangfire1",//服务器名称
            };
            app.UseHangfireServer(jobOptions);
            app.UseHangfireDashboard();

            //队列任务
            //基于队列的任务处理是Hangfire中最常用的，会立即添加到队列中执行，仅处理一次.
            var jobId = BackgroundJob.Enqueue(() => Console.WriteLine($@"队列任务"));

            //延时任务
            //延迟（计划）任务跟队列任务相似，不是立即调用而是设定一个未来时间点再来执行。最小的延时类型为秒级别的
            BackgroundJob.Schedule(() => Console.WriteLine("Schedule!"), TimeSpan.FromMinutes(1));

            //延续性任务执行
            //延续性任务类似于.NET中的Task,可以在第一个任务执行完之后紧接着再次执行另外的任务
            BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine($@"finished"));

            //定时任务
            //定时循环任务执行：一行代码添加重复执行的任务，其内置了常见的时间循环模式，也可基于CRON表达式来设定复杂的模式 
            RecurringJob.AddOrUpdate("1", () => Console.WriteLine("Recurring!"), Cron.Minutely());//每分钟执行一次
            RecurringJob.AddOrUpdate("2", () => Console.WriteLine("RecurringJob!"), Cron.Minutely(), queue: "back");//每分钟执行一次

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
