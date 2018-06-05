## Dynamic.Linq 
Execute Expression of type string. and convert it to sql 

For expression way have a look at [Dynamic.Ling](https://github.com/kahanu/System.Linq.Dynamic)
```csharp
using (var rep = new Repository())
{
string expression ="x.Person.FirstName.EndsWith(\"n\") And (x.Person.FirstName.Contains(\"a\") OR x.Person.FirstName.StartsWith(\"a\"))";
var users = rep.Get<User>().Where(expression).LoadChildren().Execute();
}
```
