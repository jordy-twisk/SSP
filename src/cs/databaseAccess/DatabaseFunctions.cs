using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace TinderCloneV1 {
    class DatabaseFunctions {

        public DatabaseFunctions() { }

        public void AddSqlInjection(JObject rboy, dynamic dynaObject, SqlCommand cmd) {
            foreach (JProperty property in rboy.Properties()) {
                foreach (PropertyInfo props in dynaObject.GetType().GetProperties()) {
                    if (props.Name == property.Name) {
                        var type = Nullable.GetUnderlyingType(props.PropertyType) ?? props.PropertyType;

                        if (type == typeof(string)) {
                            cmd.Parameters.Add(property.Name, SqlDbType.VarChar).Value = props.GetValue(dynaObject, null);
                        }
                        if (type == typeof(int)) {
                            cmd.Parameters.Add(property.Name, SqlDbType.Int).Value = props.GetValue(dynaObject, null);
                        }
                        if (type == typeof(DateTime)) {
                            cmd.Parameters.Add(property.Name, SqlDbType.DateTime).Value = props.GetValue(dynaObject, null);
                        }
                    }
                }
            }
        }
        public string RemoveLastCharacters(string queryString, int NumberOfCharacters) {
            queryString = queryString.Remove(queryString.Length - NumberOfCharacters);
            return queryString;
        }
    }
}
