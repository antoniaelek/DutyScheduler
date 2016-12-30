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
        [HttpGet]
        public ActionResult Get()
        {
            var days = GetMonth(new Month());
            return DaysToJson(days);
        }

        [HttpGet("year={year}&month={month}")]
        public ActionResult Get(int year, int month)
        {
            if (month < 1 || month > 12)
                return 404.ErrorStatusCode(new KeyValuePair<string, string>("month", "month must be a value between 1 and 12"));
            var days = GetMonth(new Month(new DateTime(year, month, 1)));
            return DaysToJson(days);
        }

        private static ActionResult DaysToJson(IEnumerable<IDay> days)
        {
            return new JsonResult(days.Select(d => new ViewModels.DayViewModel()
            {
                Type = d.Type,
                Date = d.Date.ToString("d"),
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
