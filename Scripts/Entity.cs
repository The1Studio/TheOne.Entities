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

    public abstract class Entity<TParams> : BaseEntity, IEntityWithParams<TParams> where TParams : notnull
    {
        TParams? IEntityWithParams<TParams>.Params { set => this.@params = value; }

        private TParams? @params;

        protected TParams Params => this.@params ?? throw new InvalidOperationException($"{this.name} not spawned or already recycled");
    }
}