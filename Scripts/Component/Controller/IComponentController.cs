#nullable enable
namespace TheOne.Entities.Component.Controller
{
    using TheOne.Entities.Controller;

    public interface IComponentController : IController
    {
        public void OnInstantiate();

        public void OnSpawn();

        public void OnRecycle();
    }
}