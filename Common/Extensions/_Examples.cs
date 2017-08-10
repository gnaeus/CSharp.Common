using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Common.Extensions;

partial class _Examples
{
    #region ConnectionExtensions

    class SqlRepository
    {
        readonly DbConnection _connection;

        public async Task ExecuteSomeQueryAsync()
        {
            using (await _connection.EnsureOpenAsync())
            {
                // execute some SQL
            }
        }

        public async Task<Stream> ReadFileAsync(int fileId)
        {
            Stream stream = await _connection.QueryBlobAsStreamAsync(
                "SELECT Content FROM Files WHERE Id = @fileId",
                new SqlParameter("@fileId", fileId));

            return stream;
        }
    }

    #endregion
    
    #region MappingExtensions

    class OrderModel
    {
        public int Id { get; set; }
        public ProductModel[] Products { get; set; }
    }

    class ProductModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    class OrderEntity
    {
        public int Id { get; set; }
        public ICollection<ProductEntity> Products { get; } = new HashSet<ProductEntity>();
    }

    class ProductEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    interface IDbSet<TEntity>
    {
        TEntity Add(TEntity entity);
        TEntity Remove(TEntity entity);
    }

    static class OrdersContext
    {
        public static IDbSet<OrderEntity> Orders { get; set; }
        public static IDbSet<ProductEntity> Products { get; set; }
    }

    static class OrderMapper
    {
        public static void UpdateOrder(OrderEntity entity, OrderModel model)
        {
            entity.Id = model.Id;

            entity.Products.MapFrom(model.Products)
                .WithKeys(e => e.Id, m => m.Id)
                .OnAdd(OrdersContext.Products.Add)
                .OnUpdate(Console.WriteLine)
                .OnRemove(OrdersContext.Products.Remove)
                .MapElements(ProductMapper.UpdateProduct);
        }

        public static OrderModel MapOrder(OrderEntity entity)
        {
            return new OrderModel
            {
                Id = entity.Id,

                Products = entity.Products
                    .Select(ProductMapper.MapProduct)
                    .ToArray(),
            };
        }
    }

    static class ProductMapper
    {
        public static void UpdateProduct(ProductEntity entity, ProductModel model)
        {
            entity.Id = model.Id;
            entity.Title = model.Title;
        }

        public static ProductModel MapProduct(ProductEntity entity)
        {
            return new ProductModel
            {
                Id = entity.Id,
                Title = entity.Title,
            };
        }
    }

    #endregion
}