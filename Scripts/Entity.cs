#nullable enable
namespace TheOne.Entities
{
    using UnityEngine;

    [DisallowMultipleComponent]
    public abstract class BaseEntity : Component, IEntity
    {
        public void Recycle() => this.Manager.Recycle(this);
    }

    public abstract class Entity : BaseEntity, IEntityWithoutParams
    {
    }

    public abstract class Entity<TParams> : BaseEntity, IEntityWithParams<TParams>
    {
        TParams IEntityWithParams<TParams>.Params { set => this.Params = value; }

        public TParams Params { get; private set; } = default!;
    }
}