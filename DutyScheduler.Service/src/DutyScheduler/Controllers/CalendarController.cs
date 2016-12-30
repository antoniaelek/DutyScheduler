using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DutyScheduler.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DutyScheduler.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            //var holidays = new HolidayCalculator(DateTime.Today, Utils.ReadConfig("Holidays","Path"));
            //var list = new List<string>();
            //foreach (HolidayCalculator.Holiday h in holidays.OrderedHolidays)
            //{
            //    list.Add(h.Name + " - " + h.Date.ToString("D"));
            //}
            //return list;
            var month = new Month();
            var list = new List<string>(month.Last.Day);
            for (var i = 1; i <= month.Last.Day; i++)
            {
                var date = new DateTime(month.First.Year, month.First.Month, i);

                var holiday = month.Holidays.FirstOrDefault(h => h.Date == date);
                var specialDay = month.SpecialDays.FirstOrDefault(s => s.Date == date);

                if (holiday != default(Holiday)) list.Add(holiday.Date + " " + holiday.Name);
                else if (specialDay != default(SpecialDay)) list.Add(specialDay.Date + " " + specialDay.Name);
                else list.Add(date.ToString());
            }
            return list;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
