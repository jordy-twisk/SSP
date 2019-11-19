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
}
