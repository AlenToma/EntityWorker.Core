## Edit, delete and insert
EntityWorker.Core has only one method for inserts and updates.
Depending on the primarykey it will either execute an update or insert.
```csharp
   using (var rep = new Repository())
   {
        var users = rep.Get<User>().Where(x => 
                (x.Role.Name.EndsWith("SuperAdmin") &&
                 x.UserName.Contains("alen")) ||
                 x.Address.Any(a=> a.AddressName.StartsWith("st"))
                ).LoadChildren();
         foreach (User user in users.Execute())
         {
             user.UserName = "test 1";
             user.Role.Name = "Administrator";
             user.Address.First().AddressName = "Changed";
             // now we could do rep.Save(user); but will choose to save the whole list late
         }
        users.Save();    
        // how to delete 
        // remove all the data offcource Roles will be ignored here 
        users.Remove();
        // Remove with expression
        users.RemoveAll(x=> x.UserName.Contains("test"));
        // we could also clone the whole object and insert it as new to the database like this
        var clonedUser = users.Execute().Clone().ClearAllIdsHierarchy();
        foreach (var user in clonedUser)
        {
            // Now this will clone the object to the database, of course all Foreign Key will be automatically assigned.
            // Role wont be cloned here. cource it has IndependedData attr we could choose to clone it too, by invoking
            // .ClearAllIdsHierarchy(true);
            rep.Save(user);
        }
        rep.SaveChanges();
        Console.WriteLine(users.ToJson());
        Console.ReadLine();
   }

```
