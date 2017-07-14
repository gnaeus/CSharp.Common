using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
    /// <summary>
    /// Extensions for updating `ICollection` of some domain entities from `IEnumerable` of the relevant DTOs.
    /// </summary>
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
                EntityKey = entityKey,
                ModelKey = modelKey,
            };
        }

        public static void MapValues<TEntity, TModel, TKey>(
            this MappingConfig<TEntity, TModel, TKey> config,
            Action<TEntity, TModel> mapping)
            where TEntity : class, new()
        {
            ILookup<TKey, TEntity> entityLookup = config.Entities.ToLookup(config.EntityKey);

            config.Entities.Clear();

            foreach (TModel model in config.Models ?? Enumerable.Empty<TModel>())
            {
                TKey key = config.ModelKey(model);
                
                TEntity entity = entityLookup.Contains(key)
                    ? entityLookup[key].First()
                    : new TEntity();

                mapping.Invoke(entity, model);

                config.Entities.Add(entity);
            }
        }
    }
}
