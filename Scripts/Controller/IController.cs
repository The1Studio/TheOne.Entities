#nullable enable
namespace TheOne.Entities.Controller
{
    public interface IController : IComponentLifecycle
    {
        public IComponent Component { set; }
    }
}