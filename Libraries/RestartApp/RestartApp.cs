
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Log.EventLog;

namespace Upendo.Libraries.RestartApp.ScheduledJobs
{
    /// <summary>
    /// A scheduled job that can be managed in DNN. This job simply recycles the application, forcing it to restart again without anything cached.
    /// </summary>
    public class RestartApp : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RestartApp));

        /// <summary>
        /// Gets things started...
        /// </summary>
        /// <param name="oItem"></param>
        public RestartApp(ScheduleHistoryItem oItem) : base()
        {
            ScheduleHistoryItem = oItem;
        }

        /// <summary>
        /// This method does all of the real work.
        /// </summary>
        public override void DoWork()
        {
            try
            {
                // Perform required items for logging
                Progressing();

                ScheduleHistoryItem.AddLogNote("RestartApp Starting");
                Logger.Debug("RestartApp Scheduled Job Starting");

				RestartApplication();

                ScheduleHistoryItem.AddLogNote("RestartApp Completed");
                Logger.Debug("RestartApp Scheduled Job Completed");

                // Show success
                ScheduleHistoryItem.Succeeded = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("Exception:: " + ex.ToString());
                Exceptions.LogException(ex);
            }
        }

        private void RestartApplication()
        {
            var log = new LogInfo { BypassBuffering = true, LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
            log.AddProperty("Message", "Application Restarted by a Scheduled Job");
            LogController.Instance.AddLog(log);
            Config.Touch();
        }

        private void LogError(Exception ex)
        {
            Logger.Error(ex);
            if (ex.InnerException != null)
            {
                LogError(ex.InnerException);
            }
        }
    }
}