#nullable enable
namespace TheOne.Entities
{
    using System;
    using UnityEngine;

    [DisallowMultipleComponent]
    public abstract class BaseEntity : Component, IEntity
    {
        public void Recycle() => this.Manager.Recycle(this);
    }

    public class Entity : BaseEntity, IEntityWithoutParams
    {
    }

    public abstract class Entity<TParams> : BaseEntity, IEntityWithParams
    {
        object IEntityWithParams.Params
        {
            set => this.Params = value switch
            {
                null            => default!,
                TParams @params => @params,
                _               => throw new InvalidCastException($"{this.GetType().Name} expected {typeof(TParams)}, got {value.GetType().Name}"),
            };
        }

        protected TParams Params { get; private set; } = default!;
    }
}