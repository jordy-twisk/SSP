using Newtonsoft.Json;
using System;

namespace TinderCloneV1 {
    public class User{
        [JsonProperty("studentID")]
        public int studentID{
            get;
            set;
        }

        [JsonProperty("firstName")]
        public string firstName{
            get;
            set;
        }

        [JsonProperty("surName")]
        public string surName{
            get;
            set;
        }

        [JsonProperty("phoneNumber")]
        public string phoneNumber{
            get;
            set;
        }

        [JsonProperty("photo")]
        public string photo{
            get;
            set;
        }

        [JsonProperty("description")]
        public string description{
            get;
            set;
        }

        [JsonProperty("degree")]
        public string degree{
            get;
            set;
        }

        [JsonProperty("study")]
        public string study{
            get;
            set;
        }

        [JsonProperty("studyYear")]
        public int studyYear{
            get;
            set;
        }

        [JsonProperty("interests")]
        public string interests {
            get;
            set;
        }
    }
}
