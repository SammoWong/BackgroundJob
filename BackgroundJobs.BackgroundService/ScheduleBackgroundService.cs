using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundJobs.BackgroundService
{
    public class ScheduleBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly ILogger _logger;
        public ScheduleBackgroundService(ILogger<ScheduleBackgroundService> logger)
        {
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Logic
                    await Task.Delay(1000 * 1);
                    Console.WriteLine("running at: " + DateTime.Now.ToString());
                }
                catch (Exception ex)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogError("ScheduleBackgroundService异常:" + ex.Message + ex.StackTrace);
                    }
                    else
                    {
                        _logger.LogError("ScheduleBackgroundService停止:" + ex.Message + ex.StackTrace);
                    }
                }
                await Task.Delay(1000 * 60, stoppingToken);//默认等待1分钟
            }
        }
    }
}
