using System.IO;
using BackgroundTaskLocker.Nh;
using Microsoft.Extensions.Configuration;

namespace BackgroundTaskLocker.Tests;

public class IntegrationTest
{
    public IntegrationTest()
    {
        var configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json")
                            .AddJsonFile("appsettings.Development.json")
                            .AddEnvironmentVariables()
                            .Build();

        ConnectionStringsManager.ReadFromConfiguration(configuration);
    }
}