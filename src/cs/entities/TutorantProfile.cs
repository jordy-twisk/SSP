using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    class TutorantProfile {
        [JsonProperty("tutorant")]
        public Tutorant tutorant{
            get;
            set;
        }

        [JsonProperty("user")]
        public User user{
            get;
            set;
        }

        public TutorantProfile(Tutorant tutorant, User user) {
            this.tutorant = tutorant;
            this.user = user;
        }

        public TutorantProfile() {
        }
    }
}
