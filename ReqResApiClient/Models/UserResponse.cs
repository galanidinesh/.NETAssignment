using Newtonsoft.Json;

namespace ReqResApiClient.Models
{
    public class UserResponse
    {
        [JsonProperty("data")]
        public User Data { get; set; }

        [JsonProperty("support")]
        public Support Support { get; set; }
    }
}
