namespace ReqResApiClient.Configuration
{
    public class RetryPolicySettings
    {
        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        /// <value>
        /// The retry count.
        /// </value>
        public int RetryCount { get; set; } = 3; // default 3 times
        /// <summary>
        /// Gets or sets the retry delay milliseconds.
        /// </summary>
        /// <value>
        /// The retry delay milliseconds.
        /// </value>
        public int RetryDelayMilliseconds { get; set; } = 500; // default 500 ms
    }
}
