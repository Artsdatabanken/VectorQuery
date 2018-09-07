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
            return $"SELECT c.code, c.title, l_g.localid, g.id, c_g.fraction FROM data.geometry g left join data.localid_geometry l_g on g.id = l_g.geometry_id, data.codes_geometry c_g, data.codes c, data.dataset d, data.prefix p WHERE ST_Intersects(g.geography, {queryGeometry}) and c_g.geometry_id = g.id and c_g.codes_id = c.id and g.dataset_id = d.id and d.prefix_id = p.id";
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
            return Execute(GetCmd(CreateIntersectQuery(queryGeometry)));
        }

        internal static Dictionary<string, Code> GetIntersectingCodes(string queryGeometry, string prefix)
        {
            return Execute(GetCmd(CreateIntersectQuery(queryGeometry) + $" and p.value in ({Fnuttify(prefix)})"));
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