using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1
{
    public class Message {
        [JsonProperty("MessageID")]
        public long MessageID {
            get;
            set;
        }
        [JsonProperty("type")]
        public string type {
            get;
            set;
        }
        [JsonProperty("payload")]
        public string payload {
            get;
            set;
        }
        [JsonProperty("created")]
        public DateTime created {
            get;
            set;
        }
        [JsonProperty("lastModified")]
        public DateTime lastModified {
            get;
            set;
        }
    }
}
