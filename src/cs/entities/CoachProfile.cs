using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class CoachProfile {
        [JsonProperty("coach")]
        public Coach coach;

        [JsonProperty("user")]
        public User user;

        public CoachProfile(Coach coach, User user) {
            this.coach = coach;
            this.user = user;
        }

        public CoachProfile() {

        }
    }
}
