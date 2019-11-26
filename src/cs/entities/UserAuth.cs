using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class UserAuth{
        [JsonProperty("studentID")]
        public int studentID {
            get;
            set;
        }
        [JsonProperty("password")]
        public string password {
            get;
            set;
        }
    }
    public class Tokens
    {
        [JsonProperty("tokenID")]
        public int tokenID
        {
            get;
            set;
        }
        [JsonProperty("pwID")]
        public int pwID
        {
            get;
            set;
        }
        [JsonProperty("token")]
        public string token
        {
            get;
            set;
        }
        [JsonProperty("created_at")]
        public DateTime created_at
        {
            get;
            set;
        }
    }
}
