using System.Data.SqlClient;
using System;
using System.Text;

namespace TinderCloneV1 {
    public class GeneralFunctions {
        static public string SafeGetString(SqlDataReader reader, int index) {
            if (!reader.IsDBNull(index))
                return reader.GetString(index);
            return string.Empty;
        }

        static public int SafeGetInt32(SqlDataReader reader, int index) {
            if (!reader.IsDBNull(index))
                return reader.GetInt32(index);
            return 0;
        }
        
        static public DateTime SafeGetDateTime(SqlDataReader reader, int index){
            if (!reader.IsDBNull(index))
                return reader.GetDateTime(index);
            return new DateTime(0000-00-00);
        }
    }
}