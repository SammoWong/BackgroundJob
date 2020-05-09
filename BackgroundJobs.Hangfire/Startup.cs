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
            // Add Hangfire services.�����ڴ棬Ҳ���Ի���Redis��SQLSERVER,MySQL
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
                Queues = new[] { "back", "front", "default" },//�������ƣ�ֻ��ΪСд
                WorkerCount = Environment.ProcessorCount * 5, //����������
                ServerName = "conference hangfire1",//����������
            };
            app.UseHangfireServer(jobOptions);
            app.UseHangfireDashboard();

            //��������
            //���ڶ��е���������Hangfire����õģ���������ӵ�������ִ�У�������һ��.
            var jobId = BackgroundJob.Enqueue(() => Console.WriteLine($@"��������"));

            //��ʱ����
            //�ӳ٣��ƻ�������������������ƣ������������ö����趨һ��δ��ʱ�������ִ�С���С����ʱ����Ϊ�뼶���
            BackgroundJob.Schedule(() => Console.WriteLine("Schedule!"), TimeSpan.FromMinutes(1));

            //����������ִ��
            //����������������.NET�е�Task,�����ڵ�һ������ִ����֮��������ٴ�ִ�����������
            BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine($@"finished"));

            //��ʱ����
            //��ʱѭ������ִ�У�һ�д�������ظ�ִ�е������������˳�����ʱ��ѭ��ģʽ��Ҳ�ɻ���CRON���ʽ���趨���ӵ�ģʽ 
            RecurringJob.AddOrUpdate("1", () => Console.WriteLine("Recurring!"), Cron.Minutely());//ÿ����ִ��һ��
            RecurringJob.AddOrUpdate("2", () => Console.WriteLine("RecurringJob!"), Cron.Minutely(), queue: "back");//ÿ����ִ��һ��

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
