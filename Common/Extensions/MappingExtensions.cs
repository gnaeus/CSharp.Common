using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
    public static class MappingExtensions
    {
        public struct MappingConfig<TEntity, TModel>
        {
            public ICollection<TEntity> Entities;
            public IEnumerable<TModel> Models;
        }

        public struct MappingConfig<TEntity, TModel, TKey>
        {
            public ICollection<TEntity> Entities;
            public IEnumerable<TModel> Models;
            public Func<TEntity, TKey> EntityKey;
            public Func<TModel, TKey> ModelKey;
        }

        public static MappingConfig<TEntity, TModel> UpdateFrom<TEntity, TModel>(
            this ICollection<TEntity> entities, IEnumerable<TModel> models)
        {
            return new MappingConfig<TEntity, TModel>
            {
                Entities = entities,
                Models = models,
            };
        }

        public static MappingConfig<TEntity, TModel, TKey> WithKeys<TEntity, TModel, TKey>(
            this MappingConfig<TEntity, TModel> config,
            Func<TEntity, TKey> entityKey, Func<TModel, TKey> modelKey)
        {
            return new MappingConfig<TEntity, TModel, TKey>
            {
                Entities = config.Entities,
                Models = config.Models,
            };
        }

        public static void MapValues<TEntity, TModel, TKey>(
            this MappingConfig<TEntity, TModel, TKey> config,
            Func<TModel, TEntity, TEntity> mapping)
            where TEntity : class
        {
            ILookup<TKey, TEntity> entityLookup = config.Entities.ToLookup(config.EntityKey);

            config.Entities.Clear();

            foreach (TModel model in config.Models ?? Enumerable.Empty<TModel>())
            {
                TKey key = config.ModelKey(model);

                if (entityLookup.Contains(key))
                {
                    TEntity entity = entityLookup[key].First();

                    config.Entities.Add(mapping.Invoke(model, entity));
                }
                else
                {
                    config.Entities.Add(mapping.Invoke(model, null));
                }
            }
        }

        public static void MapValues<TEntity, TModel, TKey>(
            this MappingConfig<TEntity, TModel, TKey> config,
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
