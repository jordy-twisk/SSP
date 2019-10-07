using System;
using System.Collections.Generic;
using System.Text;

namespace TinderCloneV1
{
    public class Message
    {
        public long MessageID { get; set; }
        public string type { get; set; }
        public string payload { get; set; }
        public DateTime created { get; set; }
        public DateTime lastModified { get; set; }
    }
}
