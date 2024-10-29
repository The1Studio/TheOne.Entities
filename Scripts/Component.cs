#nullable enable
namespace TheOne.Entities
{
    using TheOne.DI;
    using TheOne.Extensions;

    public abstract class Component : BetterMonoBehavior, IComponent
    {
        IDependencyContainer IComponent.Container { set => this.Container = value; }

        IEntityManager IComponent.Manager { get => this.Manager; set => this.Manager = value; }

        IEntity IComponent.Entity { get => this.Entity; set => this.Entity = value; }

        protected IDependencyContainer Container { get; private set; } = null!;

        public IEntityManager Manager { get; private set; } = null!;

        public IEntity Entity { get; private set; } = null!;

        void IComponentLifecycle.OnInstantiate() => this.OnInstantiate();

        void IComponentLifecycle.OnSpawn() => this.OnSpawn();

        void IComponentLifecycle.OnRecycle() => this.OnRecycle();

        void IComponentLifecycle.OnCleanup() => this.OnCleanup();

        protected virtual void OnInstantiate() { }

        protected virtual void OnSpawn() { }

        protected virtual void OnRecycle() { }

        protected virtual void OnCleanup() { }
    }
}