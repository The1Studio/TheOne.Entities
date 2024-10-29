#nullable enable
namespace TheOne.Entities.Controller
{
    public interface IController
    {
        public IHasController Owner { set; }
    }
}