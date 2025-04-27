using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using ReqResApiClient.Configuration;
using ReqResApiClient.Exceptions;
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

            try
            {
                Console.WriteLine("Fetching all users...");
                var users = await userService.GetAllUsersAsync();

                if (users == null || !users.Any())
                {
                    Console.WriteLine("No users found.");
                }
                else
                {
                    foreach (var user in users)
                    {
                        Console.WriteLine($"{user.Id}: {user.FirstName} {user.LastName} - {user.Email}");
                    }
                }

                Console.WriteLine("\nFetching a single user (ID = 2)...");
                var singleUser = await userService.GetUserByIdAsync(2);

                if (singleUser != null)
                {
                    Console.WriteLine($"{singleUser.Id}: {singleUser.FirstName} {singleUser.LastName} - {singleUser.Email}");
                }
                else
                {
                    Console.WriteLine("User not found.");
                }
            }
            catch (ApiException apiEx)
            {
                // Handle known API exceptions, such as 404 not found or other API errors
                Console.WriteLine($"API error: {apiEx.Message}");
            }
            catch (NotFoundException notFoundEx)
            {
                // Handle not found exceptions for specific users
                Console.WriteLine($"User not found: {notFoundEx.Message}");
            }
            catch (HttpRequestException httpEx)
            {
                // Handle network-related issues
                Console.WriteLine($"Network error: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                // Handle deserialization issues
                Console.WriteLine($"Error processing data: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }


        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration((context, config) =>
                 {
                     // Adding appsettings.json to the configuration pipeline
                     config.SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                 })
                .ConfigureServices((context, services) =>
                {
                    // Bind ApiSettings from configuration
                    services.Configure<ApiSettings>(context.Configuration.GetSection("ApiSettings"));

                    // Bind RetryPolicySettings from configuration
                    services.Configure<RetryPolicySettings>(context.Configuration.GetSection("RetryPolicySettings"));

                    //we can also bind configuration like this.
                    //services.Configure<ApiSettings>(options =>
                    //{
                    //    options.BaseUrl = "https://reqres.in/api/";
                    //    options.ApiKey = "reqres-free-v1";
                    //});

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
