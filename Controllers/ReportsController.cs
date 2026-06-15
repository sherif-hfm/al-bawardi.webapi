using janaez.webapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace janaez.webapi.Controllers
{
    [Route("reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private static readonly string[] GregorianMonthNames = new[]
        {
            "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        private static readonly string[] HijriMonthNames = new[]
        {
            "مُحرّم", "صفر", "ربيع الأول", "ربيع الثاني", "جُمادى الأولى", "جُمادى الآخرة",
            "رجب", "شعبان", "رمضان", "شوال", "ذو القعدة", "ذو الحجة"
        };

        public ReportsController(AppDbContext _appDbContext)
        {
            appDbContext = _appDbContext;
        }

        [HttpGet("monthlyReport/{year}")]
        [Authorize]
        public async Task<IActionResult> MonthlyReport(int year, [FromQuery] string calendar = "gregorian")
        {
            var calendarType = calendar?.Trim().ToLowerInvariant();

            try
            {
                return calendarType switch
                {
                    "gregorian" => Ok(await GetGregorianMonthlyReport(year)),
                    "hijri" or "umalqura" => Ok(await GetHijriMonthlyReport(year)),
                    _ => BadRequest("Invalid calendar. Use gregorian or hijri.")
                };
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<List<MonthlyReportDto>> GetGregorianMonthlyReport(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1);

            var monthCounts = await appDbContext.Funerals
                .Where(f => f.Date >= startDate && f.Date < endDate)
                .GroupBy(f => f.Date.Month)
                .Select(g => new MonthlyReportDto
                {
                    year = year,
                    monthNo = g.Key,
                    man = g.Count(f => f.SexId == "M"),
                    woman = g.Count(f => f.SexId == "F"),
                    boy = g.Count(f => f.SexId == "B"),
                    girl = g.Count(f => f.SexId == "G"),
                    total = g.Count()
                })
                .ToListAsync();

            return BuildMonthlyReport(year, GregorianMonthNames, monthCounts);
        }

        private async Task<List<MonthlyReportDto>> GetHijriMonthlyReport(int year)
        {
            var umAlQuraCalendar = new UmAlQuraCalendar();
            DateTime startDate;
            DateTime endDate;

            try
            {
                startDate = umAlQuraCalendar.ToDateTime(year, 1, 1, 0, 0, 0, 0);
                endDate = startDate.AddDays(umAlQuraCalendar.GetDaysInYear(year));
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentException("Invalid UmAlQura year.");
            }

            var funerals = await appDbContext.Funerals
                .Where(f => f.Date >= startDate && f.Date < endDate)
                .Select(f => new { f.Date, f.SexId })
                .ToListAsync();

            var monthCounts = funerals
                .GroupBy(f => umAlQuraCalendar.GetMonth(f.Date))
                .Select(g => new MonthlyReportDto
                {
                    year = year,
                    monthNo = g.Key,
                    man = g.Count(f => f.SexId == "M"),
                    woman = g.Count(f => f.SexId == "F"),
                    boy = g.Count(f => f.SexId == "B"),
                    girl = g.Count(f => f.SexId == "G"),
                    total = g.Count()
                })
                .ToList();

            return BuildMonthlyReport(year, HijriMonthNames, monthCounts);
        }

        private static List<MonthlyReportDto> BuildMonthlyReport(
            int year,
            string[] monthNames,
            List<MonthlyReportDto> monthCounts)
        {
            var countsByMonth = monthCounts.ToDictionary(m => m.monthNo);

            return Enumerable.Range(1, 12)
                .Select(monthNo =>
                {
                    countsByMonth.TryGetValue(monthNo, out var counts);

                    return new MonthlyReportDto
                    {
                        year = year,
                        monthNo = monthNo,
                        month = monthNames[monthNo - 1],
                        man = counts?.man ?? 0,
                        woman = counts?.woman ?? 0,
                        boy = counts?.boy ?? 0,
                        girl = counts?.girl ?? 0,
                        total = counts?.total ?? 0
                    };
                })
                .ToList();
        }
    }
}
