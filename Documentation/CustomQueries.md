## Create Custom ISqlQueryable
```csharp

using (var rep = new Repository())
   {
          //Create a custom ISqlQueryable, you could have store proc or a row sql query
            var cmd = rep.GetSqlCommand("SELECT * FROM Users WHERE UserName = @userName");
            AddInnerParameter(cmd, "userName", userName, System.Data.SqlDbType.NVarChar);
            List<Users> users = DataReaderConverter<User>(cmd).LoadChildren().Execute(); /// Convert the result to Data 
    }
```
