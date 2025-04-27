using Microsoft.Extensions.Caching.Memory;
using ReqResApiClient.Interfaces;
using ReqResApiClient.Models;

namespace ReqResApiClient.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IReqResApiClient _apiClient;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

        public ExternalUserService(IReqResApiClient apiClient, IMemoryCache cache)
        {
            _apiClient = apiClient;
            _cache = cache;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var cacheKey = $"User_{userId}";

            if (_cache.TryGetValue(cacheKey, out User cachedUser))
            {
                return cachedUser;
            }

            var user = await _apiClient.GetUserByIdAsync(userId);

            _cache.Set(cacheKey, user, CacheExpiration);

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string cacheKey = "AllUsers";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<User> cachedUsers))
            {
                return cachedUsers;
            }

            var users = await _apiClient.GetAllUsersAsync();

            _cache.Set(cacheKey, users, CacheExpiration);

            return users;
        }
    }
}
