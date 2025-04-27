using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using ReqResApiClient.Interfaces;
using ReqResApiClient.Models;
using ReqResApiClient.Services;

namespace ReqResApiClient.Tests.Services
{
    public class ExternalUserServiceTests
    {
        private readonly Mock<IReqResApiClient> _apiClientMock;
        private readonly IMemoryCache _memoryCache;
        private readonly ExternalUserService _service;

        public ExternalUserServiceTests()
        {
            _apiClientMock = new Mock<IReqResApiClient>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _service = new ExternalUserService(_apiClientMock.Object, _memoryCache);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
            _apiClientMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };
            _apiClientMock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(2);
        }
    }
}
