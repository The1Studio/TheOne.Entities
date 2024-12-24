﻿#nullable enable
namespace UniT.Entities.Controller
{
    using System;

    public abstract class Component<TController> : Component, IComponentLifecycle where TController : IController
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