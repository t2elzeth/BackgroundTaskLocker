using BackgroundTaskLocker.Application;
using NHibernate;

namespace BackgroundTaskLocker.Workers;

public class FirstWorker : BackgroundService
{
    private static readonly ILogger Logger = LoggerFactory.Create<FirstWorker>();

    private readonly TimeSpan _backgroundTaskLockDuration = TimeSpan.FromMinutes(30);
    private readonly TaskLocker _taskLocker;

    public FirstWorker(ISessionFactory sessionFactory)
    {
        _taskLocker = new TaskLocker(sessionFactory, new TaskLockerConfiguration
        {
            ServiceId = 1
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Logger.Debug($"Worker #1 is running at: {DateTime.Now}");

            if (_taskLocker.TryLock(BackgroundTask.UpdateUsersBlackList, _backgroundTaskLockDuration, out var taskLock))
            {
                Logger.Info("Worker#1 acquired the task #1");

                try
                {
                    Logger.Info("Worker#1 is doing its work!");

                    await Task.Delay(10_000, stoppingToken);

                    Logger.Info("Worker#1 finished its work!");
                }
                finally
                {
                    taskLock.Release();
                }
            }
            else
            {
                Logger.Info("Worker#1 could not acquire the task #1");
            }

            await Task.Delay(5_000, stoppingToken);
        }
    }
}