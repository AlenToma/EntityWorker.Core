# [EntityWorker.Core](https://www.nuget.org/packages/EntityWorker.Core/)

## Introduction

EntityWorker is an Object-Relational Mapper(ORM) that enables .NET developers to work with relational data using objects. It is a faster, more flexible alternative to EntityFramework.

You can easily integrate EntityWorker with your existing database by using attributes or `IModuleBuilder` to map all of your primary/foreign keys without touching the existing database.

EntityWorker.Core has its own provider called ISqlQueryable which can handle almost every expression like `Startwith`,
`EndWith`, `Contains` and `Any` etc. 

See Code Examples below for more info.

## EntityFramework vs EntityWorker.Core performance test
![screenshot](https://github.com/AlenToma/EntityWorker.Core/blob/master/EF_VS_EW.PNG?raw=true)

## CodeProject
* [CodeProject (Introduction)](https://www.codeproject.com/Tips/5254684/Introduction-to-EntityWorker)
* [CodeProject (Detail)](https://www.codeproject.com/Tips/1222424/EntityWorker-Core-An-Alternative-to-Entity-Framewo)

## EntityWorker.Core in action
* [IProduct](https://github.com/AlenToma/IProduct)

## Support for ADO.NET providers
* MSSQL 14+ (via [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/4.7.0-preview6.19303.8))
* SQLite (via [System.Data.SQLite](https://www.nuget.org/packages/System.Data.SQLite/))
* PostgreSQL (via [Npgsql](https://www.nuget.org/packages/Npgsql/))

## .NET Framework Support
* .NETCoreApp 2.0
* .NETFramework 4.5.1
* .NETStandard 2.0

## Code Examples

* [GlobalConfiguration](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/GlobalConfiguration.md)
* [Extension](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Extension.md)
* [Logger](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/logger.md)
* [Repository](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md)
* [Allowed System Type](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/System.Type.md)
* [Modules](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/modules.md)
* [Migrations](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Migration.md)
* [Queries](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Query.md)
* [JSON](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Json.md)
* [XML](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Xml.md)
* [Save](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Save.md)
* [Delete](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Delete.md)
* [LINQToSQL](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/LinqToSql.md)
* [Search with Pagination](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/SearchWithPagination.md)
* [Stored Procedure](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/StoredProcedure.md)
* [Dynamic LINQ](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Dynamic.Linq.md)
* [Custom ISqlQueryable](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/CustomQueries.md)
* [Entity Mappings](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/EntityMappings.md)
* [ObjectChanges](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/ObjectChanges.md)
* [Package](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Package.md)
* [Example of Many to Many Relationships](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Many%20to%20Many%20Relationships.md)
* [Attributes](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md)

## Used dependencies
* [FastDeepCloner](https://www.nuget.org/packages/FastDeepCloner)

## Issues
Please report any bugs or suggest any improvements you find via [GitHub Issues](https://github.com/AlenToma/EntityWorker.Core/issues).
