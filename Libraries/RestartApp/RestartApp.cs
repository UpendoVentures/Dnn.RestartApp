/*
Copyright Upendo Ventures, LLC 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

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