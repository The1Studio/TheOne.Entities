#nullable enable
namespace UniT.Entities.Component.Controller
{
    using UniT.Entities.Controller;

    public abstract class ComponentController<TComponent> : Controller<TComponent>, IComponentController where TComponent : IComponent, IHasController
    {
        protected TComponent Component => this.Owner;

        protected IEntityManager Manager => this.Owner.Manager;

        public virtual void OnInstantiate() { }

        public virtual void OnSpawn() { }

        public virtual void OnRecycle() { }
    }
}