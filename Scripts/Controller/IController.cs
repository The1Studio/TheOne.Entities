#nullable enable
namespace UniT.Entities.Controller
{
    public interface IController : IComponentLifecycle
    {
        public IComponent Component { set; }
    }
}