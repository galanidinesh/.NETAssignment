using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReqResApiClient.Configuration;
using ReqResApiClient.Exceptions;
using ReqResApiClient.Interfaces;
using ReqResApiClient.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReqResApiClient.Clients
{
    public class ReqResApiClient : IReqResApiClient
    {
        private readonly HttpClient _httpClient;

        public ReqResApiClient(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(apiSettings.Value.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiSettings.Value.ApiKey);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"users/{userId}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        throw new NotFoundException($"User with ID {userId} not found.");

                    throw new ApiException($"Failed to fetch user with ID {userId}: {response.ReasonPhrase}");
                }

                var content = await response.Content.ReadAsStringAsync();

                var userResponse = JsonConvert.DeserializeObject<UserResponse>(content);

                if (userResponse?.Data == null)
                {
                    throw new ApiException("Invalid user data returned from API.");
                }

                return userResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Network error while calling API.", ex);
            }
            catch (JsonException ex)
            {
                throw new ApiException("Failed to parse API response.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("API request timed out.", ex);
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            int page = 1, totalPages;

            try
            {
                do
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"users?page={page}");

                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ApiException($"Failed to fetch users on page {page}: {response.ReasonPhrase}");
                    }

                    var content = await response.Content.ReadAsStringAsync();

                    var userList = JsonConvert.DeserializeObject<UserListResponse>(content);

                    if (userList?.Data == null)
                    {
                        throw new ApiException("Invalid user list data returned from API.");
                    }

                    users.AddRange(userList.Data);
                    totalPages = userList.TotalPages;
                    page++;

                } while (page <= totalPages);

                return users;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Network error while calling API.", ex);
            }
            catch (JsonException ex)
            {
                throw new ApiException("Failed to parse API response.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiException("API request timed out.", ex);
            }
        }
    }
}
