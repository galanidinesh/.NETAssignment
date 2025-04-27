using ReqResApiClient.Models;

namespace ReqResApiClient.Interfaces
{
    public interface IReqResApiClient
    {
        /// <summary>
        /// Gets the user by identifier asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        Task<User> GetUserByIdAsync(int userId);
        /// <summary>
        /// Gets all users asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}