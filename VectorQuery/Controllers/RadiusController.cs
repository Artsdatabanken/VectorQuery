using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VectorQuery.Data;

namespace VectorQuery.Controllers
{
    [Route("ogapi/[controller]")]
    [ApiController]
    public class RadiusController : ControllerBase
    {
        [HttpGet("{x}/{y}/{radius}")]
        public Dictionary<string, Code> Get(double x, double y, double radius)
        {
            return Sql.GetIntersectingCodesUtm(Sql.CreateRadius(x, y, radius));
        }

        [HttpGet("{x}/{y}/{radius}/{codes}")]
        public Dictionary<string, Code> Get(double x, double y, double radius, string codes)
        {
            return Sql.GetIntersectingCodesUtm(Sql.CreateRadius(x, y, radius), codes.Split(','));
        }
    }
}
