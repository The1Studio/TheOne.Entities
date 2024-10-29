#nullable enable
namespace TheOne.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TheOne.DI;
    using TheOne.Extensions;
    using TheOne.Logging;
    using TheOne.Pooling;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = TheOne.Logging.ILogger;
    #if THEONE_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public sealed class EntityManager : IEntityManager
    {
        #region Constructor

        private readonly IDependencyContainer container;
        private readonly IObjectPoolManager   objectPoolManager;
        private readonly ILogger              logger;

        private readonly Dictionary<IEntity, IReadOnlyList<IComponent>> entityToComponents      = new Dictionary<IEntity, IReadOnlyList<IComponent>>();
        private readonly Dictionary<IComponent, IReadOnlyList<Type>>    componentToTypes        = new Dictionary<IComponent, IReadOnlyList<Type>>();
        private readonly Dictionary<Type, HashSet<IComponent>>          typeToSpawnedComponents = new Dictionary<Type, HashSet<IComponent>>();
        private readonly Dictionary<IEntity, object>                    spawnedEntities         = new Dictionary<IEntity, object>();

        [Preserve]
        public EntityManager(IDependencyContainer container, IObjectPoolManager objectPoolManager, ILoggerManager loggerManager)
        {
            this.container                       =  container;
            this.objectPoolManager               =  objectPoolManager;
            this.objectPoolManager.OnInstantiate += this.OnInstantiate;
            this.objectPoolManager.OnCleanup     += this.OnCleanup;
            this.logger                          =  loggerManager.GetLogger(this);
            this.logger.Debug("Constructed");
        }

        #endregion

        #region Public

        void IEntityManager.Load(IEntity prefab, int count) => this.objectPoolManager.Load(prefab.gameObject, count);

        void IEntityManager.Load(string key, int count) => this.objectPoolManager.Load(key, count);

        #if THEONE_UNITASK
        UniTask IEntityManager.LoadAsync(string key, int count, IProgress<float>? progress, CancellationToken cancellationToken) => this.objectPoolManager.LoadAsync(key, count, progress, cancellationToken);
        #else
        IEnumerator IEntityManager.LoadAsync(string key, int count, Action? callback, IProgress<float>? progress) => this.objectPoolManager.LoadAsync(key, count, callback, progress);
        #endif

        TEntity IEntityManager.Spawn<TEntity>(TEntity prefab, Vector3 position, Quaternion rotation, Transform? parent, bool spawnInWorldSpace)
        {
            var entity = this.objectPoolManager.Spawn(prefab.gameObject, position, rotation, parent, spawnInWorldSpace).GetComponent<TEntity>();
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, prefab);
            return entity;
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(TEntity prefab, TParams @params, Vector3 position, Quaternion rotation, Transform? parent, bool spawnInWorldSpace)
        {
            var entity = this.objectPoolManager.Spawn(prefab.gameObject, position, rotation, parent, spawnInWorldSpace).GetComponent<TEntity>();
            entity.Params = @params;
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, prefab);
            return entity;
        }

        TEntity IEntityManager.Spawn<TEntity>(string key, Vector3 position, Quaternion rotation, Transform? parent, bool spawnInWorldSpace)
        {
            var entity = this.objectPoolManager.Spawn(key, position, rotation, parent, spawnInWorldSpace).GetComponentOrThrow<TEntity>();
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, key);
            return entity;
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(string key, TParams @params, Vector3 position, Quaternion rotation, Transform? parent, bool spawnInWorldSpace)
        {
            var entity = this.objectPoolManager.Spawn(key, position, rotation, parent, spawnInWorldSpace).GetComponentOrThrow<TEntity>();
            entity.Params = @params;
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, key);
            return entity;
        }

        void IEntityManager.Recycle(IEntity entity)
        {
            if (!this.spawnedEntities.Remove(entity)) throw new InvalidOperationException($"{entity.gameObject.name} was not spawned from {nameof(EntityManager)}");
            this.OnRecycle(entity);
            this.objectPoolManager.Recycle(entity.gameObject);
        }

        void IEntityManager.RecycleAll(IEntity prefab)
        {
            this.OnRecycleAll(prefab);
            this.objectPoolManager.RecycleAll(prefab.gameObject);
        }

        void IEntityManager.RecycleAll(string key)
        {
            this.OnRecycleAll(key);
            this.objectPoolManager.RecycleAll(key);
        }

        void IEntityManager.Cleanup(IEntity prefab, int retainCount)
        {
            this.objectPoolManager.Cleanup(prefab.gameObject, retainCount);
        }

        void IEntityManager.Cleanup(string key, int retainCount)
        {
            this.objectPoolManager.Cleanup(key, retainCount);
        }

        void IEntityManager.Unload(IEntity prefab)
        {
            this.OnRecycleAll(prefab);
            this.objectPoolManager.Unload(prefab.gameObject);
        }

        void IEntityManager.Unload(string key)
        {
            this.OnRecycleAll(key);
            this.objectPoolManager.Unload(key);
        }

        IEnumerable<T> IEntityManager.Query<T>()
        {
            return this.typeToSpawnedComponents.GetOrDefault(typeof(T))?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        #endregion

        #region Private

        private void OnInstantiate(GameObject instance)
        {
            if (!instance.TryGetComponent<IEntity>(out var entity)) return;
            var components = entity.GetComponentsInChildren<IComponent>();
            this.entityToComponents.Add(entity, components);
            components.ForEach(component =>
            {
                this.componentToTypes.Add(
                    component,
                    component.GetType()
                        .GetInterfaces()
                        .Prepend(component.GetType())
                        .ToArray()
                );
                component.Container = this.container;
                component.Manager   = this;
                component.Entity    = entity;
            });
            components.ForEach(component => component.OnInstantiate());
        }

        private void OnSpawn(IEntity entity)
        {
            this.entityToComponents[entity].ForEach(component => this.componentToTypes[component].ForEach(type => this.typeToSpawnedComponents.GetOrAdd(type).Add(component)));
            this.entityToComponents[entity].ForEach(component => component.OnSpawn());
        }

        private void OnRecycle(IEntity entity)
        {
            this.entityToComponents[entity].ForEach(component => this.componentToTypes[component].ForEach(type => this.typeToSpawnedComponents[type].Remove(component)));
            this.entityToComponents[entity].ForEach(component => component.OnRecycle());
        }

        private void OnRecycleAll(object obj)
        {
            this.spawnedEntities.RemoveWhere((entity, key) =>
            {
                if (!key.Equals(obj)) return false;
                this.OnRecycle(entity);
                return true;
            });
        }

        private void OnCleanup(GameObject instance)
        {
            if (!instance.TryGetComponent<IEntity>(out var entity)) return;
            this.entityToComponents.Remove(entity, out var components);
            this.componentToTypes.RemoveRange(components);
            components.ForEach(component => component.OnCleanup());
        }

        #endregion
    }
}