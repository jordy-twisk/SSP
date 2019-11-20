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
        public int password {
            get;
            set;
        }
    }
}
