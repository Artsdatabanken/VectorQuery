using System.Collections.Generic;
using Npgsql;

namespace VectorQuery.Data
{
    public static class Codes
    {
        public static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>();

        static Codes()
        {
            var cmdCodes = Sql.GetCmd("SELECT code, title from data.codes c");

            var dr = cmdCodes.ExecuteReader();

            while (dr.Read()) Dictionary[dr[0].ToString()] = dr[1].ToString();

            cmdCodes.Connection?.Close();
        }

        public static Dictionary<string, Code> ReadResults(NpgsqlDataReader dr)
        {
            var results = new Dictionary<string, Code>();

            while (dr.Read())
            {
                var code = dr[0].ToString();

                string parentCode;

                if (code.Split('_').Length < 3)
                    parentCode = code.Split('-').Length > 1
                        ? code.Split('-')[0]
                        : code.Split('_')[0];

                else parentCode = code.Split('_')[0] + '_' + code.Split('_')[1];


                if (!results.ContainsKey(code))
                {
                    results[code] = new Code
                    {
                        Value = dr[1].ToString(),
                        Key = Dictionary[parentCode],
                        Ids = new List<int> {int.Parse(dr[2].ToString())}
                    };
                }

                else results[code].Ids.Add(int.Parse(dr[2].ToString()));

            }

            return results;
        }
    }
}
