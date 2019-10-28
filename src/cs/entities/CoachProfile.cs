using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class CoachProfile {
        [JsonProperty("coach")]
        public Coach coach{
            get;
            set;
        }

        [JsonProperty("user")]
        public User user{
            get;
            set;
        }

        public CoachProfile(Coach coach, User user) {
            this.coach = coach;
            this.user = user;
        }

        public CoachProfile() {

        }
    }
}
