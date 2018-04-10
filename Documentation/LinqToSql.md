## LinqToSql Result Example
lets test and see how EntityWorker.Core LinqToSql generator looks like.
We will execute a complicated query and see how it gets parsed.
```csharp
            using (var rep = new Repository())
            {
               var id = Guid.NewGuid();
               ISqlQueriable<User> users =rep.Get<Person>().Where(x => x.FirstName.Contains("Admin") ||
               string.IsNullOrEmpty(x.FirstName) || string.IsNullOrEmpty(x.FirstName) == false && x.Id != id)
               List<User> userList = users.Execute();
               string sql = users.ParsedLinqToSql;
            }
```
### Generated sql result
```sql
          -- And here is the generated Sql Query
SELECT   [Person].[id], 
         [Person].[firstname], 
         [Person].[lastname], 
         [Person].[surename] 
FROM     person 
WHERE    ((( 
                                    CASE 
                                             WHEN person.firstname LIKE String[%Admin%] THEN 1 
                                             ELSE 0 
                                    END) = 1 
                  OR       (( 
                                             CASE 
                                                      WHEN person.firstname IS NULL THEN 1 
                                                      ELSE 
                                                               CASE 
                                                                        WHEN person.firstname = String[] THEN 1
                                                                        ELSE 0 
                                                               END 
                                             END)) = 0) 
         OR       (((( 
                                                      CASE 
                                                               WHEN person.firstname IS NULL THEN 1
                                                               ELSE 
                                                                        CASE 
                                                                                 WHEN person.firstname = String[] THEN 1
                                                                                 ELSE 0 
                                                                        END 
                                                      END)) = 0) 
                  AND      ( 
                                    person.id <> Guid[51842663-16b5-40aa-8772-a8368733dd10]))) 
GROUP BY [Person].[id], 
         [Person].[firstname], 
         [Person].[lastname], 
         [Person].[surename] 
ORDER BY id offset 0 rowsFETCH next 2147483647 rows only;
ROWS FETCH NEXT 2147483647 ROWS ONLY;
          -- All String[], Date[] and Guid[] will be translated to Parameters later on.   
```
