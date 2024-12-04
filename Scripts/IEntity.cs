#nullable enable
namespace UniT.Entities
{
    public interface IEntity : IComponent
    {
    }

    public interface IEntityWithoutParams : IEntity
    {
    }

    public interface IEntityWithParams<in TParams> : IEntity
    {
        public TParams Params { set; }
    }
}