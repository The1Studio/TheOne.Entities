#if THEONE_DI
#nullable enable
namespace TheOne.Entities.DI
{
    using TheOne.DI;
    using TheOne.Logging.DI;
    using TheOne.Pooling.DI;

    public static class EntityManagerDI
    {
        public static void AddEntityManager(this DependencyContainer container)
        {
            if (container.Contains<IEntityManager>()) return;
            container.AddLoggerManager();
            container.AddObjectPoolManager();
            container.AddInterfaces<EntityManager>();
        }
    }
}
#endif