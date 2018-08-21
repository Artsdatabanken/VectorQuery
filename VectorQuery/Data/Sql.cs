using Npgsql;

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
    }
}