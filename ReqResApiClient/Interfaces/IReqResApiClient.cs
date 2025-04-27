using ReqResApiClient.Models;

namespace ReqResApiClient.Interfaces
{
    public interface IReqResApiClient
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}