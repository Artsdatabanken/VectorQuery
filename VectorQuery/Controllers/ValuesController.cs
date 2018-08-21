using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using VectorQuery.Data;

namespace VectorQuery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("{x}/{y}")]
        public Dictionary<string, Code> Get(double x, double y)
        {
            var cmd = Sql.GetCmd(
                $"SELECT c.code, c.title FROM data.geometry g, data.codes_geometry c_g, data.codes c WHERE ST_Intersects(g.geography, ST_GeomFromText(\'POINT({x} {y})\', 4326)) and c_g.geometry_id = g.id and c_g.codes_id = c.id");

            var dr = cmd.ExecuteReader();

            var results = ReadResults(dr);

            cmd.Connection?.Close();

            return results;
        }

        private static Dictionary<string, Code> ReadResults(IDataReader dr)
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
                    Key = Codes.Dictionary[parentCode]
                };
            }

            return results;
        }
    }
}
