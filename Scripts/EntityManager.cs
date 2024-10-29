#nullable enable
namespace TheOne.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TheOne.DI;
    using TheOne.Extensions;
    using TheOne.Pooling;
    using UnityEngine;
    using UnityEngine.Scripting;
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

        private readonly Dictionary<GameObject, IEntity>                objToEntity             = new Dictionary<GameObject, IEntity>();
        private readonly Dictionary<IEntity, IReadOnlyList<IComponent>> entityToComponents      = new Dictionary<IEntity, IReadOnlyList<IComponent>>();
        private readonly Dictionary<IComponent, IReadOnlyList<Type>>    componentToTypes        = new Dictionary<IComponent, IReadOnlyList<Type>>();
        private readonly Dictionary<Type, HashSet<IComponent>>          typeToSpawnedComponents = new Dictionary<Type, HashSet<IComponent>>();

        [Preserve]
        public EntityManager(IDependencyContainer container, IObjectPoolManager objectPoolManager)
        {
            this.container                      =  container;
            this.objectPoolManager              =  objectPoolManager;
            this.objectPoolManager.Instantiated += this.OnInstantiated;
            this.objectPoolManager.Spawned      += this.OnSpawned;
            this.objectPoolManager.Recycled     += this.OnRecycled;
            this.objectPoolManager.CleanedUp    += this.OnCleanedUp;
        }

        #endregion

        #region Public

        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Instantiated { add => this.instantiated += value; remove => this.instantiated -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Spawned      { add => this.spawned += value;      remove => this.spawned -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Recycled     { add => this.recycled += value;     remove => this.recycled -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.CleanedUp    { add => this.cleanedUp += value;    remove => this.cleanedUp -= value; }

        void IEntityManager.Load(IEntity prefab, int count) => this.objectPoolManager.Load(prefab.gameObject, count);

        void IEntityManager.Load(object key, int count) => this.objectPoolManager.Load(key, count);

        #if THEONE_UNITASK
        UniTask IEntityManager.LoadAsync(object key, int count, IProgress<float>? progress, CancellationToken cancellationToken) => this.objectPoolManager.LoadAsync(key, count, progress, cancellationToken);
        #else
        IEnumerator IEntityManager.LoadAsync(object key, int count, Action? callback, IProgress<float>? progress) => this.objectPoolManager.LoadAsync(key, count, callback, progress);
        #endif

        TEntity IEntityManager.Spawn<TEntity>(TEntity prefab, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            return this.objectPoolManager.Spawn(prefab.gameObject, position, rotation, parent, spawnInWorldSpace).GetComponent<TEntity>();
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(TEntity prefab, TParams @params, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            this.nextParams = @params;
            return this.objectPoolManager.Spawn(prefab.gameObject, position, rotation, parent, spawnInWorldSpace).GetComponent<TEntity>();
        }

        TEntity IEntityManager.Spawn<TEntity>(object key, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            return this.objectPoolManager.Spawn(key, position, rotation, parent, spawnInWorldSpace).GetComponentOrThrow<TEntity>();
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(object key, TParams @params, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            this.nextParams = @params;
            return this.objectPoolManager.Spawn(key, position, rotation, parent, spawnInWorldSpace).GetComponentOrThrow<TEntity>();
        }

        void IEntityManager.Recycle(IEntity entity)
        {
            this.objectPoolManager.Recycle(entity.gameObject);
        }

        void IEntityManager.RecycleAll(IEntity prefab)
        {
            this.objectPoolManager.RecycleAll(prefab.gameObject);
        }

        void IEntityManager.RecycleAll(object key)
        {
            this.objectPoolManager.RecycleAll(key);
        }

        void IEntityManager.Cleanup(IEntity prefab, int retainCount)
        {
            this.objectPoolManager.Cleanup(prefab.gameObject, retainCount);
        }

        void IEntityManager.Cleanup(object key, int retainCount)
        {
            this.objectPoolManager.Cleanup(key, retainCount);
        }

        void IEntityManager.Unload(IEntity prefab)
        {
            this.objectPoolManager.Unload(prefab.gameObject);
        }

        void IEntityManager.Unload(object key)
        {
            this.objectPoolManager.Unload(key);
        }

        IEnumerable<T> IEntityManager.Query<T>()
        {
            return this.typeToSpawnedComponents.GetOrDefault(typeof(T))?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        #endregion

        #region Private

        private Action<IEntity, IReadOnlyList<IComponent>>? instantiated;
        private Action<IEntity, IReadOnlyList<IComponent>>? spawned;
        private Action<IEntity, IReadOnlyList<IComponent>>? recycled;
        private Action<IEntity, IReadOnlyList<IComponent>>? cleanedUp;

        private object nextParams = null!;

        private void OnInstantiated(GameObject instance)
        {
            if (!instance.TryGetComponent<IEntity>(out var entity)) return;
            this.objToEntity.Add(instance, entity);
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
            this.instantiated?.Invoke(entity, components);
        }

        private void OnSpawned(GameObject instance)
        {
            if (!this.objToEntity.TryGetValue(instance, out var entity)) return;
            if (entity is IEntityWithParams entityWithParams)
            {
                entityWithParams.Params = this.nextParams;
            }
            var components = this.entityToComponents[entity];
            components.ForEach(component => this.componentToTypes[component].ForEach(type => this.typeToSpawnedComponents.GetOrAdd(type).Add(component)));
            components.ForEach(component => component.OnSpawn());
            this.spawned?.Invoke(entity, components);
        }

        private void OnRecycled(GameObject instance)
        {
            if (!this.objToEntity.TryGetValue(instance, out var entity)) return;
            var components = this.entityToComponents[entity];
            components.ForEach(component => this.componentToTypes[component].ForEach(type => this.typeToSpawnedComponents[type].Remove(component)));
            components.ForEach(component => component.OnRecycle());
            if (entity is IEntityWithParams entityWithParams)
            {
                entityWithParams.Params = null;
            }
            this.recycled?.Invoke(entity, components);
        }

        private void OnCleanedUp(GameObject instance)
        {
            if (!this.objToEntity.Remove(instance, out var entity)) return;
            this.entityToComponents.Remove(entity, out var components);
            this.componentToTypes.RemoveRange(components);
            components.ForEach(component => component.OnCleanup());
            this.cleanedUp?.Invoke(entity, components);
        }

        #endregion
    }
}