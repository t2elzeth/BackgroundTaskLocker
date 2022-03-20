using NHibernate;

namespace BackgroundTaskLocker.Nh;

public static class NHibernateManager
{
    private static ISessionFactory? _sessionFactory;

    public static ISessionFactory SessionFactory
    {
        get => _sessionFactory ?? throw new InvalidOperationException("SessionFactory is not initialized");
        set => _sessionFactory = value ?? throw new ArgumentNullException(nameof(value));
    }
}