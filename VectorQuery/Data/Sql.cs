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
            return $"SELECT g.id FROM data.geometry g WHERE ST_Intersects(g.geography, {queryGeometry})";
        }

        public static string CreateIntersectQueryUtm(string queryGeometry)
        {
            return
                $"SELECT g.id FROM data.geometry g WHERE ST_Intersects(ST_Transform(g.geography::geometry, 25833), {queryGeometry})";
        }


        public static List<Code> Execute(NpgsqlCommand cmd, string[] codes = null)
        {
            var dr = cmd.ExecuteReader();

            var results = codes == null ? Codes.ReadResults(dr) : Codes.ReadResults(dr, codes);

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
            return Execute(GetCmd(CreateIntersectQuery(queryGeometry)), codes);
        }

        internal static List<Code> GetIntersectingCodesUtm(string queryGeometry, string[] codes)
        {
            return Execute(GetCmd(CreateIntersectQueryUtm(queryGeometry)), codes);
        }


        public static string CreatePoint(double x, double y)
        {
            return $"ST_GeomFromText(\'POINT({x} {y})\', 4326)";
        }

        public static string CreateRadius(double x, double y, double radius)
        {
            return
                $"ST_Buffer(ST_Transform(ST_GeomFromText(\'POINT({x} {y})\', 4326), 25833), {radius}, 'quad_segs=8')";
        }

        public static string CreateArea(double minx, double miny, double maxx, double maxy)
        {
            return $"ST_MakeEnvelope({minx},{miny},{maxx},{maxy}, 4326)";
        }
    }
}