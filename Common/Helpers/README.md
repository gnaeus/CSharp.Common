#### tringIntepolationHelper
Simple DSL based on C# 6 String Interpolation for building dynamic SQL queries.

```cs
using static Common.Helpers.StringInterpolationHelper;

class EmployeesFilter
{
    public bool IncludeDepartment { get; set; }
    public int? DepartmentId { get; set; }
    public string[] Names { get; set; }
}

class EmployeesSearchService
{
    public void SearchEmployees(EmployeesFilter filter)
    {
        string sql = $@"
        SELECT
            {@if(filter.IncludeDepartment, @"
                dep.Id,
                dep.Name,"
            )}
            emp.Id,
            emp.Name,
            emp.DepartmentId
        FROM Emloyees AS emp
        {@if(filter.IncludeDepartment, @"
            LEFT JOIN Departments AS dep ON dep.Id = emp.DepartmentId"
        )}
        WHERE
        {@if(filter.DepartmentId != null, @"
            emp.DepartmentId = @DepartmentId",
        @else(@"
            emp.DepartmentId IS NULL"
        ))}
        AND (
            {@foreach(filter.Names, name =>
                $"emp.Name LIKE '{name}%'",
                " OR "
            )}
        )";
    }
}

class ProductsFilter
{
    public string Title { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string[] Tags { get; set; }
    public ProductsSortBy SortBy { get; set; }
}

enum ProductsSortBy { Title, Price }

class ProductsSearchService
{
    public void SearchProducts(ProductsFilter filter)
    {
        string sql = $@"
        SELECT
            p.Title,
            p.Description,
            p.Price
        FROM Products AS p
        WHERE 1 = 1
        {@if(filter.Title != null, @"
            AND p.Title = @Title"
        )}
        {@if(filter.MinPrice != null, @"
            AND p.Price >= @MinPrice"
        )}
        {@if(filter.MaxPrice != null, @"
            AND p.Price <= @MaxPrice"
        )}
        {@if(filter.Tags != null, $@"
            AND (
                SELECT COUNT(1)
                FROM ProductTags AS t
                WHERE t.ProductId = p.Id
                  AND t.Tag IN (@Tags)
            ) = {filter.Tags.Length}"
        )}
        ORDER BY
        {@switch(filter.SortBy,
            @case(ProductsSortBy.Title, " Title ASC"),
            @case(ProductsSortBy.Price, " Price ASC")
        )}";
    }
}
```

### SqlFullTextSearchHepler
Utils for Full Text Search in Microsoft SQL Server

__`string PrepareFullTextQuery(string searchPhrase, bool fuzzy = false, int minWordLength = 3)`__  
Build query for SQL Server FTS Engine CONTAINS function.
Result should be passed through ADO.NET SqlParameter due to preventing SQL Injection.  

```cs
using System.Diagnostics;
using Common.Helpers;

class SqlServerFullTextSearchService
{
    public void SearchArticles(string title = "Я на Cолнышке лежу")
    {
        string ftsQuery = SqlFullTextSearchHepler.PrepareFullTextQuery(title, fuzzy: true);

        Debug.Assert(ftsQuery ==
            "\"cолнышке*\" NEAR \"лежу*\"\n" +
            " OR FORMSOF(FREETEXT, \"cолнышке\")"+
            " AND FORMSOF(FREETEXT, \"лежу\")");

        string sql = @"
        SELECT TOP (10)
            a.Id,
            a.Title,
            a.Content,
            fts.[RANK]
        FROM CONTAINSTABLE(Departments, (Title), @ftsQuery) AS fts
        INNER JOIN Articles AS a ON fts.[KEY] = a.ID
        ORDER BY fts.[RANK] DESC";
    }
}
```

### BitHelper

__`ulong MurmurHash3(ulong key)`__  
Compute [MurMurHash](http://zimbry.blogspot.ru/2011/09/better-bit-mixing-improving-on.html)

__`uint ReverseBits(uint value)`__  
Reverse bits in `[Flags] enum` value for use in `OrderBy()` extension

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;

[Flags]
enum UserRoles
{
    Admin = 1, Moderator = 2, User = 4, Reader = 8,
}

class User
{
    public UserRoles Roles { get; set; }
}

static class UserExtensions
{
    public static IEnumerable<User> OrderByRoles(this IEnumerable<User> users)
    {
        return users.OrderByDescending(u => BitHelper.ReverseBits((uint)u.Roles));
    }
}

```

### FileSystemHelper

__`void CleanDirectory(string path)`__  
Reqursively delete all files and folders from directory.

__`string RemoveInvalidCharsFromFileName(string fileName)`__  
Cleanup `fileName` from invalid characters.

### UriHelper

__`string GetHost(string uriString)`__  
"http://localhost/SomeApp" => "localhost"

__`string AddTrailingSlash(string url)`__  
"http://localhost/SomeApp" => "http://localhost/SomeApp/"

__`string ChangeHost(string absoluteUrl, string host)`__  
("http://localhost:8080/SomeApp", "127.0.0.1") => "http://127.0.0.1:8080/SomeApp"

__`bool CanonicalEqual(string url1, string url2)`__  
"http://localhost/SomeApp" == "http://localhost/someapp/"
