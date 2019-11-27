using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class UserAuth{
        [JsonProperty("studentID")]
        public int StudentID {
            get;
            set;
        }
        [JsonProperty("password")]
        public string Password {
            get;
            set;
        }
        [JsonProperty("hash")]
        public string Hash
        {
            get;
            set;
        }
        [JsonProperty("salt")]
        public string Salt
        {
            get;
            set;
        }
        [JsonProperty("role")]
        public string Role
        {
            get;
            set;
        }
    }
    public class Token
    {
        /*
        [JsonProperty("tokenID")]
        public int TokenID
        {
            get;
            set;
        }
        [JsonProperty("pwID")]
        public int PwID
        {
            get;
            set;
        }
        */
        [JsonProperty("token")]
        public string TokenString
        {
            get;
            set;
        }
        [JsonProperty("created_at")]
        public DateTime Created_at
        {
            get;
            set;
        }
    }
}
