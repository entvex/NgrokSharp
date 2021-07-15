﻿using Newtonsoft.Json;

namespace NgrokSharp.DTO
{
    public partial class TunnelErrorDTO
    {
        [JsonProperty("error_code", NullValueHandling = NullValueHandling.Ignore)]
        public long? ErrorCode { get; set; }

        [JsonProperty("status_code", NullValueHandling = NullValueHandling.Ignore)]
        public long? StatusCode { get; set; }

        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string Msg { get; set; }

        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
        public Details Details { get; set; }
    }

    public partial class Details
    {
        [JsonProperty("err", NullValueHandling = NullValueHandling.Ignore)]
        public string Err { get; set; }
    }
}