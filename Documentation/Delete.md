## Delete items 
entityworker delete items hierarki. lets se how this works

```csharp
using (var rep = new Repository())
{
int userId = 1;
// See here i made sure to load children. Delete method will make sure to delete all children 
// that dose not contain IndependedData Attribute
rep.Get<User>().Where(x=> x.Id == userId).LoadChildren().Delete().SaveChanges();

}


```
