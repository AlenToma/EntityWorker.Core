## Store Procedure
Here is how you could use store procedure in entityworker

```sql
-- DB
CREATE PROCEDURE [dbo].[GetPerson]
                    @FirstName varchar(50)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    select * from Person where FirstName like @FirstName +'%'
                END

```


```csharp
// Code
  var cmd = rep.GetStoredProcedure("GetPerson");
      rep.AddInnerParameter(cmd, "FirstName", "Admin");
      ISqlQueryable<Person><person> data = rep.DataReaderConverter<Person><person>(cmd).LoadChildren();
      List<Person><person> persons = data.Execute();
      // Or custom Class
      List<Person><person> persons = rep.DataReaderConverter(cmd, typeof(Person));

```
