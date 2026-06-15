using Elfie.Serialization;
using Humanizer;
using janaez.webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace janaez.webapi.Controllers
{
    [Route("funeral")]
    [ApiController]
    public class FuneralController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;
        private readonly AppDbContext appDbContext;
        private string[] formats = new[] { "yyyy-M-d", "yyyy-MM-dd" };
        private readonly ITimeZoneService _timeZoneService;
        public FuneralController(ILogger<HomeController> _logger, AppDbContext _appDbContext, ITimeZoneService timeZoneService)
        {
            logger = _logger;
            appDbContext = _appDbContext;
            _timeZoneService = timeZoneService;
        }

        [HttpGet()]
        [Route("")]
        public IActionResult GetTodayFuneral()
        {
            var sqlQuery = appDbContext.SqlQueries.First(s => s.Key == "TodayFuneral");
            //string date = "2024-01-02";
            string date = _timeZoneService.GetCurrentArabTime().ToString("yyyy-MM-dd");
            string sql = string.Format(sqlQuery.Value, date);
            var newsql = FormattableStringFactory.Create(sql);
            var summaries = appDbContext.Database
            .SqlQuery<TodayFuneral>(newsql)
            .ToList();
            return Ok(summaries);
        }

        [HttpGet()]
        [Route("today-funeral-v2")]
        public async Task<IActionResult> GetTodayFuneralV2()
        {
            try
            {
                var today = _timeZoneService.GetCurrentArabTime().Date;

                var grouped = await appDbContext.Funerals
                    .Include(f => f.Prayer)
                    .Where(f => f.Date == today)
                    .GroupBy(f => new { f.PrayerId, f.Prayer.prayerName })
                    .Select(g => new
                    {
                        g.Key.PrayerId,
                        PrayerName = g.Key.prayerName,
                        Men = g.Count(x => x.SexId == "M"),
                        Women = g.Count(x => x.SexId == "F"),
                        Children = g.Count(x => x.SexId != "M" && x.SexId != "F")
                    })
                    .OrderBy(x => x.PrayerId)
                    .ToListAsync();

                var result = grouped.Select(g => new TodayFuneralV2Dto
                {
                    prayerId = g.PrayerId,
                    prayerName = g.PrayerName,
                    men = g.Men > 0 ? g.Men.ToString() : null,
                    women = g.Women > 0 ? g.Women.ToString() : null,
                    children = g.Children > 0 ? g.Children.ToString() : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting today funeral v2");
                return StatusCode(500, "Internal Server Error");
            }
        }


        /// <summary>
        /// Gets today's funerals in detail. Does not require authorization.
        /// </summary>
        [HttpGet("today-details")]
        public async Task<IActionResult> GetTodayFuneralDetails()
        {
            try
            {
                var today = _timeZoneService.GetCurrentArabTime().Date;
                var results = await appDbContext.Funerals
                    .Include(f => f.Prayer)
                    .Include(f => f.PurialPlace)
                    .Include(f => f.Sex)
                    .Where(f => f.Date == today)
                    .Select(f => new TodayFuneralDetailDto
                    {
                        DeadName = f.DeadName,
                        prayerId = f.PrayerId,
                        prayerName = f.Prayer.prayerName,
                        placeId = f.PurialPlaceId,
                        placeName = f.PurialPlace.placeName,
                        SexId = f.SexId,
                        SexName = f.Sex.Name,
                        AmbulanceNo = f.AmbulanceNo,
                        GraveNo = f.GraveNo,
                        Notes = f.Notes
                    })
                    .ToListAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting today funeral details");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet()]
        [Route("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var now = _timeZoneService.GetCurrentArabTime().Date;
            var yesterday = now.AddDays(-1);
            var last7Days = now.AddDays(-7);
            var lastMonth = now.AddDays(-30);
            var gregorianYearStart = new DateTime(now.Year, 1, 1);
            var diffDays = (now - gregorianYearStart).Days;

            // Hijri (UmAlQura) conversion
            var umAlQura = new UmAlQuraCalendar(); // Or use Xceed.UmAlQura if you prefer
            int hijriYear = umAlQura.GetYear(now);
            DateTime hijriYearStart = umAlQura.ToDateTime(hijriYear, 1, 1, 0, 0, 0, 0);
            int diffDaysHijri = (now - hijriYearStart).Days;

            var todayCount = await appDbContext.Funerals
                .CountAsync(f => f.Date == now);

            var yesterdayCount = await appDbContext.Funerals
                .CountAsync(f => f.Date == yesterday);

            var last7DaysCount = await appDbContext.Funerals
                .CountAsync(f => f.Date >= last7Days && f.Date <= now);

            var lastMonthCount = await appDbContext.Funerals
                .CountAsync(f => f.Date >= lastMonth && f.Date <= now);

            var lastYearCount = await appDbContext.Funerals
                .CountAsync(f => f.Date >= gregorianYearStart && f.Date <= now);

            var lastYearHijriCount = await appDbContext.Funerals
                .CountAsync(f => f.Date >= hijriYearStart && f.Date <= now);

            return Ok(new
            {
                today = todayCount,
                yesterday = yesterdayCount,
                last7Days = last7DaysCount,
                lastMonth = lastMonthCount,
                diffDays = diffDays,
                diffDaysHijiry = diffDaysHijri,
                lastYear = lastYearCount,
                lastYearHijiry = lastYearHijriCount
            });
        }

        [Authorize]
        [HttpGet("getDetails/{date}")]
        public async Task<IActionResult> GetDetails(string date)
        {
            try
            {
                // Parse the date string to DateTime (format "yyyy-MM-dd")
                

                if (!DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var qDate))
                {
                    return BadRequest("Invalid date format. Use yyyy-MM-dd or yyyy-M-d.");
                }

                var results = await appDbContext.Funerals
                    .Include(f => f.Prayer)
                    .Include(f => f.PurialPlace)
                    .Include(f => f.Sex)
                    .Where(f => f.Date == qDate)
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("getDetail/{id}")]
        [Authorize]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var funeral = await appDbContext.Funerals
                    .Include(f => f.Prayer)
                    .Include(f => f.PurialPlace)
                    .Include(f => f.Sex)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (funeral == null)
                    return NotFound();

                return Ok(funeral);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpGet("getDayDetail/{date}/{prayerId}")]
        public async Task<IActionResult> GetDayDetail(string date, int prayerId)
        {
            try
            {
                if (!DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var qDate))
                {
                    return BadRequest("Invalid date format. Use yyyy-MM-dd or yyyy-M-d.");
                }


                var funerals = await appDbContext.Funerals
                    .Include(f => f.Prayer)
                    .Include(f => f.PurialPlace)
                    .Include(f => f.Sex)
                    .Where(f => f.Date == qDate && f.PrayerId == prayerId)
                    .ToListAsync();

                return Ok(funerals);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }


        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddFuneral([FromBody] FuneralCreateDto dto)
        {

            if (!DateTime.TryParseExact(dto.Date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var funeralDate))
            {
                return BadRequest("Invalid date format. Use yyyy-MM-dd or yyyy-M-d.");
            }

            var funeral = new Funeral
            {
                DeadName = dto.DeadName,
                Date = funeralDate,
                SexId = dto.Sex,
                PrayerId = dto.PrayerId,
                PurialPlaceId = dto.PlaceId,
                AmbulanceNo = dto.AmbulanceNo,
                GraveNo = dto.GraveNo,
                Notes = dto.Notes,
                CreatedAt = DateTime.Now,
            };

            try
            {
                appDbContext.Funerals.Add(funeral);
                await appDbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> UpdateFuneral([FromBody] FuneralUpdateDto dto)
        {
            if (!DateTime.TryParseExact(dto.Date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var funeralDate))
            {
                return BadRequest("Invalid date format. Use yyyy-MM-dd or yyyy-M-d.");
            }

            var funeral = await appDbContext.Funerals.FindAsync(dto.Id);
            if (funeral == null)
                return NotFound();

            funeral.DeadName = dto.DeadName;
            funeral.Date = funeralDate;
            funeral.SexId = dto.Sex;
            funeral.PrayerId = dto.PrayerId;
            funeral.PurialPlaceId = dto.PlaceId;
            funeral.AmbulanceNo = dto.AmbulanceNo;
            funeral.GraveNo = dto.GraveNo;
            funeral.Notes = dto.Notes;
            funeral.UpdatedAt = DateTime.Now;

            try
            {
                await appDbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpDelete("del/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFuneral(int id)
        {
            var funeral = await appDbContext.Funerals.FindAsync(id);
            if (funeral == null)
                return NotFound();

            try
            {
                appDbContext.Funerals.Remove(funeral);
                await appDbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpGet("prayers")]
        public async Task<IActionResult> GetPrayers()
        {
            try
            {
                var prayers = await appDbContext.Prayers.ToListAsync();
                return Ok(prayers);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpGet("sexs")]
        public async Task<IActionResult> GetSexs()
        {
            try
            {
                var sexes = await appDbContext.Sexes.ToListAsync();
                return Ok(sexes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpGet("places")]
        public async Task<IActionResult> GetPlaces()
        {
            try
            {
                var places = await appDbContext.PurialPlaces.ToListAsync();
                return Ok(places);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
        }
    }
}