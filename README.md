# Introduction to [EntityWorker.Core](https://www.nuget.org/packages/EntityWorker.Core/)

## EntityFrameWork vs EntityWorker.Core Performance test
![screenshot](https://github.com/AlenToma/EntityWorker.Core/blob/master/EF_VS_EW.PNG?raw=true)

## CodeProject
[EntityWorker-Core-An-Alternative-to-Entity-Framewo](https://www.codeproject.com/Tips/1222424/EntityWorker-Core-An-Alternative-to-Entity-Framewo)

## EntityWorker.Core in Action
[LightData.CMS](https://github.com/AlenToma/LightData.CMS)

## <h1 id="update">Update in >= 1.2.5</h1>
DbEntity is Removed now, you dont have to inherit from it anymore. This way you can use the objects in other project without refering to EntityWorker.Core.

Also, make sure to read about GlobalConfiguration

SqlQueryable now inherits from IOrderedQueryable so we could cast it IQueryable<T> later on.

## Support for Providers
1- Mssql

2- Sqlite

3- Postgresql


## .NET FRAMEWORK SUPPORT 
1- .NETCoreApp 2.0

2- .NETFramework 4.5.1

3- .NETFramework 4.6

4- .NETFramework 4.6.1

5- .NETStandard 2.0
## What is EntityWorker.Core?
EntityWorker.Core is an object-relation mappar that enables .NET developers to work with relational data using objects.
EntityWorker.Core is an alternative to the Entity Framework. It is more flexible and much faster than the Entity Framework.
## Can I use it to an existing database?
Yes, you can easily implement your existing modules and use attributes to map all your Primary Keys and Foreign Key without even
touching the database.
## Expression
EntityWorker.Core has its own provider called ISqlQueryable, which can handle almost every expression like Startwith,
EndWith, Contains and so on
See Code Example for more info.
## Code Example

* 1- [GlobalConfiguration](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/GlobalConfiguration.md)
* 2- [Repository](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Repository.md)
* 3- [Allowed System type](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/System.Type.md)
* 4- [Modules](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/modules.md)
* 5- [Migrations](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Migration.md)
* 6- [Querys](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Query.md)
* 7- [Save](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Save.md)
* 8- [LinqToSql](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/LinqToSql.md)
* 9- [Entity Mappings](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/EntityMappings.md)
* 10- [ObjectChanges](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/ObjectChanges.md)
* 11- [Attributes](https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md)

## Issues
Please report any bugs or improvement you might find.
