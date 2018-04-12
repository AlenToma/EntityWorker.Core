## Logger
You could get all entityworker logs by creating Assigning logger i global configration
Here is how
```csharp
// create a class that inherit from EntityWorker.Core.Interface.ILog 
public class Logger : EntityWorker.Core.Interface.Ilog
{
        /// <summary>
        /// Here we log errors
        /// </summary>
        /// <param name="exception"></param>
        public void Error(Exception exception)
        {
          /// here do what you want with it 
        }
        
        /// <summary>
        /// Here we log data like executed sql information
        /// </summary>
        /// <param name="message"></param>
        /// <param name="infoData"></param>
        public void Info(string message, object infoData)
        {
          /// here do what you want with it 
        }

}
// now that we create the logg class we can now tell Entityworker to begin logging
// in GlobalConfiguration we assign the new created class to ILog. only exist in nuget => 2.0.0
 GlobalConfiguration.Log = new Logger();
 // thats all 
```
