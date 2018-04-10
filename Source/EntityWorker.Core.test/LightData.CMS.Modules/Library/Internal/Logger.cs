using EntityWorker.Core.Interface;
using System;

namespace LightData.CMS.Modules.Library.Internal
{
    public class Logger : ILog
    {
        public event EventHandler onLog;

        public Logger(EventHandler eventlog)
        {
            onLog = eventlog;
        }

        public void Error(Exception exception)
        {
            onLog?.Invoke(this, new Args(exception));
        }

        public void Info(string message, object infoData)
        {
            onLog?.Invoke(this, new Args(message, infoData));
        }
    }
}
