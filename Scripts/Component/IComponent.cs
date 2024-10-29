#nullable enable
namespace TheOne.Entities.Component
{
    using System.Diagnostics.CodeAnalysis;
    using TheOne.Entities.Entity;
    using UnityEngine;

    public interface IComponent
    {
        public IEntityManager Manager { get; set; }

        public IEntity Entity { get; set; }

        public void OnInstantiate();

        public void OnSpawn();

        public void OnRecycle();

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

        #endregion
    }
}