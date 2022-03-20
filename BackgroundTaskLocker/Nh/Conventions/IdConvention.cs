using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Humanizer;

namespace BackgroundTaskLocker.Nh.Conventions;

public class IdConvention : IIdConvention
{
    public void Apply(IIdentityInstance instance)
    {
        if (string.IsNullOrEmpty(instance.GeneratedBy.Class))
        {
            var tableName  = instance.EntityType.Name.Pluralize().Underscore();
            var columnName = instance.Name.Underscore();

            instance.GeneratedBy.Native($"{tableName}_{columnName}_seq");

            instance.UnsavedValue("0");
        }

        instance.Column(instance.Name.Underscore());
    }
}