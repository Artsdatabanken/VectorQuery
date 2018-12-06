using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VectorQuery.Data;

namespace VectorQuery.Controllers
{
    [Route("ogapi/[controller]")]
    [ApiController]
    public class BboxController : ControllerBase
    {
        [HttpGet("{minx}/{miny}/{maxx}/{maxy}")]
        public List<Code> Get(double minx, double miny, double maxx, double maxy)
        {
            return Sql.GetIntersectingCodes(Sql.CreateArea(minx, miny, maxx, maxy));
        }

        [HttpGet("{minx}/{miny}/{maxx}/{maxy}/{codes}")]
        public List<Code> Get(double minx, double miny, double maxx, double maxy, string codes)
        {
            return Sql.GetIntersectingCodes(Sql.CreateArea(minx, miny, maxx, maxy), codes.Split(','));
        }
    }
}
