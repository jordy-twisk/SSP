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

        [JsonProperty("student")]
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
