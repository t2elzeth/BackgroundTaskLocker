using BackgroundTaskLocker.Application;
using NHibernate;

namespace BackgroundTaskLocker.Workers;

public class SecondWorker : BackgroundService
{
    private static readonly ILogger Logger = LoggerFactory.Create<FirstWorker>();

    private readonly TimeSpan _backgroundTaskLockDuration = TimeSpan.FromMinutes(30);
    private readonly TaskLocker _taskLocker;

    public SecondWorker(ISessionFactory sessionFactory)
    {
        _taskLocker = new TaskLocker(sessionFactory, new TaskLockerConfiguration
        {
            ServiceId = 2
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);

            Logger.Debug($"Worker#2 is running at: {DateTime.Now}");

            if (_taskLocker.TryLock(BackgroundTask.UpdateUsersBlackList, _backgroundTaskLockDuration, out var taskLock))
            {
                Logger.Info("Worker#2 acquired the task #1");

                try
                {
                    Logger.Info("Worker#2 is doing its work!");

                    await Task.Delay(10_000, stoppingToken);

                    Logger.Info("Worker#2 finished its work!");
                }
                finally
                {
                    taskLock.Release();
                }
            }
            else
            {
                Logger.Info("Worker#2 could not acquire the task #1");
            }
        }
    }
}