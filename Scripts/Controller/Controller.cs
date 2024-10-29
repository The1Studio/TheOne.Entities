#nullable enable
namespace TheOne.Entities.Controller
{
    using System.Diagnostics.CodeAnalysis;
    #if THEONE_UNITASK
    using System.Threading;
    #else
    using System.Collections;
    using System.Collections.Generic;
    #endif

    public abstract class Controller<TComponent> : IController where TComponent : IComponentWithController
    {
        IComponent IController.Component { set => this.Component = (TComponent)value; }

        protected TComponent Component { get; private set; } = default!;

        protected IEntityManager Manager => this.Component.Manager;

        protected IEntity Entity => this.Component.Entity;

        void IComponentLifecycle.OnInstantiate() => this.OnInstantiate();

        void IComponentLifecycle.OnSpawn() => this.OnSpawn();

        void IComponentLifecycle.OnRecycle() => this.OnRecycle();

        void IComponentLifecycle.OnCleanup() => this.OnCleanup();

        protected virtual void OnInstantiate() { }

        protected virtual void OnSpawn() { }

        protected virtual void OnRecycle() { }

        protected virtual void OnCleanup() { }

        #region Extensions

        #region Self

        protected T? GetComponentOrDefault<T>() => this.Component.GetComponentOrDefault<T>();

        protected T GetComponent<T>() => this.Component.GetComponent<T>();

        protected T[] GetComponents<T>() => this.Component.GetComponents<T>();

        protected bool HasComponent<T>() => this.Component.HasComponent<T>();

        protected bool TryGetComponent<T>([MaybeNullWhen(false)] out T component) => this.Component.TryGetComponent(out component);

        #endregion

        #region Children

        protected T? GetComponentInChildrenOrDefault<T>(bool includeInactive = false) => this.Component.GetComponentInChildrenOrDefault<T>(includeInactive);

        protected T GetComponentInChildren<T>(bool includeInactive = false) => this.Component.GetComponentInChildren<T>(includeInactive);

        protected T[] GetComponentsInChildren<T>(bool includeInactive = false) => this.Component.GetComponentsInChildren<T>(includeInactive);

        protected bool HasComponentInChildren<T>(bool includeInactive = false) => this.Component.HasComponentInChildren<T>(includeInactive);

        protected bool TryGetComponentInChildren<T>([MaybeNullWhen(false)] out T component, bool includeInactive = false) => this.Component.TryGetComponentInChildren(out component, includeInactive);

        #endregion

        #region Parent

        protected T? GetComponentInParentOrDefault<T>(bool includeInactive = false) => this.Component.GetComponentInParentOrDefault<T>(includeInactive);

        protected T GetComponentInParent<T>(bool includeInactive = false) => this.Component.GetComponentInParent<T>(includeInactive);

        protected T[] GetComponentsInParent<T>(bool includeInactive = false) => this.Component.GetComponentsInParent<T>(includeInactive);

        protected bool HasComponentInParent<T>(bool includeInactive = false) => this.Component.HasComponentInParent<T>(includeInactive);

        protected bool TryGetComponentInParent<T>([MaybeNullWhen(false)] out T component, bool includeInactive = false) => this.Component.TryGetComponentInParent(out component, includeInactive);

        #endregion

        #region Async

        #if THEONE_UNITASK
        protected CancellationToken GetCancellationTokenOnDisable() => this.Component.GetCancellationTokenOnDisable();
        #else
        protected void StartCoroutine(IEnumerator coroutine) => this.Component.StartCoroutine(coroutine);

        protected void StopCoroutine(IEnumerator coroutine) => this.Component.StopCoroutine(coroutine);

        protected IEnumerator GatherCoroutines(params IEnumerator[] coroutines) => this.Component.GatherCoroutines(coroutines);

        protected IEnumerator GatherCoroutines(IEnumerable<IEnumerator> coroutines) => this.Component.GatherCoroutines(coroutines);
        #endif

        #endregion

        #endregion
    }
}