using System.Reflection;
using BackgroundTaskLocker.Nh.Conventions;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Context;
using NHibernate.Dialect;
using Configuration = NHibernate.Cfg.Configuration;

namespace BackgroundTaskLocker.Nh;

public class SessionFactoryBuilder
{
    private Type? _currentSessionContext;

    private string _connectionStringName = ConnectionStringsManager.DefaultConnectionStringName;
    private string? _connectionString;

    private string? _exportMappingsTo;
    private Action<Configuration>? _nhConfig;
    private IInterceptor? _interceptor;

    private readonly IList<Assembly> _fluentMappings = new List<Assembly>();
    private readonly IList<Assembly> _hbmMappings = new List<Assembly>();
    private readonly IList<IConvention> _conventions = new List<IConvention>();

    public SessionFactoryBuilder AddFluentMappingsFrom(string assemblyName)
    {
        _fluentMappings.Add(Assembly.Load(assemblyName));

        return this;
    }

    public SessionFactoryBuilder AddFluentMappingsFrom(Assembly assembly)
    {
        _fluentMappings.Add(assembly);

        return this;
    }

    public SessionFactoryBuilder AddHbmMappingsFrom(string assemblyName)
    {
        _hbmMappings.Add(Assembly.Load(assemblyName));

        return this;
    }

    public SessionFactoryBuilder ExportMappingsTo(string path)
    {
        _exportMappingsTo = path;

        return this;
    }

    public SessionFactoryBuilder CurrentSessionContext<TSessionContext>()
        where TSessionContext : ICurrentSessionContext
    {
        _currentSessionContext = typeof(TSessionContext);
        return this;
    }

    public SessionFactoryBuilder SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public SessionFactoryBuilder SetConnectionStringName(string connectionStringName)
    {
        _connectionStringName = connectionStringName;
        return this;
    }

    public SessionFactoryBuilder Use(IConvention convention)
    {
        _conventions.Add(convention);
        return this;
    }

    public SessionFactoryBuilder ExposeConfiguration(Action<Configuration> configuration)
    {
        _nhConfig = configuration;
        return this;
    }

    public SessionFactoryBuilder SetInterceptor(IInterceptor interceptor)
    {
        _interceptor = interceptor;
        return this;
    }

    public ISessionFactory Build()
    {
        var postgreSqlConfiguration = PostgreSQLConfiguration.PostgreSQL82.Dialect<PostgreSQL83Dialect>();

        var connectionString = _connectionString ?? ConnectionStringsManager.Get(_connectionStringName);

        postgreSqlConfiguration.ConnectionString(connectionString);

        var configuration = Fluently.Configure()
                                    .Database(postgreSqlConfiguration)
                                    .Mappings(m =>
                                    {
                                        foreach (var fluentMapping in _fluentMappings)
                                            m.FluentMappings.AddFromAssembly(fluentMapping);

                                        foreach (var hbmMapping in _hbmMappings)
                                            m.HbmMappings.AddFromAssembly(hbmMapping);

                                        if (!string.IsNullOrEmpty(_exportMappingsTo))
                                        {
                                            m.FluentMappings.ExportTo(_exportMappingsTo);
                                            m.AutoMappings.ExportTo(_exportMappingsTo);
                                        }

                                        m.FluentMappings.Conventions.Add(new IdConvention(),
                                                                         new PropertyConvention(),
                                                                         new ReferenceConvention(),
                                                                         new TableNameConvention(),
                                                                         new EnumConvention(),
                                                                         new HasManyConvention(),
                                                                         DefaultAccess.Property()
                                                                        );

                                        foreach (var convention in _conventions)
                                            m.FluentMappings.Conventions.Add(convention);
                                    });

        if (_interceptor != null)
        {
            configuration.ExposeConfiguration(config => config.SetInterceptor(_interceptor));
        }

        if (_nhConfig != null)
            configuration.ExposeConfiguration(_nhConfig);

        if (_currentSessionContext != null)
            configuration.CurrentSessionContext(_currentSessionContext.AssemblyQualifiedName);

        var sessionFactory = configuration.BuildSessionFactory();

        return sessionFactory;
    }
}