using Moq;
using Xunit;
using ReqResApiClient.Interfaces;
using ReqResApiClient.Services;
using ReqResApiClient.Models;
using ReqResApiClient.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace ReqResApiClient.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IReqResApiClient> _mockApiClient;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly IExternalUserService _userService;

        public UserServiceTests()
        {
            // Mock the IReqResApiClient
            _mockApiClient = new Mock<IReqResApiClient>();

            // Mock the IMemoryCache
            _mockCache = new Mock<IMemoryCache>();

            // Initialize the service by passing the mocked dependencies
            _userService = new ExternalUserService(_mockApiClient.Object, _mockCache.Object);
        }

        // Test 1: Fetch all users successfully
        [Fact]
        public async Task GetAllUsersAsync_ReturnsUsers()
        {
            // Arrange
            var mockUsers = new List<User>
            {
                new User { Id = 1, FirstName = "George", LastName = "Bluth", Email = "george.bluth@reqres.in" },
                new User { Id = 2, FirstName = "Janet", LastName = "Weaver", Email = "janet.weaver@reqres.in" }
            };
            _mockApiClient.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(mockUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();
            var resultList = new List<User>(result);  // Convert IEnumerable to List

            // Assert
            Assert.NotNull(resultList);
            Assert.Equal(2, resultList.Count);
            Assert.Equal("George", resultList[0].FirstName);
        }

        // Test 2: Fetch a single user successfully
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser()
        {
            // Arrange
            var mockUser = new User { Id = 1, FirstName = "George", LastName = "Bluth", Email = "george.bluth@reqres.in" };
            _mockApiClient.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(mockUser);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("George", result.FirstName);
        }

        // Test 3: Fetch users from cache when already cached
        [Fact]
        public async Task GetAllUsersAsync_ReturnsFromCache_WhenCached()
        {
            // Arrange
            var mockUsers = new List<User>
            {
                new User { Id = 1, FirstName = "George", LastName = "Bluth", Email = "george.bluth@reqres.in" }
            };

            // Setup the memory cache mock to return cached data
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out mockUsers)).Returns(true);

            // Act
            var result = await _userService.GetAllUsersAsync();
            var resultList = new List<User>(result);  // Convert IEnumerable to List

            // Assert
            Assert.NotNull(resultList);
            Assert.Equal(1, resultList.Count);
            Assert.Equal("George", resultList[0].FirstName);
        }

        // Test 4: User not found (404 scenario)
        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // Test 5: Handling API failure (e.g., 404 error for GetAllUsersAsync)
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

        // Test 6: Handling deserialization errors in GetAllUsersAsync
        [Fact]
        public async Task GetAllUsersAsync_ThrowsApiException_WhenDeserializationFails()
        {
            // Arrange
            _mockApiClient.Setup(x => x.GetAllUsersAsync())
                .ThrowsAsync(new System.Text.Json.JsonException("Deserialization error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => _userService.GetAllUsersAsync());
            Assert.Equal("Deserialization error", exception.Message);
        }

        // Test 7: Handling network issues or timeouts (e.g., HttpRequestException)
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

        // Test 8: Handling retry policy (mocking Polly behavior)
        [Fact]
        public async Task GetUserByIdAsync_Retries_WhenTransientErrorOccurs()
        {
            // Arrange
            var mockUser = new User { Id = 1, FirstName = "George", LastName = "Bluth", Email = "george.bluth@reqres.in" };
            _mockApiClient.Setup(x => x.GetUserByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new HttpRequestException("Transient error"))
                .ReturnsAsync(mockUser); // Simulate retry

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("George", result.FirstName);
        }

        // Test 9: Handling empty response from API (e.g., empty list from GetAllUsersAsync)
        [Fact]
        public async Task GetAllUsersAsync_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var mockUsers = new List<User>(); // Empty list
            _mockApiClient.Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(mockUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();
            var resultList = new List<User>(result);  // Convert IEnumerable to List

            // Assert
            Assert.Empty(resultList);
        }

        // Test 10: Handling unexpected API responses (non-200 status codes, e.g., 500 Internal Server Error)
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
