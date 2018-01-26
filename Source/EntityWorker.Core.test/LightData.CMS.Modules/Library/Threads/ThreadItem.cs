using System.Threading;

namespace LightData.CMS.Modules.Library.Threads
{
    public class ThreadItem
    {
        private Thread _thread;

        public ThreadItem(ParameterizedThreadStart parameterizedThreadStart)
        {
            _thread = new Thread(parameterizedThreadStart);
            _thread.IsBackground = true;
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            _thread.Abort();
        }

        public bool IsRunning()
        {
            return _thread.IsAlive;
        }
    }
}
