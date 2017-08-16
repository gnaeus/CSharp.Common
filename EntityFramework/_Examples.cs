using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EntityFramework.Common.Extensions;

partial class _Examples
{    
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

            var tableAndSchema = _context.GetTableAndSchemaName(typeof(Passport));
            // tableAndSchema.TableName == "Passports"; tableAndSchema.Schema == "dbo"

            // uses existing transaction, otherwise creates new one
            _context.ExecuteInTransaction(() =>
            {
                _context.SaveChanges();
                _context.SaveChanges();
            });

            using (IDbTransaction transaction = _context.BeginTransaction())
            {
                _context.SaveChanges();
                transaction.Commit();
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
