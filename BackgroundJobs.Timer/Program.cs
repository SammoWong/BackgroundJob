using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;

namespace BackgroundJobs.Timer
{
    class Program
    {
        private static object locker = new object();
        static void Main(string[] args)
        {
            System.Timers.Timer timer = new System.Timers.Timer()
            {
                Enabled = true,
                Interval = 1000 * 5,//执行间隔时间,单位为毫秒;
            };
            timer.Elapsed += new ElapsedEventHandler(Run);
            timer.Start();

            Console.ReadKey();
        }

        private static void Run(object source, ElapsedEventArgs e)
        {
            lock (locker)
            {
                Console.WriteLine("running at: " + DateTime.Now.ToString());
            }
        }
    }
}
