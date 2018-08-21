using System.Collections.Generic;

namespace VectorQuery.Data
{
    public static class Codes
    {
        public static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>();

        static Codes()
        {
            var cmdCodes = Sql.GetCmd("SELECT code, title from data.codes c");

            var dr = cmdCodes.ExecuteReader();

            while (dr.Read())
            {
                Dictionary[dr[0].ToString()] = dr[1].ToString();
            }

            cmdCodes.Connection?.Close();
        }
    }
}
