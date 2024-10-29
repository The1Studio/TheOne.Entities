#nullable enable
namespace TheOne.Entities.Entity.Controller
{
    using TheOne.Entities.Component.Controller;
    using TheOne.Entities.Controller;

    public abstract class EntityController<TEntity> : ComponentController<TEntity>, IEntityController where TEntity : IEntity, IHasController
    {
    }
}