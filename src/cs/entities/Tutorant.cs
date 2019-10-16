using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1
{
    public class Tutorant{
        [JsonProperty("studentID")]
        public int studentID {
            get;
            set;
        }

    }
}
