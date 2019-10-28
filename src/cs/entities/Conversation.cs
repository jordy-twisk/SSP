using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class Conversation{
        [JsonProperty("MessagesID")]
        public int MessagesID {
            get;
            set;
        }
        [JsonProperty("MessageID")]
        public long MessageID {
            get;
            set;
        }
    }
}
