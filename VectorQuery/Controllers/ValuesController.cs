using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace VectorQuery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static readonly Dictionary<string, string> CodesDictionary = new Dictionary<string, string>();

        public ValuesController()
        {
            var cmdCodes = GetCmd("SELECT code, title from data.codes c");

            var dr = cmdCodes.ExecuteReader();

            while (dr.Read())
            {
                CodesDictionary[dr[0].ToString()] = dr[1].ToString();
            }

            cmdCodes.Connection?.Close();
        }

       private static NpgsqlCommand GetCmd(string sql)
        {
            var conn = new NpgsqlConnection(
                "Server=bigbadabom;User Id=reader;Password=reader;Database=bigbadabom");

            conn.Open();

            return new NpgsqlCommand(sql, conn);
        }

        [HttpGet("{x}/{y}")]
        public Dictionary<string, Code> Get(double x, double y)
        {
            var cmd = GetCmd($"SELECT c.code, c.title FROM data.geometry g, data.codes_geometry c_g, data.codes c WHERE ST_Intersects(g.geography, ST_GeomFromText(\'POINT({x} {y})\', 4326)) and c_g.geometry_id = g.id and c_g.codes_id = c.id");

            var dr = cmd.ExecuteReader();

            var results = ReadResults(dr);

            cmd.Connection?.Close();

            return results;
        }

        private static Dictionary<string, Code> ReadResults(NpgsqlDataReader dr)
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

                results[code] = new Code
                {
                    Value = dr[1].ToString(),
                    Key =CodesDictionary[parentCode]
                };
            }
            
            return results;
        }
    }

    public class Code
    {
        public string Key;
        public string Value;
    }
}
