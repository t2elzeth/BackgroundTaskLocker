using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Humanizer;

namespace BackgroundTaskLocker.Nh.Conventions;

public class PropertyConvention : IPropertyConvention
{
    public void Apply(IPropertyInstance instance)
    {
        instance.Column(instance.Name.Underscore());
    }
}