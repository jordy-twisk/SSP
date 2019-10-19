using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class Coach{
        [JsonProperty("studentID")]
        public int studentID {
            get;
            set;
        }
        [JsonProperty("workload")]
        public int workload {
            get;
            set;
        }
    }
}
