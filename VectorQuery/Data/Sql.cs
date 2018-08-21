using Npgsql;
using System.Collections.Generic;

namespace VectorQuery.Data
{
    internal class Sql
    {
        public static NpgsqlCommand GetCmd(string sql)
        {
            var conn = new NpgsqlConnection(
                "Server=vectorquery_bigbadabom;User Id=reader;Password=reader;Database=bigbadabom");

            conn.Open();

            return new NpgsqlCommand(sql, conn);
        }

        public static string CreateIntersectQuery(string queryGeometry)
        {
            return $"SELECT c.code, c.title FROM data.geometry g, data.codes_geometry c_g, data.codes c, data.dataset d, data.prefix p WHERE ST_Intersects(g.geography, {queryGeometry}) and c_g.geometry_id = g.id and c_g.codes_id = c.id and g.dataset_id = d.id and d.prefix_id = p.id";
        }

        public static Dictionary<string, Code> Execute(NpgsqlCommand cmd)
        {
            var dr = cmd.ExecuteReader();

            var results = Codes.ReadResults(dr);

            cmd.Connection?.Close();

            return results;
        }

        public static Dictionary<string, Code> GetIntersectingCodes(string queryGeometry)
        {
            var cmd = GetCmd(CreateIntersectQuery(queryGeometry));

            return Execute(cmd);
        }

        internal static Dictionary<string, Code> GetIntersectingCodes(string queryGeometry, string prefix)
        {
            var cmd = GetCmd(CreateIntersectQuery(queryGeometry) + $" and p.value in ({Fnuttify(prefix)})");

            return Execute(cmd);
        }

        private static string Fnuttify(string prefix)
        {
            return "'" + prefix.Replace(",", "','").TrimEnd('\'').TrimEnd(',') + "'";
        }

        public static string CreatePoint(double x, double y)
        {
            return $"ST_GeomFromText(\'POINT({x} {y})\', 4326)";
        }

        public static string CreateArea(double minx, double miny, double maxx, double maxy)
        {
            return $"ST_MakeEnvelope({minx},{miny},{maxx},{maxy}, 4326)";
        }
    }
}