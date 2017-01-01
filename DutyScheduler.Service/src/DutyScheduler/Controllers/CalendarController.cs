using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DutyScheduler.Models;
using DutyScheduler.Helpers;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DutyScheduler.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        /// <summary>
        /// Gets the calendar for the current month.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Get()
        {
            var days = GetMonth(new Month());
            return DaysToJson(days);
        }

        /// <summary>
        /// Gets the calendar for the specified month and year.
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <returns></returns>
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

        private static ActionResult DaysToJson(IEnumerable<IDay> days)
        {
            return new JsonResult(days.Select(d => new ViewModels.DayViewModel()
            {
                Type = d.Type,
                Date = d.Date.ToString("d.M.yyyy"),
                Name = d.Name,
                Scheduled = d.Scheduled,
                IsReplaceable = d.IsReplaceable
            }));
        }

        private static IEnumerable<IDay> GetMonth(Month month)
        {
            var list = new List<IDay>(month.Last.Day);
            for (var i = 1; i <= month.Last.Day; i++)
            {
                var date = new DateTime(month.First.Year, month.First.Month, i);

                var holiday = month.Holidays.FirstOrDefault(h => h.Date == date);
                var specialDay = month.SpecialDays.FirstOrDefault(s => s.Date == date);

                if (holiday != default(Holiday)) list.Add(holiday);
                else if (specialDay != default(SpecialDay)) list.Add(specialDay);
                else list.Add(new Day(date));
            }
            return list;
        }

        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
