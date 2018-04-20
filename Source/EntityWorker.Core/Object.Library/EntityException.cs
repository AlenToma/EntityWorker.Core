using System;
using System.Collections.Generic;
using System.Text;

namespace EntityWorker.Core.Object.Library
{
    /// <summary>
    /// CustomException
    /// </summary>
    public class EntityException : Exception
    {
        public EntityException(string message) : base(message)
        {
            GlobalConfiguration.Log?.Error(this);
        }
    }
}
