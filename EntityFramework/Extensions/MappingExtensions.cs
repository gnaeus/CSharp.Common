using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntityFramework.Extensions
{
    public static class MappingExtensions
    {
        public struct DbSetMappingConfig<TEntity, TModel>
            where TEntity : class
        {
            public IDbSet<TEntity> DbSet;
            public ICollection<TEntity> Entities;
            public IEnumerable<TModel> Models;
        }

        public struct DbSetMappingConfig<TEntity, TModel, TKey>
            where TEntity : class
        {
            public IDbSet<TEntity> DbSet;
            public ICollection<TEntity> Entities;
            public IEnumerable<TModel> Models;
            public Func<TEntity, TKey> EntityKey;
            public Func<TModel, TKey> ModelKey;
        }

        public static DbSetMappingConfig<TEntity, TModel> UpdateCollection<TEntity, TModel>(
            this IDbSet<TEntity> table, ICollection<TEntity> entities, IEnumerable<TModel> models)
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
            };
        }

        public static void MapValues<TEntity, TModel, TKey>(
            this DbSetMappingConfig<TEntity, TModel, TKey> config,
            Func<TModel, TEntity, TEntity> mapping)
            where TEntity : class
        {
            ILookup<TKey, TEntity> entityLookup = config.Entities.ToLookup(config.EntityKey);

            HashSet<TKey> modelKeys = new HashSet<TKey>();

            foreach (TModel model in config.Models ?? Enumerable.Empty<TModel>())
            {
                TKey key = config.ModelKey(model);

                modelKeys.Add(key);

                if (entityLookup.Contains(key))
                {
                    TEntity entity = entityLookup[key].First();

                    mapping.Invoke(model, entity);
                }
                else
                {
                    config.DbSet.Add(mapping.Invoke(model, null));
                }
            }

            foreach (TEntity entity in config.Entities)
            {
                if (!modelKeys.Contains(config.EntityKey(entity)))
                {
                    config.DbSet.Remove(entity);
                }
            }
        }

        public static void UpdateValues<TEntity, TModel, TKey>(
            this DbSetMappingConfig<TEntity, TModel, TKey> config,
            Action<TModel, TEntity> mapping)
            where TEntity : class, new()
        {
            MapValues(config, (model, entity) =>
            {
                mapping.Invoke(model, entity ?? (entity = new TEntity()));
                return entity;
            });
        }
    }
}
