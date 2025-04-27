using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using ReqResApiClient.Configuration;
using ReqResApiClient.Exceptions;
using ReqResApiClient.Interfaces;
using ReqResApiClient.Models;
using ReqResApiClient.Services;

namespace ReqResApiClient.Tests
{
    public class UserServiceTests
    {
        /// <summary>
        /// The mock API client
        /// </summary>
        private readonly Mock<IReqResApiClient> _mockApiClient;

        /// <summary>
        /// The memory cache
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// The cache settings
        /// </summary>
        private readonly IOptions<CacheSettings> _cacheSettings;

        /// <summary>
        /// The user service
        /// </summary>
        private readonly IExternalUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
        /// </summary>
        public UserServiceTests()
        {
            _mockApiClient = new Mock<IReqResApiClient>();

            // Create real MemoryCache instance
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Provide default CacheSettings with 5 minutes expiration
            _cacheSettings = Options.Create(new CacheSettings { ExpirationMinutes = 5 });

            _userService = new ExternalUserService(_mockApiClient.Object, _memoryCache, _cacheSettings);
        }

        /// <summary>
        /// Gets all users asynchronous returns users.
        /// </summary>
        [Fact]
        public async Task GetAllUsersAsync_ReturnsUsers()
        {
            // Arrange
            var mockUsers = new List<User>
            {
                new User { Id = 1, FirstName = "George", LastName = "Bluth", Email = "george.bluth@reqres.in" },
                new User { Id = 2, FirstName = "Janet", LastName = "Weaver", Email = "janet.weaver@reqres.in" }
            };
            _mockApiClient.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(mockUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();
            var resultList = new List<User>(result);

            // Assert
            Assert.NotNull(resultList);
            Assert.Equal(2, resultList.Count);
            Assert.Equal("George", resultList[0].FirstName);
        }

        /// <summary>
        /// Gets the user by identifier asynchronous returns user.
        /// </summary>
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser()
        {
            // Arrange
            var mockUser = new User { Id = 1, FirstName = "George", LastName = "Bluth", Email = "george.bluth@reqres.in" };
            _mockApiClient.Setup(x => x.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(mockUser);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("George", result.FirstName);
        }

        /// <summary>
        /// Gets the user by identifier asynchronous returns from cache when user is cached.
        /// </summary>
        [Fact]
        public async Task GetUserByIdAsync_ReturnsFromCache_WhenUserIsCached()
        {
            // Arrange
            var userId = 1;
            var cachedUser = new User { Id = userId, FirstName = "Cached", LastName = "User", Email = "cached.user@example.com" };

            _memoryCache.Set($"User_{userId}", cachedUser, TimeSpan.FromMinutes(5));

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cachedUser.Id, result.Id);
        }

        /// <summary>
        /// Gets the user by identifier asynchronous returns null when user not found.
        /// </summary>
        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Gets all users asynchronous throws API exception when API fails.
        /// </summary>
        [Fact]
        public async Task GetAllUsersAsync_ThrowsApiException_WhenApiFails()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new ApiException("API call failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(() => _userService.GetAllUsersAsync());
            Assert.Equal("API call failed", exception.Message);
        }

        /// <summary>
        /// Gets all users asynchronous throws json exception when deserialization fails.
        /// </summary>
        [Fact]
        public async Task GetAllUsersAsync_ThrowsJsonException_WhenDeserializationFails()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new System.Text.Json.JsonException("Deserialization error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => _userService.GetAllUsersAsync());
            Assert.Equal("Deserialization error", exception.Message);
        }

        /// <summary>
        /// Gets the user by identifier asynchronous throws HTTP request exception when network fails.
        /// </summary>
        [Fact]
        public async Task GetUserByIdAsync_ThrowsHttpRequestException_WhenNetworkFails()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _userService.GetUserByIdAsync(1));
            Assert.Equal("Network error", exception.Message);
        }

        /// <summary>
        /// Gets all users asynchronous returns empty list when no users exist.
        /// </summary>
        [Fact]
        public async Task GetAllUsersAsync_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var mockUsers = new List<User>(); // Empty list
            _mockApiClient.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(mockUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();
            var resultList = new List<User>(result);

            // Assert
            Assert.Empty(resultList);
        }

        /// <summary>
        /// Gets all users asynchronous throws API exception when unexpected error occurs.
        /// </summary>
        [Fact]
        public async Task GetAllUsersAsync_ThrowsApiException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new ApiException("Unexpected error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(() => _userService.GetAllUsersAsync());
            Assert.Equal("Unexpected error", exception.Message);
        }
    }
}
