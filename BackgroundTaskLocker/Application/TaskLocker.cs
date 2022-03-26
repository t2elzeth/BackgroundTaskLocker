using System.Diagnostics.CodeAnalysis;
using Dapper;
using NHibernate;

namespace BackgroundTaskLocker.Application;

public enum BackgroundTask
{
    UpdateUsersBlackList = 1
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TaskLockerConfiguration
{
    public long ServiceId { get; set; }
}

public class TaskLocker
{
    private readonly ISessionFactory _sessionFactory;
    private readonly TaskLockerConfiguration _configuration;

    public TaskLocker(ISessionFactory sessionFactory, TaskLockerConfiguration configuration)
    {
        _sessionFactory = sessionFactory;
        _configuration  = configuration;
    }

    public bool TryLock(BackgroundTask taskId, TimeSpan lockTtl, [MaybeNullWhen(false)] out TaskLock taskLock)
    {
        var connection  = _sessionFactory.OpenSession().Connection;
        var transaction = connection.BeginTransaction();

        var now           = DateTime.UtcNow;
        var expireDateUtc = now + lockTtl;

        const string sql = @"
update public.background_tasks t
set service_id = :serviceId,
expire_date = :expireDateUTC,
is_locked = true,
lock_timestamp = :now,
is_done = false
    where t.id = :id
    and (service_id = :serviceId or :now > expire_date or service_id is null)
    and t.is_done = true
";
        var parameters = new
        {
            now,
            id            = taskId,
            serviceId     = _configuration.ServiceId,
            expireDateUTC = expireDateUtc
        };

        var rowCount = connection.Execute(sql, parameters, transaction);

        //Берем блокировку если:
        /*
            *serviceId = null
            * блокировка протухла
            * serviceId = текущему сервису
        */

        transaction.Commit();

        var lockAcquired = rowCount > 0;

        if (lockAcquired)
        {
            taskLock = new TaskLock(taskId, _sessionFactory);
            return true;
        }

        taskLock = null;
        return false;
    }
}

public class TaskLock
{
    private readonly BackgroundTask _taskId;
    private readonly ISessionFactory _sessionFactory;

    public TaskLock(BackgroundTask taskId, ISessionFactory sessionFactory)
    {
        _taskId         = taskId;
        _sessionFactory = sessionFactory;
    }

    public void Release()
    {
        var connection  = _sessionFactory.OpenSession().Connection;
        var transaction = connection.BeginTransaction();

        const string sql = @"
update public.background_tasks t
        set service_id = null,
        expire_date = null,
        is_locked = false,
        is_done = true,
        lock_timestamp = null
            where t.id = :id
";
        var parameters = new
        {
            id = _taskId
        };

        connection.Execute(sql, parameters, transaction);

        transaction.Commit();
    }
}