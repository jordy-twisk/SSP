using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1.src.cs.entities {
    class TutorantProfile {
        [JsonProperty("tutorant")]
        public Tutorant tutorant;

        [JsonProperty("user")]
        public User user;

        public TutorantProfile(Tutorant tutorant, User user) {
            this.tutorant = tutorant;
            this.user = user;
        }

        public TutorantProfile() {

        }
    }
}
