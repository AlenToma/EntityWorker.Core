## ObjectChanges
lets see how EntityWorker gets the changed objects 
```csharp
        using (var rep = new Repository())
            {
                //var m = rep.Get<User>().
                var user = rep.Get<User>().LoadChildren().ExecuteFirstOrDefault();
                user.UserName = "hahahadfsfddfsdfhaha";
                var changes = rep.GetObjectChanges(user); 
                var oldValue = changes.First().OldValue;
                var newValue = changes.First().NewValue;
                var propertyName = changes.First().PropertyName;
                rep.Save(user);
                var changes2 = rep.GetObjectChanges(user);
                rep.SaveChanges();
                var changes3 = rep.GetObjectChanges(user);
            }

```
