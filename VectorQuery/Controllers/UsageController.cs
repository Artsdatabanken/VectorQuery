using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VectorQuery.Data;

namespace VectorQuery.Controllers
{
    [Route("ogapi/[controller]")]
    [ApiController]
    public class UsageController : ControllerBase
    {
        [HttpGet]
        public Dictionary<string, string> Get()
        {
            return new Dictionary<string, string>
            {
                {"ogapi/Codes/x/y", "Returns codes and geometries for a point in wgs84 coordinates"},
                {"ogapi/Codes/x/y/prefix", "Returns codes and geometries for a point in wgs84 coordinates by given prefix"},
                {"ogapi/Bbox/minx/miny/maxx/maxy", "Returns codes and geometries for a bbox in wgs84 coordinates"},
                {"ogapi/Bbox/minx/miny/maxx/maxy/prefix", "Returns codes and geometries for a bbox in wgs84 coordinates by given prefix"},
            };
        }
    }
}
