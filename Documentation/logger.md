## Logger
You could get all entityworker logs by creating Assigning logger i global configration
Here is how
```csharp
// create a class that inherit from EntityWorker.Core.Interface.ILog 
public class Logger : EntityWorker.Core.Interface.Ilog
{
    public class Logger : ILog
    {
        private string logIdentifier = $"{DateTime.Now.ToString("yyyy-MM-dd")} Ilog.txt";
        private string logPath = AppDomain.CurrentDomain.BaseDirectory;
        public Logger()
        {
            DirectoryInfo dinfo = new DirectoryInfo(logPath);
            var files = dinfo.GetFiles("*.txt");
            foreach (FileInfo file in files)
            {

                var name = file.Name.Split(' ')[0];
                if (name.ConvertValue<DateTime?>().HasValue && file.Name.Contains("Ilog"))
                {

                    if (name.ConvertValue<DateTime>().Date == DateTime.Now.Date)
                    {
                        logIdentifier = file.Name;
                        break;
                    }
                }
            }

            logIdentifier = Path.Combine(logPath, logIdentifier);
            File.Open(logIdentifier, FileMode.OpenOrCreate).Close();

        }

        public void Dispose()
        {
        }

        public void Error(Exception exception)
        {
            using (StreamWriter stream = new StreamWriter(logIdentifier))
                stream.WriteLine($"{DateTime.Now} - {exception.Message}");
        }

        public void Info(string message, object infoData)
        {
#if DEBUG
            using (StreamWriter stream = new StreamWriter(logIdentifier))
                stream.WriteLine($"{DateTime.Now} - {message} - \n {infoData}");
#endif
        }
    }

}
// now that we create the logg class we can now tell Entityworker to begin logging
// in GlobalConfiguration we assign the new created class to ILog. only exist in nuget => 2.0.0
 GlobalConfiguration.Log = new Logger();
 // thats all 
```
