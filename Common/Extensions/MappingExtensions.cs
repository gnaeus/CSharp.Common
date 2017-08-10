using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
    /// <summary>
    /// Extensions for updating <see cref="ICollection{T}"/> of some domain
    /// entities from <see cref="IEnumerable{T}"/> of the relevant models.
    /// </summary>
    public static class MappingExtensions
    {
        public static Mapping<TEntity, TModel> MapFrom<TEntity, TModel>(
            this ICollection<TEntity> entities, IEnumerable<TModel> models)
            where TEntity : class, new()
        {
            return new Mapping<TEntity, TModel>(entities, models);
        }

        public struct Mapping<TEntity, TModel>
            where TEntity : class, new()
        {
            readonly ICollection<TEntity> _entities;
            readonly IEnumerable<TModel> _models;

            public Mapping(ICollection<TEntity> entities, IEnumerable<TModel> models)
            {
                _entities = entities;
                _models = models;
            }

            public Mapping<TEntity, TModel, TKey> WithKeys<TKey>(
                Func<TEntity, TKey> entityKey, Func<TModel, TKey> modelKey)
            {
                return new Mapping<TEntity, TModel, TKey>(_entities, _models, entityKey, modelKey);
            }
        }

        public class Mapping<TEntity, TModel, TKey>
            where TEntity : class, new()
        {
            readonly ICollection<TEntity> _entities;
            private IEnumerable<TModel> _models;
            readonly Func<TEntity, TKey> _entityKey;
            readonly Func<TModel, TKey> _modelKey;
            private Action<TEntity> _onAdd;
            private Action<TEntity> _onUpdate;
            private Action<TEntity> _onRemove;

            public Mapping(
                ICollection<TEntity> entities, IEnumerable<TModel> models,
                Func<TEntity, TKey> entityKey, Func<TModel, TKey> modelKey)
            {
                _entities = entities;
                _models = models;
                _entityKey = entityKey;
                _modelKey = modelKey;
            }

            public Mapping<TEntity, TModel, TKey> OnAdd(Action<TEntity> action)
            {
                _onAdd += action;
                return this;
            }

            public Mapping<TEntity, TModel, TKey> OnAdd<T>(Func<TEntity, T> action)
            {
                _onAdd += entity => action.Invoke(entity);
                return this;
            }

            public Mapping<TEntity, TModel, TKey> OnUpdate(Action<TEntity> action)
            {
                _onUpdate += action;
                return this;
            }

            public Mapping<TEntity, TModel, TKey> OnUpdate<T>(Func<TEntity, T> action)
            {
                _onUpdate += entity => action.Invoke(entity);
                return this;
            }

            public Mapping<TEntity, TModel, TKey> OnRemove(Action<TEntity> action)
            {
                _onRemove += action;
                return this;
            }

            public Mapping<TEntity, TModel, TKey> OnRemove<T>(Func<TEntity, T> action)
            {
                _onRemove += entity => action.Invoke(entity);
                return this;
            }

            public void MapElements(Action<TEntity, TModel> mapping)
            {
                if (_models == null)
                {
                    return;
                }
                
                if (_onRemove != null)
                {
                    if (!(_models is IReadOnlyCollection<TModel>))
                    {
                        _models = _models.ToArray();
                    }

                    HashSet<TKey> modelKeys = new HashSet<TKey>(_models.Select(_modelKey));
                    
                    foreach (TEntity entity in _entities)
                    {
                        if (!modelKeys.Contains(_entityKey.Invoke(entity)))
                        {
                            _onRemove.Invoke(entity);
                        }
                    }
                }

                ILookup<TKey, TEntity> entityLookup = _entities.ToLookup(_entityKey);
                
                _entities.Clear();

                foreach (TModel model in _models)
                {
                    TKey key = _modelKey.Invoke(model);

                    if (entityLookup.Contains(key))
                    {
                        TEntity entity = entityLookup[key].First();

                        mapping.Invoke(entity, model);

                        _entities.Add(entity);

                        _onUpdate?.Invoke(entity);
                    }
                    else
                    {
                        TEntity entity = new TEntity();

                        mapping.Invoke(entity, model);

                        _entities.Add(entity);

                        _onAdd?.Invoke(entity);
                    }
                }
            }
        }
    }
}
