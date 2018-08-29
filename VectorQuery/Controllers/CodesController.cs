using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VectorQuery.Data;

namespace VectorQuery.Controllers
{
    [Route("ogapi/[controller]")]
    [ApiController]
    public class CodesController : ControllerBase
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
    }
}
