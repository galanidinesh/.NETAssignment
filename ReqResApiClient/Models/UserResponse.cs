using Newtonsoft.Json;

namespace ReqResApiClient.Models
{
    public class UserResponse
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty("data")]
        public User Data { get; set; }

        /// <summary>
        /// Gets or sets the support.
        /// </summary>
        /// <value>
        /// The support.
        /// </value>
        [JsonProperty("support")]
        public Support Support { get; set; }
    }
}
