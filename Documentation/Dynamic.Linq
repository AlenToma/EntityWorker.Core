## Dynamic.Linq 
Execute Expression of type string
```csharp
using (var rep = new Repository())
{
string expression ="x.Person.FirstName.EndsWith(\"n\") And (x.Person.FirstName.Contains(\"a\") OR x.Person.FirstName.StartsWith(\"a\"))";
var users = rep.Get<User>().Where(expression).LoadChildren().Execute();
}
```
