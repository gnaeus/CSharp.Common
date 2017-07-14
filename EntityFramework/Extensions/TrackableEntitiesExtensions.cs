using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using EntityFramework.Common.Entities;

namespace EntityFramework.Common.Extensions
{
    public static partial class DbContextExtensions
    {
        /// <summary>
        /// Populate special properties for all Trackable Entities in context.
        /// </summary>
        public static void UpdateTrackableEntities(this DbContext context)
        {
            DateTime utcNow = DateTime.UtcNow;

            var entries = context.GetChangedEntries(
                EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var dbEntry in entries)
            {
                UpdateTrackableEntity(dbEntry, utcNow);
            }
        }


        private static void UpdateTrackableEntity(DbEntityEntry dbEntry, DateTime utcNow)
        {
            object entity = dbEntry.Entity;

            switch (dbEntry.State)
            {
                case EntityState.Added:
                    var creationTrackable = entity as ICreationTrackable;
                    if (creationTrackable != null)
                    {
                        creationTrackable.CreatedUtc = utcNow;
                    }
                    break;

                case EntityState.Modified:
                    var modificatonTrackable = entity as IModificationTrackable;
                    if (modificatonTrackable != null)
                    {
                        modificatonTrackable.UpdatedUtc = utcNow;
                    }
                    break;

                case EntityState.Deleted:
                    var softDeletable = entity as ISoftDeletable;
                    if (softDeletable != null)
                    {
                        var deletionTrackable = entity as IDeletionTrackable;
                        if (deletionTrackable != null)
                        {
                            deletionTrackable.DeletedUtc = utcNow;
                        }

                        softDeletable.IsDeleted = true;
                        dbEntry.State = EntityState.Modified;
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
