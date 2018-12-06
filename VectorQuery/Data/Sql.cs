using Npgsql;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace VectorQuery.Data
{
    internal class Sql
    {
        private static string ConnectionString { get; set; }

        public static NpgsqlCommand GetCmd(string sql)
        {
            CheckConnectionString();

            var conn = new NpgsqlConnection(ConnectionString);

            conn.Open();

            return new NpgsqlCommand(sql, conn);
        }

        private static void CheckConnectionString()
        {
            if (!string.IsNullOrEmpty(ConnectionString)) return;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            ConnectionString = configuration.GetConnectionString("postgres");
        }

        public static string CreateIntersectQuery(string queryGeometry)
        {
            return $"SELECT c.code, c.title, l_g.localid, g.id, c_g.fraction, c_g.created, c.predecessor " +
                   $"FROM data.geometry g left join data.localid_geometry l_g on g.id = l_g.geometry_id, data.codes_geometry c_g, data.codes c, data.dataset d " +
                   $"WHERE ST_Intersects(g.geography, {queryGeometry}) and c_g.geometry_id = g.id and c_g.codes_id = c.id and g.dataset_id = d.id";
        }

        public static string CreateIntersectQueryUtm(string queryGeometry)
        {
            return $"SELECT c.code, c.title, l_g.localid, g.id, c_g.fraction, c_g.created, c.predecessor " +
                   $"FROM data.geometry g left join data.localid_geometry l_g on g.id = l_g.geometry_id, data.codes_geometry c_g, data.codes c, data.dataset d " +
                   $"WHERE ST_Intersects(ST_Transform(g.geography::geometry, 25833), {queryGeometry}) and c_g.geometry_id = g.id and c_g.codes_id = c.id and g.dataset_id = d.id";
        }

        public static string CreateRecursiceJoinForCode(string[] code)
        {
            var recursiveJoin = $"SELECT id from (";
            for (var i = 0; i < code.Length; i++)
            {
                recursiveJoin += $"SELECT r{i}.id from (";
                recursiveJoin +=
                    $"WITH RECURSIVE q{i} AS (SELECT id, code FROM data.codes WHERE code = '{code[i]}' UNION ALL " +
                    $"SELECT m.id, m.code FROM data.codes m JOIN q{i} ON m.predecessor = q{i}.code) SELECT * FROM q{i}";

                recursiveJoin += $") as r{i}";
                if (i < code.Length - 1) recursiveJoin += " UNION ALL ";
            }

            return recursiveJoin + ") as ru";
        }

        public static List<Code> Execute(NpgsqlCommand cmd)
        {
            var dr = cmd.ExecuteReader();

            var results = Codes.ReadResults(dr);

            cmd.Connection?.Close();

            return results;
        }

        public static List<Code> GetIntersectingCodes(string queryGeometry)
        {
            return Execute(GetCmd(CreateIntersectQuery(queryGeometry)));
        }

        public static List<Code> GetIntersectingCodesUtm(string queryGeometry)
        {
            return Execute(GetCmd(CreateIntersectQueryUtm(queryGeometry)));
        }

        internal static List<Code> GetIntersectingCodes(string queryGeometry, string[] codes)
        {
            return Execute(GetCmd(CreateIntersectQuery(queryGeometry) + $" and c.id in ({CreateRecursiceJoinForCode(codes)})"));
        }

        internal static List<Code> GetIntersectingCodesUtm(string queryGeometry, string[] codes)
        {
            return Execute(GetCmd(CreateIntersectQueryUtm(queryGeometry) + $" and c.id in ({CreateRecursiceJoinForCode(codes)})"));
        }

        private static string Fnuttify(string prefix)
        {
            return "'" + prefix.Replace(",", "','").TrimEnd('\'').TrimEnd(',') + "'";
        }

        public static string CreatePoint(double x, double y)
        {
            return $"ST_GeomFromText(\'POINT({x} {y})\', 4326)";
        }

        public static string CreateRadius(double x, double y, double radius)
        {
            return $"ST_Buffer(ST_Transform(ST_GeomFromText(\'POINT({x} {y})\', 4326), 25833), {radius}, 'quad_segs=8')";
        }

        public static string CreateArea(double minx, double miny, double maxx, double maxy)
        {
            return $"ST_MakeEnvelope({minx},{miny},{maxx},{maxy}, 4326)";
        }
    }
}