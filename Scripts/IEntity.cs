#nullable enable
namespace TheOne.Entities
{
    using System;

    public interface IEntity : IComponent
    {
    }

    public interface IEntityWithoutParams : IEntity
    {
    }

    public interface IEntityWithParams : IEntity
    {
        public object? Params { set; }
    }

    public interface IEntityWithParams<in TParams> : IEntityWithParams where TParams : notnull
    {
        object? IEntityWithParams.Params
        {
            set => this.Params = value switch
            {
                null            => default,
                TParams @params => @params,
                _               => throw new InvalidCastException($"{this.GetType().Name} expected params of type {typeof(TParams)}, got {value.GetType().Name}"),
            };
        }

        public new TParams? Params { set; }
    }
}