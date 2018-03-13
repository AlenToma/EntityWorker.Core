  ## Xml Serializing and deserializing.
  Entityworker.Core has its own Xml handler that dont require the object to be Serializable. lets se how it work
  
  ```csharp
   using (var rep = new Repository())
   {
    var usersXmlString = rep.Get<User>().LoadChildren().Xml();
    // Xml() will execlude all properties that has XmlIgnore Attributes
    // Convert it Back
    // AllProperties with XmlIgnore attributes will be loaded back from the database if Primary key exist withing the Xml string
    ISqlQueryable<User> users = rep.FromXml<User>(usersXmlString).LoadChildren();
    List<User> userList = users.Execute();
    /// Or
    users.Save();
    user.SaveChanges()
   }
   ```
