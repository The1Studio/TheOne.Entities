#nullable enable
namespace TheOne.Entities
{
    using System.Diagnostics.CodeAnalysis;
    using TheOne.DI;
    using UnityEngine;
    #if THEONE_UNITASK
    using System.Threading;
    #else
    using System.Collections;
    using System.Collections.Generic;
    #endif

    public interface IComponent : IComponentLifecycle
    {
        public IDependencyContainer Container { set; }

        public IEntityManager Manager { get; set; }

        public IEntity Entity { get; set; }

        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }

        // ReSharper disable once InconsistentNaming
        public Transform transform { get; }

        #region Extensions

        #region Self

        public T? GetComponentOrDefault<T>();

        public T GetComponent<T>();

        public T[] GetComponents<T>();

        public bool HasComponent<T>();

        public bool TryGetComponent<T>([MaybeNullWhen(false)] out T component);

        #endregion

        #region Children

        public T? GetComponentInChildrenOrDefault<T>(bool includeInactive = false);

        public T GetComponentInChildren<T>(bool includeInactive = false);

        public T[] GetComponentsInChildren<T>(bool includeInactive = false);

        public bool HasComponentInChildren<T>(bool includeInactive = false);

        public bool TryGetComponentInChildren<T>([MaybeNullWhen(false)] out T component, bool includeInactive = false);

        #endregion

        #region Parent

        public T? GetComponentInParentOrDefault<T>(bool includeInactive = false);

        public T GetComponentInParent<T>(bool includeInactive = false);

        public T[] GetComponentsInParent<T>(bool includeInactive = false);

        public bool HasComponentInParent<T>(bool includeInactive = false);

        public bool TryGetComponentInParent<T>([MaybeNullWhen(false)] out T component, bool includeInactive = false);

        #endregion

        #region Async

        #if THEONE_UNITASK
        public CancellationToken GetCancellationTokenOnDisable();
        #else
        public void StartCoroutine(IEnumerator coroutine);

        public void StopCoroutine(IEnumerator coroutine);

        public IEnumerator GatherCoroutines(params IEnumerator[] coroutines);

        public IEnumerator GatherCoroutines(IEnumerable<IEnumerator> coroutines);
        #endif

        #endregion

        #endregion
    }
}