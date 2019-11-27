using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class CoachProfile {
        [JsonProperty("coach"), JsonRequired]
        public Coach coach{
            get;
            set;
        }

        [JsonProperty("student"), JsonRequired]
        public Student student {
            get;
            set;
        }

        public CoachProfile(Coach coach, Student student) {
            this.coach = coach;
            this.student = student;
        }

        public CoachProfile() {

        }
    }
}
