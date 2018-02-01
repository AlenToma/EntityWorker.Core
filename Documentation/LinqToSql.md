## LinqToSql Result Example
lets test and see how EntityWorker.Core LinqToSql generator looks like.
We will execute a complicated query and see how it gets parsed.
```csharp
            using (var rep = new Repository())
            {
               ISqlQueriable<User> users = rep.Get<User>().Where(x =>
               (x.Role.Name.EndsWith("SuperAdmin") &&
                x.UserName.Contains("alen")) ||
                x.Address.Any(a => (a.AddressName.StartsWith("st") || a.AddressName.Contains("mt")) && a.Id > 0).
                Skip(20).Take(100).Execute();  
                );
                
                List<User> userList = users.Execute();
                var sql = users.ParsedLinqToSql;
            }
```
### Generated sql result
```sql
          -- And here is the generated Sql Query
             SELECT distinct Users.* FROM Users 
             left join [Roles] CEjB on CEjB.[Id] = Users.[Role_Id]
             WHERE (([CEjB].[Name] like String[%SuperAdmin] AND [Users].[UserName] like String[%alen%]) 
             OR  EXISTS (SELECT 1 FROM [Address] 
             INNER JOIN [Address] MJRhcYK on Users.[Id] = MJRhcYK.[User_Id]
             WHERE (([Address].[AddressName] like String[st%] OR [Address].[AddressName] like String[%mt%]) 
             AND ([Address].[Id] > 0))))
             ORDER BY Id
             OFFSET 20
             ROWS FETCH NEXT 100 ROWS ONLY;
          -- All String[], Date[] and Guid[] will be translated to Parameters later on.   
```
