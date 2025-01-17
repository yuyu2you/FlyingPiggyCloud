﻿using Newtonsoft.Json;

namespace SixCloud.Models
{
    public abstract class ResultBase
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        //[JsonProperty(PropertyName = "token")]
        //public string Token { get; set; }
    }
}
