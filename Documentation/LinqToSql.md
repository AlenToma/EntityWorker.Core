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
SELECT
   [Person].[Id],
   [Person].[FirstName],
   [Person].[LastName],
   [Person].[SureName] 
FROM
   [Person] 
WHERE
   (
((
      CASE
         WHEN
            [Person].[FirstName] LIKE String[ % Admin % ] 
         THEN
            1 
         ELSE
            0 
      END
) = 1 
      OR 
      (
(
         CASE
            WHEN
               [Person].[FirstName] IS NULL 
            THEN
               1 
            ELSE
               CASE
                  WHEN
                     [Person].[FirstName] = String[] 
                  THEN
                     1 
                  ELSE
                     0 
               END
         END
)
      )
      = 1) 
      OR 
      (
(((
         CASE
            WHEN
               [Person].[FirstName] IS NULL 
            THEN
               1 
            ELSE
               CASE
                  WHEN
                     [Person].[FirstName] = String[] 
                  THEN
                     1 
                  ELSE
                     0 
               END
         END
)) = 0) 
         AND 
         (
            [Person].[Id] <> Guid[d82d1a00 - 5eb9 - 4017 - 8c6e - 23a631757532]
         )
      )
   )
GROUP BY
   [Person].[Id], [Person].[FirstName], [Person].[LastName], [Person].[SureName] 
ORDER BY
   Id OFFSET 0 ROWS FETCH NEXT 2147483647 ROWS ONLY;
          -- All String[], Date[] and Guid[] will be translated to Parameters later on.   
```
