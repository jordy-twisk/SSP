using System.Data.SqlClient;
using System;

namespace TinderCloneV1 {

    /* Some functions to prevent inserting nullable data in de entity properties from the requestBody */
    public class SafeReader {
        static public string SafeGetString(SqlDataReader reader, int index) {
            if (!reader.IsDBNull(index))
                return reader.GetString(index);
            return string.Empty;
        }

        static public int SafeGetInt(SqlDataReader reader, int index) {
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