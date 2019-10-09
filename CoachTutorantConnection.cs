using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1
{
    public class CoachTutorantConnection{
        [JsonProperty("studentIDTutorant")]
        public int studentIDTutorant {
            get;
            set;
        }
        [JsonProperty("studentIDCoach")]
        public int studentIDCoach {
            get;
            set;
        }
        [JsonProperty("status")]
        public string status {
            get;
            set;
        }
    }
}
