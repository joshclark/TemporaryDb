#TemporaryDb

This library can be used to create a temporary database that can be used for testing and then dropped once the tests are complete.  Currently, only LocalDb is supported.  The code attempts to connect to LocalDb on localhost.


## Getting Started
Install the [Nuget package](https://www.nuget.org/packages/TemporaryDb/) and write the following code:

```c#
using System;
using TemporaryDb;

public class Program
{
    public static void Main(string[] args)
    {
		using (var db = new TempLocalDb("testdb"))
        {
           UseTheDatabase(db.ConnectionString);
        }
    }
}
```

<hr>

[![Build status](https://ci.appveyor.com/api/projects/status/qli3fwkqmmv7re4b?svg=true)](https://ci.appveyor.com/project/joshclark/TemporaryDb) <a href="https://www.nuget.org/packages/TemporaryDb/"><img src="http://img.shields.io/nuget/v/TemporaryDb.svg?style=flat-square" alt="NuGet version" height="18"></a> 

