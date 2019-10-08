using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1{
    class User{
        public int studentID{
            get;
            set;
        }
        [JsonProperty("firstName")]
        public string firstName{
            get;
            set;
        }
        public string surName{
            get;
            set;
        }
        public string phoneNumber{
            get;
            set;
        }
        public string photo{
            get;
            set;
        }
        public string description{
            get;
            set;
        }
        public string degree{
            get;
            set;
        }
        public string study{
            get;
            set;
        }
        public int studyYear{
            get;
            set;
        }

        public string interests {
            get;
            set;
        }
    }
}
