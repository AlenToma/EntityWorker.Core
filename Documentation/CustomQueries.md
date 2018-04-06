## Create Custom ISqlQueryable
```csharp

   using (var rep = new Repository())
   {
            //Create a custom ISqlQueryable, you could have store proc or a row sql query
            var cmd = rep.GetSqlCommand("SELECT * FROM Users WHERE UserName = @userName");
            rep.AddInnerParameter(cmd, "userName", userName, System.Data.SqlDbType.NVarChar);
            /// Convert the result to Data
            List<Users> users = DataReaderConverter<User>(cmd).LoadChildren().Execute(); 
            // Or use this to convert an unknown object eg custom object
            List<Users> users = (List<Users>)DataReaderConverter(cmd, typeof(User)); 
    }
```
