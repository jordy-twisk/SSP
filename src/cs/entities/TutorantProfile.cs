using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class TutorantProfile {
        [JsonProperty("tutorant")]
        public Tutorant tutorant{
            get;
            set;
        }

        [JsonProperty("user")]
        public Student user{
            get;
            set;
        }

        public TutorantProfile(Tutorant tutorant, Student user) {
            this.tutorant = tutorant;
            this.user = user;
        }

        public TutorantProfile() {
        }
    }
}
