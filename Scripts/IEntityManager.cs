#nullable enable
namespace TheOne.Entities
{
    using System;
    using System.Collections.Generic;
    using TheOne.Extensions;
    using UnityEngine;
    #if THEONE_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public interface IEntityManager
    {
        public event Action<IEntity> Instantiated;

        public event Action<IEntity> Spawned;

        public event Action<IEntity> Recycled;

        public event Action<IEntity> CleanedUp;

        public void Load(IEntity prefab, int count = 1);

        public void Load(string key, int count = 1);

        public TEntity Spawn<TEntity>(TEntity prefab, Vector3 position = default, Quaternion rotation = default, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithoutParams;

        public TEntity Spawn<TEntity>(TEntity prefab, object @params, Vector3 position = default, Quaternion rotation = default, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithParams;

        public TEntity Spawn<TEntity>(string key, Vector3 position = default, Quaternion rotation = default, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithoutParams;

        public TEntity Spawn<TEntity>(string key, object @params, Vector3 position = default, Quaternion rotation = default, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithParams;

        public void Recycle(IEntity instance);

        public void RecycleAll(IEntity prefab);

        public void RecycleAll(string key);

        public void Cleanup(IEntity prefab, int retainCount = 1);

        public void Cleanup(string key, int retainCount = 1);

        public void Unload(IEntity prefab);

        public void Unload(string key);

        public IEnumerable<T> Query<T>();

        #region Implicit Key

        public void Load<TEntity>(int count = 1) where TEntity : IEntity => this.Load(typeof(TEntity).GetKey(), count);

        public TEntity Spawn<TEntity>(Vector3 position = default, Quaternion rotation = default, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithoutParams => this.Spawn<TEntity>(typeof(TEntity).GetKey(), position, rotation, parent, spawnInWorldSpace);

        public TEntity Spawn<TEntity>(object @params, Vector3 position = default, Quaternion rotation = default, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithParams => this.Spawn<TEntity>(typeof(TEntity).GetKey(), @params, position, rotation, parent, spawnInWorldSpace);

        public void RecycleAll<TEntity>() where TEntity : IEntity => this.RecycleAll(typeof(TEntity).GetKey());

        public void Cleanup<TEntity>(int retainCount = 1) where TEntity : IEntity => this.Cleanup(typeof(TEntity).GetKey(), retainCount);

        public void Unload<TEntity>() where TEntity : IEntity => this.Unload(typeof(TEntity).GetKey());

        #endregion

        #region Async

        #if THEONE_UNITASK
        public UniTask LoadAsync(string key, int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default);

        public UniTask LoadAsync<TEntity>(int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default) where TEntity : IEntity => this.LoadAsync(typeof(TEntity).GetKey(), count, progress, cancellationToken);
        #else
        public IEnumerator LoadAsync(string key, int count = 1, Action? callback = null, IProgress<float>? progress = null);

        public IEnumerator LoadAsync<TEntity>(int count = 1, Action? callback = null, IProgress<float>? progress = null) where TEntity : IEntity => this.LoadAsync(typeof(TEntity).GetKey(), count, callback, progress);
        #endif

        #endregion
    }
}