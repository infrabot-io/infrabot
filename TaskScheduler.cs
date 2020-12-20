using System;
using System.Collections.Generic;
using System.Threading;

namespace InfraBot.Core
{
    public class SchedulerService
    {
        private static SchedulerService _instance;
        public static List<Timer> timers = new List<Timer>();
        private SchedulerService() { }
        public static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());
        public static void ScheduleTask(int hour, int min, double intervalInSeconds, Action task, bool runonce = true)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, 0);
            if (now > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }
            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }
            Timer aa = null;
            var timer = aa;
            if (runonce)
            {
                timer = new Timer(x => { task.Invoke(); }, null, timeToGo, Timeout.InfiniteTimeSpan);
            }
            else
            {
                timer = new Timer(x => { task.Invoke(); }, null, timeToGo, TimeSpan.FromSeconds(intervalInSeconds));
            }
            timers.Add(timer);
        }
    }
}