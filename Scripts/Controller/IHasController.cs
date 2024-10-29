#nullable enable
namespace TheOne.Entities.Controller
{
    using System;

    public interface IHasController
    {
        public Type ControllerType { get; }

        public IController Controller { set; }
    }
}