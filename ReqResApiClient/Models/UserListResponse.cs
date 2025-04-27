using Newtonsoft.Json;

namespace ReqResApiClient.Models
{
    public class UserListResponse
    {
        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        /// <value>
        /// The page.
        /// </value>
        [JsonProperty("page")]
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the per page.
        /// </summary>
        /// <value>
        /// The per page.
        /// </value>
        [JsonProperty("per_page")]
        public int PerPage { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the total pages.
        /// </summary>
        /// <value>
        /// The total pages.
        /// </value>
        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty("data")]
        public List<User> Data { get; set; }

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
