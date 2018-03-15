## Package Handler
Create Protected package that contains files and data for backup purpose or moving data from one location to another.
Note that this package can only be readed by EntityWorker.Core
```csharp
// Create class that inherit from PackageEntity
  public class Package : EntityWorker.Core.Object.Library.PackageEntity
    {
        // List of objects
        public override List<object> Data { get; set; }
        // List of files
        public override List<byte[]> Files { get; set; }
    }
            using (var rep = new Repository())
            {
                var users = rep.Get<User>().LoadChildren().Execute();
                // You could save the result to a file or even database
                byte[] package = rep.CreatePackage(new Package() { Data = users.Cast<object>().ToList() });
                // read the package, convert the byte[] to Package 
                var readerPackage = rep.GetPackage<Package>(package);
                Console.WriteLine((readerPackage.Data.Count <= 0 ? "Failed" : "Success"));
            }

```
