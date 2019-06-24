# Introduction to [EntityWorker.Core](https://www.nuget.org/packages/EntityWorker.Core/)
EntityWorker is an object-relation mapper(ORM) that enable .NET developers to work with relations data using objects.
EntityWorker is an alternative to entityframwork. is more flexible and much faster than entity framework.
## EntityFrameWork vs EntityWorker.Core Performance test
![screenshot](https://github.com/AlenToma/EntityWorker.Core/blob/master/EF_VS_EW.PNG?raw=true)

## CodeProject
* [CodeProject](https://www.codeproject.com/Tips/1222424/EntityWorker-Core-An-Alternative-to-Entity-Framewo)

## EntityWorker.Core in Action
* [IProduct](https://github.com/AlenToma/IProduct)

## Support for Providers
* Mssql
* Sqlite
* Postgresql
You will have to install the correct provider package.
Depending on which provider you use, you will have to install [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/4.7.0-preview6.19303.8) for mssql , [Npgsql](https://www.nuget.org/packages/Npgsql/) for pgsql and
[System.Data.SQLite](https://www.nuget.org/packages/System.Data.SQLite/) for SQLite. You will be noticed when the providers package is missing
## .NET FRAMEWORK SUPPORT 
* .NETCoreApp 2.0
* .NETFramework 4.5.1
* .NETStandard 2.0
## Can I use it to an existing database?
Yes, you can easily implement your existing modules and use attributes or IModuleBuilder to map all your Primary Keys and Foreign Key without even touching the database.
## Expression
EntityWorker.Core has its own provider called ISqlQueryable, which can handle almost every expression like Startwith,
EndWith, Contains and so on
See Code Example for more info.

## Code Example

* [GlobalConfiguration](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/GlobalConfiguration.md)
* [Extension](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Extension.md)
* [Logger](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/logger.md)
* [Repository](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md)
* [Allowed System type](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/System.Type.md)
* [Modules](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/modules.md)
* [Migrations](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Migration.md)
* [Querys](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Query.md)
* [Json](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Json.md)
* [Xml](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Xml.md)
* [Save](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Save.md)
* [Delete](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Delete.md)
* [LinqToSql](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/LinqToSql.md)
* [Search with Pagination](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/SearchWithPagination.md)
* [Store Procedure](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/StoredProcedure.md)
* [Dynamic linq](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Dynamic.Linq.md)
* [Custom ISqlQueryable](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/CustomQueries.md)
* [Entity Mappings](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/EntityMappings.md)
* [ObjectChanges](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/ObjectChanges.md)
* [Package](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Package.md)
* [Example of Many to Many Relationships](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Many%20to%20Many%20Relationships.md)
* [Attributes](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md)
## Issues
Please report any bugs or improvement you might find.
