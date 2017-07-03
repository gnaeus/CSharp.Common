using System.Diagnostics;
using Common.Helpers;
using static Common.Helpers.StringInterpolationHelper;

partial class _Examples
{
    #region StringInterpolationHelper

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

    #endregion

    #region SqlFullTextSearchHepler

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

    #endregion
}
