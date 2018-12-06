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

        public static List<Code> ReadResults(NpgsqlDataReader dr)
        {
            var results = new Dictionary<string, Code>();

            var predecessors = new Dictionary<string, bool>();

            var successors = new Dictionary<string, bool>();

            while (dr.Read())
            {
                var result = CreateCodeFromDataReader(dr);

                predecessors[result.Predecessor] = true;
                successors[result.Key] = true;

                if (result.Key.Split('-')[0] == "NA") AddResultIfNewOrNewer(results, CreateNaCode(dr, result));

                else results[result.Key] = result;
            }

            var resultsList = results.Values.ToList();

            return StructureResults(resultsList, FindRootNodes(resultsList, predecessors.Keys.ToList(), successors.Keys.ToList()) );
        }

        private static void AddResultIfNewOrNewer(IDictionary<string, Code> results, Code result)
        {
            if (!results.ContainsKey(result.Key) || results[result.Key].Created < result.Created) results[result.Key] = result;
        }

        private static Code CreateCodeFromDataReader(IDataRecord dr)
        {
            return new Code
            {
                Key = dr[0].ToString(),
                Value = dr[1].ToString(),
                Predecessor = dr[6].ToString()
            };
        }

        private static List<Code> FindRootNodes(IEnumerable<Code> resultsList,  IEnumerable<string> predecessors, ICollection<string> successors)
        {
            predecessors = predecessors.Where(p => !successors.Contains(p));

            return resultsList.Where(r => predecessors.Contains(r.Predecessor)).ToList();
        }

        private static List<Code> StructureResults(IReadOnlyCollection<Code> results, IReadOnlyCollection<Code> predecessors)
        {
            foreach (var predecessor in predecessors) predecessor.Successors = StructureResults(results, results.Where(r => r.Predecessor == predecessor.Key).ToList());

            return predecessors.ToList();
        }

        private static Code CreateNaCode(IDataRecord dr, Code code)
        {
            code.Id = dr[2].ToString();
 
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
