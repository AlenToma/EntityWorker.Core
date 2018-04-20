using System;

namespace EntityWorker.Core.Interface
{
    public interface ILog : IDisposable
    {
        /// <summary>
        /// Here we log errors
        /// </summary>
        /// <param name="exception"></param>
        void Error(Exception exception);

        /// <summary>
        /// Here we log data like executed sql information
        /// </summary>
        /// <param name="message"></param>
        /// <param name="infoData"></param>
        void Info(string message, object infoData);
    }
}
