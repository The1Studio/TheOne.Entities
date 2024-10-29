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

        void IComponent.OnInstantiate() => this.OnInstantiate();

        void IComponent.OnSpawn() => this.OnSpawn();

        void IComponent.OnRecycle() => this.OnRecycle();

        protected virtual void OnInstantiate() { }

        protected virtual void OnSpawn() { }

        protected virtual void OnRecycle() { }
    }
}