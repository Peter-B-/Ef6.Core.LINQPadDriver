# Ef6.Core.LINQPadDriver
[![.NET](https://github.com/Peter-B-/Ef6.Core.LINQPadDriver/actions/workflows/build-and-publish.yml/badge.svg?branch=main)](https://github.com/Peter-B-/Ef6.Core.LINQPadDriver/actions/workflows/build-and-publish.yml)
![Nuget](https://img.shields.io/nuget/v/Ef6.Core.LINQPadDriver)

Entity Framework 6 driver for LINQPad 6+ running .Net (not Framework).

## Installation
Open LINQPad and click "Add connection" in the connection overview. In the "Choose Data Context" dialog, select "View more drivers" and check "Show all drivers" in the top center.

Search for `Ef6.Core.LINQPadDriver` and install it.

The driver has been tested with EntityFramework 6.4.4 and dotConnect for Oracle 9.14.1273.

## DB context creation

There should now be a "Entity Framework 6 on .Net Core" option in the "Choose Data Context" dialog. Select it and click "Next". 

Pick your EF6 assembly and select your DbContext type. Provide the full connection string and click "Ok".

Your DbContext must have a public constructor accepting a `nameOrConnectionString` string as a parameter:

```csharp
public class MyDbContext : DbContext
{
    public MyDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
    {
    }
    
    // additional constructors are allowed
}
```

## Connectionstring

Setting the correct connection string can be tricky and the error messages might not be helpful. Here are some tips.

### Model First

The connection string must contain references to the model file, the provider and the actual connection string.
It usually is not possible to reuse the connection string from your `App.Config`, due to formatting issues.

You can generate the connection string by adapting this LINQPad script:

```CSharp
// Requires "EntityFramework" Nuget package

var ecsb = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder
{
	Metadata = "res://*/DemoModel.csdl|res://*/DemoModel.ssdl|res://*/DemoModel.msl",
	Provider = "System.Data.SqlClient",
	ProviderConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DemoModel;Integrated Security=True;MultipleActiveResultSets=True;App=EntityFramework"
};

ecsb.ConnectionString.Dump();
```

The resulting connection string is in the correct format to be pasted into the dialog of the connection.

You can find an example of a Model First context in this repo in `/Ef6Demo/Ef6Demo.ModelFirst` and the matching connection string in
[/Ef6Demo/ModelFirst.linq](/Ef6Demo/ModelFirst.linq) for reference.

### Code First

The connection string for a Code First context is more straight forward. 

You can find an example of a Code First context in this repo in `/Ef6Demo/Ef6Demo.CodeFirst` and the matching connection string in
[/Ef6Demo/CodeFirst.linq](/Ef6Demo/CodeFirst.linq) for reference.


# Grouping
The driver supports grouping DbSets by decorating them with a `System.ComponentModel.Category` attribute.

```csharp
public class MyDbContext : DbContext
{
    [Category("Groups")]
    public virtual DbSet<UserGroup> UserGroups { get; set; }

    [Category("Users")]
    public virtual DbSet<User> Users { get; set; }
}
```

You can also apply the `System.ComponentModel.Category` attribute to the entity:
 ```csharp
[Category("Users")]
public class User
{
    ...
}
```

![Grouping example](docs/groups.png)

