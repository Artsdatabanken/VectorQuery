using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                var codeValue = dr[0].ToString();

                var codeValueSplitDash = codeValue.Split('-');

                var codeValueIsNa = codeValueSplitDash[0] == "NA";

                var parentCode = GetParentCodeValue(codeValueSplitDash);

                if (codeValueIsNa)
                {
                    var result = CreateNaCode(dr, parentCode);
                    if (!results.ContainsKey(codeValue) || results[codeValue].Created < result.Created) results[codeValue] = result;
                }
                else
                    results[codeValue] = new Code
                    {
                        Value = dr[1].ToString(),
                        Key = Dictionary[parentCode],
                    };
            }

            return results;
        }

        private static Code CreateNaCode(IDataRecord dr, string parentCode)
        {
            var code = new Code
            {
                Value = dr[1].ToString(),
                Key = Dictionary[parentCode],
                Id = dr[2].ToString()
            };

            if (!IsDbNull(dr[5])) code.Created = (DateTime) dr[5];

            code.Fraction = IsDbNull(dr[4]) ? 10 : (int) dr[4];

            return code;
        }

        private static bool IsDbNull(object tested)
        {
            return tested is DBNull;
        }

        private static string GetParentCodeValue(IReadOnlyList<string> codeValueSplitDash)
        {
            return codeValueSplitDash.Count == 1 ? codeValueSplitDash[0] : string.Join("-", codeValueSplitDash.ToArray(), 0, codeValueSplitDash.Count - 1);
        }
    }
}
