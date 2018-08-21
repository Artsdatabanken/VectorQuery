using System.Collections.Generic;
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
            return Sql.GetIntersectingCodes(Sql.CreatePoint(x, y));
        }

        [HttpGet("{x}/{y}/{prefix}")]
        public Dictionary<string, Code> Get(double x, double y, string prefix)
        {
            return Sql.GetIntersectingCodes(Sql.CreatePoint(x, y), prefix);
        }

        [HttpGet("{minx}/{miny}/{maxx}/{maxy}")]
        public Dictionary<string, Code> Get(double minx, double miny, double maxx, double maxy)
        {
            return Sql.GetIntersectingCodes(Sql.CreateArea(minx, miny, maxx, maxy));
        }

        [HttpGet("{minx}/{miny}/{maxx}/{maxy}/{prefix}")]
        public Dictionary<string, Code> Get(double minx, double miny, double maxx, double maxy, string prefix)
        {
            return Sql.GetIntersectingCodes(Sql.CreateArea(minx, miny, maxx, maxy), prefix);
        }
    }
}
