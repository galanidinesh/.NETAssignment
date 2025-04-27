using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ReqResApiClient.Configuration;
using ReqResApiClient.Interfaces;
using ReqResApiClient.Models;

namespace ReqResApiClient.Services
{
    public class ExternalUserService : IExternalUserService
    {
        /// <summary>
        /// The API client
        /// </summary>
        private readonly IReqResApiClient _apiClient;

        /// <summary>
        /// The cache
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// The cache expiration
        /// </summary>
        private readonly TimeSpan _cacheExpiration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalUserService"/> class.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="cacheSettings">The cache settings.</param>
        public ExternalUserService(IReqResApiClient apiClient, IMemoryCache cache, IOptions<CacheSettings> cacheSettings)
        {
            _apiClient = apiClient;
            _cache = cache;
            var minutes = cacheSettings.Value?.ExpirationMinutes ?? 5;
            _cacheExpiration = TimeSpan.FromMinutes(minutes > 0 ? minutes : 5);
        }

        /// <summary>
        /// Gets the user by identifier asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public async Task<User> GetUserByIdAsync(int userId)
        {
            var cacheKey = $"User_{userId}";

            if (_cache.TryGetValue(cacheKey, out User cachedUser))
            {
                return cachedUser;
            }

            var user = await _apiClient.GetUserByIdAsync(userId);

            _cache.Set(cacheKey, user, _cacheExpiration);

            return user;
        }

        /// <summary>
        /// Gets all users asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string cacheKey = "AllUsers";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<User> cachedUsers))
            {
                return cachedUsers;
            }

            var users = await _apiClient.GetAllUsersAsync();

            _cache.Set(cacheKey, users, _cacheExpiration);

            return users;
        }
    }
}
