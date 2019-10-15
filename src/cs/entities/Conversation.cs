using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1
{
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
        [JsonProperty("SenderID")]
        public int SenderID {
            get;
            set;
        }
        [JsonProperty("ReceiverID")]
        public int ReceiverID {
            get;
            set;
        }
    }
}
