﻿using System;
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

            foreach (var dbEntry in entries)
            {
                UpdateAuditableEntity(dbEntry, utcNow, editorUserId);
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

            foreach (var dbEntry in entries)
            {
                UpdateAuditableEntity(dbEntry, utcNow, editorUser);
            }
        }
        
        private static void UpdateAuditableEntity<TUserId>(
            DbEntityEntry dbEntry, DateTime utcNow, TUserId editorUserId)
            where TUserId : struct
        {
            object entity = dbEntry.Entity;

            switch (dbEntry.State)
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
            
            UpdateTrackableEntity(dbEntry, utcNow);
        }

        private static void UpdateAuditableEntity(
            DbEntityEntry dbEntry, DateTime utcNow, string editorUser)
        {
            object entity = dbEntry.Entity;

            switch (dbEntry.State)
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

            UpdateTrackableEntity(dbEntry, utcNow);
        }
    }
}