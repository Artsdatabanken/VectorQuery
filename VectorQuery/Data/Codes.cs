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
                var codeSplitUnderscore = code.Split('_');
                var codeSplitDash = code.Split('-');

                if (codeSplitUnderscore.Length < 3)
                    parentCode = codeSplitDash.Length > 1
                        ? codeSplitDash[0]
                        : codeSplitUnderscore[0];

                else parentCode = codeSplitUnderscore[0] + '_' + codeSplitUnderscore[1];

                var id = string.IsNullOrEmpty(dr[2].ToString()) ? dr[3].ToString() : dr[2].ToString().Replace("{", "").Replace("}", "");
                
                var fraction = string.IsNullOrEmpty(dr[4].ToString()) ? 10 : int.Parse(dr[4].ToString());

                if (!results.ContainsKey(code))
                {
                    results[code] = new Code
                    {
                        Value = dr[1].ToString(),
                        Key = Dictionary[parentCode],
                        Ids = new List<string> {id},
                        Fraction = fraction
                    };
                }

                else results[code].Ids.Add(id);

            }

            return results;
        }
    }
}
