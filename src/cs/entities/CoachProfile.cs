using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1 {
    public class CoachProfile {
        private Coach coach;
        private User user;

        public CoachProfile(Coach coach, User user) {
            this.coach = coach;
            this.user = user;
        }

        public CoachProfile() {

        }
    }
}
