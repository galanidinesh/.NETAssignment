using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using ReqResApiClient.Configuration;
using ReqResApiClient.Interfaces;
using ReqResApiClient.Services;

namespace ReqResApiClient.ConsoleDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var userService = host.Services.GetRequiredService<IExternalUserService>();

            Console.WriteLine("Fetching all users...");
            var users = await userService.GetAllUsersAsync();
            foreach (var user in users)
            {
                Console.WriteLine($"{user.Id}: {user.FirstName} {user.LastName} - {user.Email}");
            }

            Console.WriteLine("\nFetching a single user (ID = 2)...");
            var singleUser = await userService.GetUserByIdAsync(2);
            Console.WriteLine($"{singleUser.Id}: {singleUser.FirstName} {singleUser.LastName} - {singleUser.Email}");
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    //services.Configure<ApiSettings>(options =>
                    //{
                    //    options.BaseUrl = "https://reqres.in/api/";
                    //    options.ApiKey = "reqres-free-v1";
                    //});

                    // Bind ApiSettings from configuration
                    services.Configure<ApiSettings>(context.Configuration.GetSection("ApiSettings"));

                    // Bind RetryPolicySettings from configuration
                    services.Configure<RetryPolicySettings>(context.Configuration.GetSection("RetryPolicySettings"));



                    services.AddMemoryCache();

                    services.AddHttpClient<IReqResApiClient, ReqResApiClient.Clients.ReqResApiClient>()
                    .AddPolicyHandler((serviceProvider, request) =>
                    {
                        var retrySettings = serviceProvider.GetRequiredService<IOptions<RetryPolicySettings>>().Value;

                        return HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .WaitAndRetryAsync(
                                retrySettings.RetryCount,
                                retryAttempt => TimeSpan.FromMilliseconds(retrySettings.RetryDelayMilliseconds),
                                onRetry: (outcome, timespan, retryAttempt, context) =>
                                {
                                    Console.WriteLine($"[Retry Attempt {retryAttempt}] Waiting {timespan.TotalMilliseconds}ms before retrying...");
                                });
                    }); ;


                    services.AddScoped<IExternalUserService, ExternalUserService>();
                });
    }
}
