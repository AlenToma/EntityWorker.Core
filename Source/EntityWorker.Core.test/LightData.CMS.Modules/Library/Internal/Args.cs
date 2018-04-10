using System;
using System.Collections.Generic;
using System.Text;

namespace LightData.CMS.Modules.Library.Internal
{
    public class Args : EventArgs
    {
        public object Data { get; private set; }
        public Args(object data)
        {
            Data = data;
        }

        public Args(string message, object data)
        {
            Data = $"{DateTime.Now} - {message} - {data}";
        }
    }
}
