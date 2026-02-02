using janaez.webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace janaez.webapi.Controllers
{
    
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;

        
        private readonly IWebHostEnvironment _env;

        private readonly BasicAuthSettings appSettings;

        private readonly ITimeZoneService _timeZoneService;

        public HomeController(ILogger<HomeController> _logger, IWebHostEnvironment env, IOptions<BasicAuthSettings> _appSettings, ITimeZoneService timeZoneService)
        {
            logger = _logger;
            _env = env;
            appSettings = _appSettings.Value;
            _timeZoneService = timeZoneService;
        }

        [HttpGet()]
        [Route("/_info")]
        public IActionResult Info()
        {
            return Ok(new { time = _timeZoneService.GetCurrentArabTime(), environment = _env.EnvironmentName, buildDate = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location) });
        }
    }
}
