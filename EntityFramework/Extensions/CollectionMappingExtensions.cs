using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntityFramework.Common.Extensions
{
    /// <summary>
    /// Extensions for updating `ICollection` of some domain entities from `IEnumerable` of the relevant DTOs.
    /// </summary>
    public static partial class DbSetExtensions
    {
        public struct DbSetMappingConfig<TEntity, TModel>
            where TEntity : class
        {
            internal IDbSet<TEntity> DbSet;
            internal ICollection<TEntity> Entities;
            internal IReadOnlyCollection<TModel> Models;
        }

        public struct DbSetMappingConfig<TEntity, TModel, TKey>
            where TEntity : class
        {
            internal IDbSet<TEntity> DbSet;
            internal ICollection<TEntity> Entities;
            internal IReadOnlyCollection<TModel> Models;
            internal Func<TEntity, TKey> EntityKey;
            internal Func<TModel, TKey> ModelKey;
        }

        public static DbSetMappingConfig<TEntity, TModel> UpdateCollection<TEntity, TModel>(
            this IDbSet<TEntity> table, ICollection<TEntity> entities, IReadOnlyCollection<TModel> models)
            where TEntity : class
        {
            return new DbSetMappingConfig<TEntity, TModel>
            {
                DbSet = table,
                Entities = entities,
                Models = models,
            };
        }

        public static DbSetMappingConfig<TEntity, TModel, TKey> WithKeys<TEntity, TModel, TKey>(
            this DbSetMappingConfig<TEntity, TModel> config,
            Func<TEntity, TKey> entityKey, Func<TModel, TKey> modelKey)
            where TEntity : class
        {
            return new DbSetMappingConfig<TEntity, TModel, TKey>
            {
                DbSet = config.DbSet,
                Entities = config.Entities,
                Models = config.Models,
                EntityKey = entityKey,
                ModelKey = modelKey,
            };
        }

        public static void MapValues<TEntity, TModel, TKey>(
            this DbSetMappingConfig<TEntity, TModel, TKey> config,
            Action<TEntity, TModel> mapping)
            where TEntity : class, new()
        {
            ILookup<TKey, TEntity> entityLookup = config.Entities.ToLookup(config.EntityKey);

            IEnumerable<TModel> models = config.Models ?? Enumerable.Empty<TModel>();

            HashSet<TKey> modelKeys = new HashSet<TKey>(models.Select(config.ModelKey));

            foreach (TEntity entity in config.Entities)
            {
                if (!modelKeys.Contains(config.EntityKey(entity)))
                {
                    config.DbSet.Remove(entity);
                }
            }
            
            config.Entities.Clear();

            foreach (TModel model in models)
            {
                TKey key = config.ModelKey(model);

                if (entityLookup.Contains(key))
                {
                    TEntity entity = entityLookup[key].First();

                    mapping.Invoke(entity, model);

                    config.Entities.Add(entity);
                }
                else
                {
                    TEntity entity = new TEntity();

                    mapping.Invoke(entity, model);

                    config.Entities.Add(entity);

                    // TODO: is it required?
                    config.DbSet.Add(entity);
                }
            }   
        }
    }
}
