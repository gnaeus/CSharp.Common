using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using EntityFramework.Common.Entities;

namespace EntityFramework.Common.Extensions
{
    public static partial class DbContextExtensions
    {
        /// <summary>
        /// Populate special properties for all Auditable Entities in context.
        /// </summary>
        public static void UpdateAuditableEntities<TUserId>(this DbContext context, TUserId editorUserId)
            where TUserId : struct
        {
            DateTime utcNow = DateTime.UtcNow;

            var entries = context.GetChangedEntries(
                EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries)
            {
                UpdateAuditableEntity(entry, utcNow, editorUserId);
            }
        }

        /// <summary>
        /// Populate special properties for all Auditable Entities in context.
        /// </summary>
        public static void UpdateAuditableEntities(this DbContext context, string editorUser)
        {
            DateTime utcNow = DateTime.UtcNow;

            var entries = context.GetChangedEntries(
                EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries)
            {
                UpdateAuditableEntity(entry, utcNow, editorUser);
            }
        }

        /// <summary>
        /// Populate special properties for all Trackable Entities in context.
        /// </summary>
        public static void UpdateTrackableEntities(this DbContext context)
        {
            DateTime utcNow = DateTime.UtcNow;

            var entries = context.GetChangedEntries(
                EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries)
            {
                UpdateTrackableEntity(entry, utcNow);
            }
        }

        private static void UpdateAuditableEntity<TUserId>(
            DbEntityEntry entry, DateTime utcNow, TUserId editorUserId)
            where TUserId : struct
        {
            object entity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    var creationAuditable = entity as ICreationAuditable<TUserId>;
                    if (creationAuditable != null)
                    {
                        creationAuditable.CreatorUserId = editorUserId;
                    }
                    break;

                case EntityState.Modified:
                    var modificationAuditable = entity as IModificationAuditable<TUserId>;
                    if (modificationAuditable != null)
                    {
                        modificationAuditable.UpdaterUserId = editorUserId;
                    }
                    break;

                case EntityState.Deleted:
                    var deletionAuditable = entity as IDeletionAuditable<TUserId>;
                    if (deletionAuditable != null)
                    {
                        deletionAuditable.DeleterUserId = editorUserId;
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }
            
            UpdateTrackableEntity(entry, utcNow);
        }

        private static void UpdateAuditableEntity(
            DbEntityEntry entry, DateTime utcNow, string editorUser)
        {
            object entity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    var creationAuditable = entity as ICreationAuditable;
                    if (creationAuditable != null)
                    {
                        creationAuditable.CreatorUser = editorUser;
                    }
                    break;

                case EntityState.Modified:
                    var modificationAuditable = entity as IModificationAuditable;
                    if (modificationAuditable != null)
                    {
                        modificationAuditable.UpdaterUser = editorUser;
                    }
                    break;

                case EntityState.Deleted:
                    var deletionAuditable = entity as IDeletionAuditable;
                    if (deletionAuditable != null)
                    {
                        deletionAuditable.DeleterUser = editorUser;
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }

            UpdateTrackableEntity(entry, utcNow);
        }

        /// <remarks>
        /// EF automatically detects if byde[] RowVersion is changed by reference (not only by value)
        /// and gentrates code like 'DECLARE @p int; UPDATE [Table] SET @p = 0 WHERE RowWersion = ...'
        /// </remarks>
        private static void UpdateTrackableEntity(DbEntityEntry entry, DateTime utcNow)
        {
            object entity = entry.Entity;

            switch (entry.State)
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
                        var deletionTrackable = entry as IDeletionTrackable;
                        if (deletionTrackable != null)
                        {
                            deletionTrackable.DeletedUtc = utcNow;
                        }

                        softDeletable.IsDeleted = true;
                        entry.State = EntityState.Modified;
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }

            if ((entry.State & (EntityState.Modified | EntityState.Deleted)) != 0)
            {
                var optimisticConcurrent = entity as IOptimisticConcurrent;
                if (optimisticConcurrent != null)
                {
                    // take row version from entity that modified by client
                    entry.Property("RowVersion").OriginalValue = optimisticConcurrent.RowVersion;
                }
            }
        }
    }
}
