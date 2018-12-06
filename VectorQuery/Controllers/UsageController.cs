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
                {"ogapi/Codes/x/y/codes", "Returns codes and geometries for a point in wgs84 coordinates by given codes"},
                {"ogapi/Bbox/minx/miny/maxx/maxy", "Returns codes and geometries for a bbox in wgs84 coordinates"},
                {"ogapi/Bbox/minx/miny/maxx/maxy/codes", "Returns codes and geometries for a bbox in wgs84 coordinates by given codes and radius"},
                {"ogapi/Radius/x/y/radius", "Returns codes and geometries for a point in wgs84 coordinates"},
                {"ogapi/Radius/x/y/radius/codes", "Returns codes and geometries for a point in wgs84 coordinates by given codes and radius"},
            };
        }
    }
}
