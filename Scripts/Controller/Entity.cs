#nullable enable
namespace TheOne.Entities.Controller
{
    using System;

    public abstract class Entity<TController> : Entity, IComponentWithController where TController : IController
    {
        protected virtual Type ControllerType => typeof(TController);

        protected TController Controller { get; private set; } = default!;

        void IComponentLifecycle.OnInstantiate()
        {
            var controller = (TController)this.Container.Instantiate(this.ControllerType);
            controller.Component = this;
            this.Controller      = controller;
            this.OnInstantiate();
            this.Controller.OnInstantiate();
        }

        void IComponentLifecycle.OnSpawn()
        {
            this.OnSpawn();
            this.Controller.OnSpawn();
        }

        void IComponentLifecycle.OnRecycle()
        {
            this.OnRecycle();
            this.Controller.OnRecycle();
        }

        void IComponentLifecycle.OnCleanup()
        {
            this.OnCleanup();
            this.Controller.OnCleanup();
        }
    }

    public abstract class Entity<TParams, TController> : Entities.Entity<TParams>, IComponentWithController where TController : IController where TParams : notnull
    {
        protected virtual Type ControllerType => typeof(TController);

        protected TController Controller { get; private set; } = default!;

        void IComponentLifecycle.OnInstantiate()
        {
            var controller = (TController)this.Container.Instantiate(this.ControllerType);
            controller.Component = this;
            this.Controller      = controller;
            this.OnInstantiate();
            this.Controller.OnInstantiate();
        }

        void IComponentLifecycle.OnSpawn()
        {
            this.OnSpawn();
            this.Controller.OnSpawn();
        }

        void IComponentLifecycle.OnRecycle()
        {
            this.OnRecycle();
            this.Controller.OnRecycle();
        }

        void IComponentLifecycle.OnCleanup()
        {
            this.OnCleanup();
            this.Controller.OnCleanup();
        }
    }
}