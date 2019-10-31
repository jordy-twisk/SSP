using System.Data.SqlClient;

namespace TinderCloneV1 {
    public class GeneralFunctions {
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
    }
}