using Dapper;
using janaez.webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace janaez.webapi.Controllers
{
    [Route("reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger<ReportsController> logger;
        private readonly AppDbContext appDbContext;
        private string[] formats = new[] { "yyyy-M-d", "yyyy-MM-dd" };

        public ReportsController(ILogger<ReportsController> _logger, AppDbContext _appDbContext)
        {
            logger = _logger;
            appDbContext = _appDbContext;
        }

        [HttpGet("monthlyReport/{year}")]
        [Authorize]
        public IActionResult MonthlyReport(int year)
        {
            using var conn = appDbContext.Database.GetDbConnection();
            var sqlQuery = appDbContext.SqlQueries.First(s => s.Key == "MonthlyReport");
            conn.Open();
            var results = conn.Query(sqlQuery.Value, new { year = year }); // Dapper
            return Ok(results);
        }
    }
}
