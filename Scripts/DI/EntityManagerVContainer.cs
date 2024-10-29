#if THEONE_VCONTAINER
#nullable enable
namespace TheOne.Entities.DI
{
    using TheOne.DI;
    using TheOne.Logging.DI;
    using TheOne.Pooling.DI;
    using VContainer;

    public static class EntityManagerVContainer
    {
        public static void RegisterEntityManager(this IContainerBuilder builder)
        {
            if (builder.Exists(typeof(IEntityManager), true)) return;
            builder.RegisterDependencyContainer();
            builder.RegisterLoggerManager();
            builder.RegisterObjectPoolManager();
            builder.Register<EntityManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
#endif