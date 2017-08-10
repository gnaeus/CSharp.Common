using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Common.Utils;

partial class _Examples
{
    #region AsyncLazy

    class AsyncService
    {
        readonly AsyncLazy<string> LazyString = new AsyncLazy<string>(async () =>
        {
            await Task.Delay(1000);
            return "lazy string";
        });

        async Task Method()
        {
            string str = await LazyString;
            // do somethig with this str
        }
    }

    #endregion

    #region DisposableStream

    class BlobStreamingService
    {
        readonly DbConnection _connection;

        async Task<Stream> StreamFile(int fileId)
        {
            // error handling is skipped
            await _connection.OpenAsync();
            DbCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT TOP (1) Content FROM Files WHERE Id = @fileId";
            command.Parameters.Add(new SqlParameter("@fileId", fileId));
            DbDataReader reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();

            // reader will be disposed with wrapped stream
            return new DisposableStream(reader, reader.GetStream(0));
        }
    }

    #endregion

    #region QueryBuilder
    
    class EmployeesSearchQueryService
    {
        public string SearchEmployees(EmployeesFilter filter)
        {
            var query = new QueryBuilder();
            var q = query.Append;

            q("SELECT");
            if (filter.IncludeDepartment)
            {
                q("dep.Id,");
                q("dep.Name,");
            }
            {
                q("emp.Id,");
                q("emp.Name,");
                q("emp.DepartmentId,");
            }
            q("FROM Emloyees AS emp");
            if (filter.IncludeDepartment)
            {
                q("LEFT JOIN Departments AS dep ON dep.Id = emp.DepartmentId");
            }
            q("WHERE");
            if (filter.DepartmentId != null)
            {
                q("emp.DepartmentId = @DepartmentId");
            }
            else
            {
                q("emp.DepartmentId IS NULL");
            }

            return query.ToString();
        }
    }
    
    class ProductsSearchQueryService
    {
        public string SearchProducts(ProductsFilter filter)
        {
            var query = new QueryBuilder();
            var q = query.Append;
            
            q("SELECT");
            {
                q("p.Title,");
                q("p.Description,");
                q("p.Price");
            }
            q("FROM Products AS p");
            q("WHERE 1 = 1");
            if (filter.Title != null)
            {
                q("AND p.Title = @Title");
            }
            if (filter.MinPrice != null)
            {
                q("AND p.Price = @MinPrice");
            }
            if (filter.MaxPrice != null)
            {
                q("AND p.Price = @MaxPrice");
            }
            if (filter.Tags != null)
            {
                q("AND (");
                {
                    q("SELECT COUNT(1)");
                    q("FROM ProductTags AS t");
                    q("WHERE t.ProductId = p.Id");
                    q("AND t.Tag IN (@Tags)");
                }
                q(") = @TagsLength");
            }
            q("ORDER BY");
            switch (filter.SortBy)
            {
                case ProductsSortBy.Title:
                    q("Title ASC");
                    break;
                case ProductsSortBy.Price:
                    q("Price ASC");
                    break;
            }

            return query.ToString();
        }
    }

    #endregion
}
