using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Commons.Logging.Serilog;
using Serilog;
using BackgroundTaskLocker.Application;
using BackgroundTaskLocker.Logging;
using BackgroundTaskLocker.Nh;
using BackgroundTaskLocker.Workers;
using ILoggerFactory = Commons.Logging.ILoggerFactory;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(ConfigureServices)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
    .Build();

await host.RunAsync();

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    ConnectionStringsManager.ReadFromConfiguration(context.Configuration);
    LoggerFactory.Instance = CreateLoggerFactory(context.Configuration);

    services.AddHostedService<FirstWorker>();
    // services.AddHostedService<SecondWorker>();
}

void ConfigureContainer(HostBuilderContext context, ContainerBuilder containerBuilder)
{
    containerBuilder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

    containerBuilder.RegisterInstance(new TaskLockerConfiguration
    {
        ServiceId = 2502050
    }).SingleInstance();

    containerBuilder.RegisterInstance(NhSessionFactory.Instance);
}

ILoggerFactory CreateLoggerFactory(IConfiguration configuration)
{
    var loggerConfiguration = new LoggerConfiguration()
        .WithDefaults()
        .ReadFrom.Configuration(configuration);

    return new SerilogLoggerFactory(loggerConfiguration);
}