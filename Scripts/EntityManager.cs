#nullable enable
namespace TheOne.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TheOne.DI;
    using TheOne.Entities.Component;
    using TheOne.Entities.Controller;
    using TheOne.Entities.Entity;
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

        private readonly Dictionary<IEntity, IReadOnlyList<IComponent>> entities         = new Dictionary<IEntity, IReadOnlyList<IComponent>>();
        private readonly Dictionary<IComponent, IReadOnlyList<Type>>    componentToTypes = new Dictionary<IComponent, IReadOnlyList<Type>>();
        private readonly Dictionary<Type, HashSet<IComponent>>          typeToComponents = new Dictionary<Type, HashSet<IComponent>>();
        private readonly Dictionary<IEntity, object>                    spawnedEntities  = new Dictionary<IEntity, object>();

        [Preserve]
        public EntityManager(IDependencyContainer container, IObjectPoolManager objectPoolManager, ILoggerManager loggerManager)
        {
            this.container                       =  container;
            this.objectPoolManager               =  objectPoolManager;
            this.objectPoolManager.OnInstantiate += this.OnInstantiate;
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

        TEntity IEntityManager.Spawn<TEntity>(TEntity prefab, Vector3 position, Quaternion rotation, Transform? parent, bool worldPositionStays)
        {
            var entity = this.objectPoolManager.Spawn(prefab.gameObject, position, rotation, parent, worldPositionStays).GetComponent<TEntity>();
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, prefab);
            return entity;
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(TEntity prefab, TParams @params, Vector3 position, Quaternion rotation, Transform? parent, bool worldPositionStays)
        {
            var entity = this.objectPoolManager.Spawn(prefab.gameObject, position, rotation, parent, worldPositionStays).GetComponent<TEntity>();
            entity.Params = @params;
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, prefab);
            return entity;
        }

        TEntity IEntityManager.Spawn<TEntity>(string key, Vector3 position, Quaternion rotation, Transform? parent, bool worldPositionStays)
        {
            var entity = this.objectPoolManager.Spawn(key, position, rotation, parent, worldPositionStays).GetComponentOrThrow<TEntity>();
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, key);
            return entity;
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(string key, TParams @params, Vector3 position, Quaternion rotation, Transform? parent, bool worldPositionStays)
        {
            var entity = this.objectPoolManager.Spawn(key, position, rotation, parent, worldPositionStays).GetComponentOrThrow<TEntity>();
            entity.Params = @params;
            this.OnSpawn(entity);
            this.spawnedEntities.Add(entity, key);
            return entity;
        }

        void IEntityManager.Recycle(IEntity entity)
        {
            this.spawnedEntities.Remove(entity);
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
            this.OnCleanup();
        }

        void IEntityManager.Cleanup(string key, int retainCount)
        {
            this.objectPoolManager.Cleanup(key, retainCount);
            this.OnCleanup();
        }

        void IEntityManager.Unload(IEntity prefab)
        {
            this.OnRecycleAll(prefab);
            this.objectPoolManager.RecycleAll(prefab.gameObject);
            this.objectPoolManager.Unload(prefab.gameObject);
            this.OnCleanup();
        }

        void IEntityManager.Unload(string key)
        {
            this.OnRecycleAll(key);
            this.objectPoolManager.RecycleAll(key);
            this.objectPoolManager.Unload(key);
            this.OnCleanup();
        }

        IEnumerable<T> IEntityManager.Query<T>()
        {
            return this.typeToComponents.GetOrDefault(typeof(T))?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        #endregion

        #region Private

        private void OnInstantiate(GameObject instance)
        {
            if (!instance.TryGetComponent<IEntity>(out var entity)) return;
            var components = entity.GetComponentsInChildren<IComponent>();
            this.entities.Add(entity, components);
            components.ForEach(component =>
            {
                this.componentToTypes.Add(
                    component,
                    component.GetType()
                        .GetInterfaces()
                        .Prepend(component.GetType())
                        .ToArray()
                );
                component.Manager = this;
                component.Entity  = entity;
                if (component is IHasController owner)
                {
                    var controller = (IController)this.container.Instantiate(owner.ControllerType);
                    controller.Owner = owner;
                    owner.Controller = controller;
                }
            });
            components.ForEach(component => component.OnInstantiate());
        }

        private void OnSpawn(IEntity entity)
        {
            this.entities[entity].ForEach(component => this.componentToTypes[component].ForEach(type => this.typeToComponents.GetOrAdd(type).Add(component)));
            this.entities[entity].ForEach(component => component.OnSpawn());
        }

        private void OnRecycle(IEntity entity)
        {
            this.entities[entity].ForEach(component => this.componentToTypes[component].ForEach(type => this.typeToComponents[type].Remove(component)));
            this.entities[entity].ForEach(component => component.OnRecycle());
        }

        private void OnRecycleAll(object obj)
        {
            this.spawnedEntities.RemoveWhere((entity, key) =>
            {
                if (key != obj) return false;
                this.OnRecycle(entity);
                return true;
            });
        }

        private void OnCleanup()
        {
            this.entities.RemoveWhere((entity, components) =>
            {
                if (entity.gameObject) return false;
                components.ForEach(component => this.componentToTypes.Remove(component));
                return true;
            });
        }

        #endregion
    }
}