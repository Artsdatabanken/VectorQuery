using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace VectorQuery.Data
{
    public static class Codes
    {
        public static readonly Dictionary<string, string> CodesPredecessors = new Dictionary<string, string>();

        public static readonly Dictionary<int, string> CodeIds = new Dictionary<int, string>();

        public static readonly Dictionary<int, List<int>> GeometryCodesIds = new Dictionary<int, List<int>>();

        public static readonly Dictionary<int, List<Code>> GeometryCodes = new Dictionary<int, List<Code>>();

        public static readonly Dictionary<int, string> GeometryLocalids = new Dictionary<int, string>();

        public static readonly Dictionary<string, string> CodeTitles = new Dictionary<string, string>();

        static Codes()
        {
            FillCodesDictionary();

            FillGeometryDictionary();

            FillGeometryLocalids();
        }


        public static void Load()
        {

        }

        private static void FillGeometryDictionary()
        {
            var cmdCodes = Sql.GetCmd("SELECT geometry_id, codes_id, created, fraction, code from data.codes_geometry");

            var dr = cmdCodes.ExecuteReader();

            while (dr.Read())
            {
                var geometryId = int.Parse(dr[0].ToString());

                var codeId = int.Parse(dr[1].ToString());

                if (!GeometryCodesIds.ContainsKey(geometryId)) GeometryCodesIds[geometryId] = new List<int>();

                GeometryCodesIds[geometryId].Add(codeId);

                if (!GeometryCodes.ContainsKey(geometryId)) GeometryCodes[geometryId] = new List<Code>();

                var created = dr[2].ToString();

                var fraction = string.IsNullOrEmpty(dr[3].ToString()) ? 0 : Convert.ToInt32(dr[3].ToString());

                var key = dr[4].ToString();

                if (string.IsNullOrEmpty(created))
                {

                    GeometryCodes[geometryId].Add(new Code
                    {
                        Fraction = fraction,
                        Key = key,
                        Predecessor = CodesPredecessors[key],
                        Value = CodeTitles[key]
                    });

                    continue;
                }

                GeometryCodes[geometryId].Add(new Code
                {
                    Created = Convert.ToDateTime(created),
                    Fraction = fraction,
                    Key = key,
                    Predecessor = CodesPredecessors[key],
                    Value = CodeTitles[key]
                });
            }

            cmdCodes.Connection?.Close();
        }

        private static void FillCodesDictionary()
        {
            var cmdCodes = Sql.GetCmd("SELECT code, predecessor, id, title from data.codes");

            var dr = cmdCodes.ExecuteReader();

            while (dr.Read())
            {
                var code = dr[0].ToString();

                var title = dr[3].ToString();

                var predecessor = dr[1].ToString();

                var id = int.Parse(dr[2].ToString());

                CodesPredecessors[code] = predecessor;

                CodeIds[id] = code;

                CodeTitles[code] = title;
            }

            cmdCodes.Connection?.Close();
        }

        private static void FillGeometryLocalids()
        {
            var cmdCodes = Sql.GetCmd("SELECT geometry_id, localid from data.localid_geometry");

            var dr = cmdCodes.ExecuteReader();

            while (dr.Read())
            {
                var geometryId = int.Parse(dr[0].ToString());

                var localId = dr[1].ToString();

                GeometryLocalids[geometryId] = localId;
            }

            cmdCodes.Connection?.Close();
        }


        public static List<Code> ReadResults(NpgsqlDataReader dr, string[] codes = null)
        {
            var results = new Dictionary<string, Code>();

            while (dr.Read())
            {
                var geometryId = int.Parse(dr[0].ToString());

                if (GeometryCodes.ContainsKey(geometryId))
                {
                    AddResults(GeometryCodes[geometryId], results);
                }
            }

            return FixResults(results.Values.ToList(), codes);
        }

        private static void AddResults(List<Code> geometryCode, IDictionary<string, Code> results)
        {
            geometryCode.ForEach(g =>
                results[g.Key] = results.ContainsKey(g.Key)
                    ? results[g.Key].Created < g.Created ? g : results[g.Key]
                    : g);
        }

        private static List<Code> FixResults(IReadOnlyCollection<Code> results, string[] codes = null)
        {

            var predecessors = new Dictionary<string, bool>();

            var successors = new Dictionary<string, bool>();

            foreach (var result in results)
            {
                predecessors[result.Predecessor] = true;
                successors[result.Key] = true;
            }

            var structuredResults = StructureResults(results,
                FindRootNodes(results, predecessors.Keys.ToList(), successors.Keys.ToList()));

            return codes == null ? structuredResults : RemoveSuperfluousResults(structuredResults, codes);
        }

        private static List<Code> RemoveSuperfluousResults(IReadOnlyCollection<Code> structuredResults,
            IEnumerable<string> codes)
        {
            return codes.Select(code => FindRootNodes(structuredResults, code)).Where(rootnode => rootnode != null)
                .ToList();
        }

        private static Code FindRootNodes(IEnumerable<Code> results, string code)
        {
            return (from result in results select result.Key == code ? result : FindRootNodes(result.Successors, code))
                .FirstOrDefault();
        }

        private static List<Code> FindRootNodes(IEnumerable<Code> resultsList, IEnumerable<string> predecessors,
            ICollection<string> successors)
        {
            predecessors = predecessors.Where(p => !successors.Contains(p));

            return resultsList.Where(r => predecessors.Contains(r.Predecessor)).ToList();
        }

        private static List<Code> StructureResults(IReadOnlyCollection<Code> results,
            IReadOnlyCollection<Code> predecessors)
        {
            foreach (var predecessor in predecessors)
                predecessor.Successors =
                    StructureResults(results, results.Where(r => r.Predecessor == predecessor.Key).ToList());

            return predecessors.ToList();
        }
    }
}
