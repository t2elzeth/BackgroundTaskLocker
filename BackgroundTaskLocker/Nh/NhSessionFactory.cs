using NHibernate;
using NHibernate.Context;
using Environment = NHibernate.Cfg.Environment;

namespace BackgroundTaskLocker.Nh;

public static class NhSessionFactory
{
    public static ISessionFactory Instance { get; }

    static NhSessionFactory()
    {
        Instance = new SessionFactoryBuilder()
            .CurrentSessionContext<AsyncLocalSessionContext>()
            .AddFluentMappingsFrom("BackgroundTaskLocker")
            .ExposeConfiguration(cfg => cfg.SetProperty(Environment.Hbm2ddlKeyWords, "none"))
            .Build();
    }
}