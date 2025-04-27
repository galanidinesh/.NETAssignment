using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReqResApiClient.Configuration
{
    public class RetryPolicySettings
    {
        public int RetryCount { get; set; } = 3; // default 3 times
        public int RetryDelayMilliseconds { get; set; } = 500; // default 500 ms
    }
}
