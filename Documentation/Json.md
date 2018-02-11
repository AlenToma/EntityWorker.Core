  ## Json Serializing and deserializing.
  Entityworker.Core has its own json handler. lets se how it work
  
  ```csharp
   using (var rep = new Repository())
   {
    var usersJsonString = rep.Get<User>().LoadChildren().Json();
    // Json() will execlude all properties that has JsonIgnore Attributes
    // Convert it Back
    // All JsonIgnore attributes will be loaded back from the database if Primary key exist withing the json string
    ISqlQueryable<User> users = rep.FromJson<User>(usersJsonString).LoadChildren();
    List<User> userList = users.Execute();
    /// Or
    users.Save();
    user.SaveChanges()
   }
   ```
