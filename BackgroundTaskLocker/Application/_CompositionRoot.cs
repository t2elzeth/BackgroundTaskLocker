using Autofac;

namespace BackgroundTaskLocker.Application;

[UsedImplicitly]
public class CompositionRoot : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TaskLocker>().SingleInstance();
    }
}