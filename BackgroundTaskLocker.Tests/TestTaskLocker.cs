using System;
using BackgroundTaskLocker.Application;
using BackgroundTaskLocker.Nh;
using Dapper;
using FluentAssertions;
using Xunit;

namespace BackgroundTaskLocker.Tests;

public class TestTaskLocker : IntegrationTest
{
    private readonly TimeSpan _backgroundTaskLockDuration = TimeSpan.FromMinutes(30);

    public TestTaskLocker()
    {
        CleanDatabase();
    }

    [Fact]
    public void It_should_acquire_free_task()
    {
        var taskLocker = ComposeSUT(serviceId: 1);

        var isAcquired = taskLocker.TryLock(BackgroundTask.UpdateUsersBlackList, _backgroundTaskLockDuration, out _);

        isAcquired.Should().BeTrue();
    }

    [Fact]
    public void Second_service_must_not_acquire_when_first_already_took()
    {
        // Arrange
        var firstTaskLocker  = ComposeSUT(serviceId: 1);
        var secondTaskLocker = ComposeSUT(serviceId: 2);
        firstTaskLocker.TryLock(BackgroundTask.UpdateUsersBlackList, _backgroundTaskLockDuration, out _);

        // Act
        var isAcquired = secondTaskLocker.TryLock(BackgroundTask.UpdateUsersBlackList, _backgroundTaskLockDuration, out _);

        // Assert
        isAcquired.Should().BeFalse();
    }

    [Fact]
    public void Second_service_must_acquire_when_first_released()
    {
        // Arrange
        var firstTaskLocker  = ComposeSUT(serviceId: 1);
        var secondTaskLocker = ComposeSUT(serviceId: 2);
        firstTaskLocker.TryLock(BackgroundTask.UpdateUsersBlackList, _backgroundTaskLockDuration, out var firstServiceLock);
        firstServiceLock!.Release();

        // Act
        var isAcquired = secondTaskLocker.TryLock(BackgroundTask.UpdateUsersBlackList, _backgroundTaskLockDuration, out _);

        // Assert
        isAcquired.Should().BeTrue();
    }

    private static TaskLocker ComposeSUT(long serviceId)
    {
        var configuration = new TaskLockerConfiguration
        {
            ServiceId = serviceId
        };
        return new TaskLocker(NhSessionFactory.Instance, configuration);
    }

    private static void CleanDatabase()
    {
        var connection  = NhSessionFactory.Instance.OpenSession().Connection;
        var transaction = connection.BeginTransaction();

        const string sql = @"
update public.background_tasks t
 set service_id = null,
 expire_date = null,
 is_locked = false,
 lock_timestamp = null
  where t.id = :id
";
        var parameters = new
        {
            id = BackgroundTask.UpdateUsersBlackList
        };

        connection.Execute(sql, parameters, transaction);

        transaction.Commit();
    }
}