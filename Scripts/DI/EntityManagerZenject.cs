#if THEONE_ZENJECT
#nullable enable
namespace TheOne.Entities.DI
{
    using TheOne.DI;
    using TheOne.Logging.DI;
    using TheOne.Pooling.DI;
    using Zenject;

    public static class EntityManagerZenject
    {
        public static void BindEntityManager(this DiContainer container)
        {
            if (container.HasBinding<IEntityManager>()) return;
            container.BindDependencyContainer();
            container.BindLoggerManager();
            container.BindObjectPoolManager();
            container.BindInterfacesTo<EntityManager>().AsSingle();
        }
    }
}
#endif