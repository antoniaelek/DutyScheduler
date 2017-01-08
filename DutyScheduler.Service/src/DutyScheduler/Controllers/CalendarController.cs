using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using DutyScheduler.Models;
using DutyScheduler.Helpers;
using DutyScheduler.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.SwaggerGen.Annotations;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DutyScheduler.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        private static readonly string DateFormat = "yyyy-MM-dd";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CalendarController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets the calendar for the current month.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Calendar returned successfully.")]
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Get()
        {
            var days = GetMonth(new Month());
            return DaysToJson(days);
        }

        /// <summary>
        /// Gets the calendar for the specified month and year.
        /// </summary>
        /// <param name="year">Year part of date</param>
        /// <param name="month">Month part of date</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Calendar returned successfully.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Invalid month or year.")]
        [AllowAnonymous]
        [HttpGet("year={year}&month={month}")]
        public ActionResult Get(int year, int month)
        {
            if (month < 1 || month > 12)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("month", "month must be a value between 1 and 12");
                if (year < 0) dict.Add("year", "year must be a value greater than 0");
                return 404.ErrorStatusCode(dict);
            }
            var days = GetMonth(new Month(new DateTime(year, month, 1)));
            return DaysToJson(days);
        }

        private static ActionResult DaysToJson(IEnumerable<Day> days)
        {
            return new JsonResult(days.Select(d => new DayViewModel()
            {
                Date = d.Date.ToString(DateFormat),
                Weekday = d.WeekDay,
                Type = d.Type,
                Name = d.Name,
                IsReplaceable = d.IsReplaceable,
                Scheduled = d.Scheduled,
                IsPrefered = d.IsPrefered
            }));
        }

        private static IEnumerable<Day> GetMonth(Month month)
        {
            var list = new List<Day>(month.Last.Day);
            for (var i = 1; i <= month.Last.Day; i++)
            {
                var date = new DateTime(month.First.Year, month.First.Month, i);

                var holiday = month.Holidays.FirstOrDefault(h => h.Date == date);
                var specialDay = month.SpecialDays.FirstOrDefault(s => s.Date == date);
                var nonWorkingDay = month.NonWorkingDays.FirstOrDefault(s => s.Date == date);

                if (holiday != default(Holiday)) list.Add(holiday);
                else if (specialDay != default(SpecialDay)) list.Add(specialDay);
                else if (nonWorkingDay != default(NonWorkingDay)) list.Add(nonWorkingDay);
                else list.Add(new OrdinaryDay(date));
            }
            return list;
        }
    }
}
