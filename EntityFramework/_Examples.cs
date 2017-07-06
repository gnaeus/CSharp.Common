using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EntityFramework.Common.Extensions;
using EntityFramework.Common.Utils;

partial class _Examples
{
    #region JsonField

    class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }

        private JsonField<Address> _address;
        internal string AddressJson // used by EntityFramework
        {
            get { return _address.Json; }
            set { _address.Json = value; }
        }
        public Address Address // used by application code
        {
            get { return _address.Value; }
            set { _address.Value = value; }
        }

        // collection initialization by default
        private JsonField<ICollection<string>> _phones = new HashSet<string>();
        internal string PhonesJson // used by EntityFramework
        {
            get { return _phones.Json; }
            set { _phones.Json = value; }
        }
        public ICollection<string> Phones // used by application code
        {
            get { return _phones.Value; }
            set { _phones.Value = value; }
        }
    }

    [NotMapped]
    class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
        public string Building { get; set; }
    }

    #endregion

    #region MappingExtensions

    class ProductModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
    
    class ProductEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    class ProductsContext : DbContext
    {
        public DbSet<ProductEntity> Products { get; set; }
    }

    class ProductService
    {
        readonly ProductsContext _context;

        public void UpdateProducts(ProductModel[] productModels)
        {
            int[] productIds = productModels.Select(p => p.Id).ToArray();

            List<ProductEntity> productEntities = _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToList();

            _context.Products.UpdateCollection(productEntities, productModels)
                .WithKeys(e => e.Id, m => m.Id)
                .MapValues(UpdateProduct);

            _context.SaveChanges();
        }

        private static void UpdateProduct(ProductEntity entity, ProductModel model)
        {
            entity.Id = model.Id;
            entity.Title = model.Title;
        }
    }

    #endregion

    #region DbContextExtensions

    class Passport
    {
        [Key, Column(Order = 1)]
        public string Series { get; set; }

        [Key, Column(Order = 2)]
        public string Code { get; set; }
    }

    class PassportsContext : DbContext
    {
        public DbSet<Passport> Passports { get; }
    }

    class PassportsService
    {
        readonly PassportsContext _context;

        void Method()
        {
            var passport = new Passport { Series = "123456", Code = "7890" };

            _context.Passports.Add(passport);

            var keys = _context.GetPrimaryKeys(passport);
            // keys == [{ Key = "Series", Value = "123456" }, { Key = "7890", Value = "7890" }]

            var addedEntities = _context.GetChangedEntries(EntityState.Added);
            // addedEntities[0].Entry == passport;

            using (IDbTransaction transaction = _context.BeginTransaction())
            {
                _context.SaveChanges();
            }
        }
    }
    
    #endregion

    #region QueriableExtensions

    class Post
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public DateTime Date { get; set; }
    }

    class BlogContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
    }

    class PostsService
    {
        readonly BlogContext _context;

        public async Task<List<Post>> GetPostsAsync(DateTime sinceDate, IEnumerable<int> authorIds)
        {
            return await _context.Posts
                .Where(p => p.Date >= sinceDate)
                .ExecuteChunkedInQueryAsync(p => p.AuthorId, authorIds, chunkSize: 100);
        }
    }

    #endregion
}
