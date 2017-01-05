using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DutyScheduler.Models;
using DutyScheduler.Helpers;
using DutyScheduler.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DutyScheduler.Controllers
{
    [Route("api/[controller]")]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CalendarController(ApplicationDbContext context,
            UserManager<User> userManager,
            IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Build();
        }

        /// <summary>
        /// Gets the calendar for the current month.
        /// </summary>
        /// <returns></returns>
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
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <returns></returns>
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

        private static ActionResult DaysToJson(IEnumerable<IDay> days)
        {
            return new JsonResult(days.Select(d => new ViewModels.DayViewModel()
            {
                Type = d.Type,
                Date = d.Date.ToString("d.M.yyyy"),
                Name = d.Name,
                IsReplaceable = d.IsReplaceable,
                Scheduled = d.Scheduled,
                IsPrefered = d.IsPrefered
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
                var nonWorkingDay = month.NonWorkingDays.FirstOrDefault(s => s.Date == date);

                if (holiday != default(Holiday)) list.Add(holiday);
                else if (specialDay != default(SpecialDay)) list.Add(specialDay);
                else if (nonWorkingDay != default(NonWorkingDay)) list.Add(nonWorkingDay);
                else list.Add(new Day(date));
            }
            return list;
        }

        [Authorize]
        [HttpPost("year={year}&month={month}&day={day}")]
        public async void Post(int year, int month, int day, [FromBody]DayPostViewModel model)
        {
            var date = new DateTime(year,month,day);
            if (model == default(DayPostViewModel)) return;

            if (model.SetPrefered != null) await SetPrefered(date, model.SetPrefered.Value);
            if (model.ApplyForReplacement) ApplyForReplacement(date);
            if (model.SetReplaceable) SetReplaceable(date);
        }


        private async Task<User> GetCurrentUser()
        {
            await _context.Users.AsNoTracking().LoadAsync();
            return await _userManager.FindByNameAsync(User.Identity.Name);
        }

        private async Task SetPrefered(DateTime date, bool isPrefered)
        {
            var user = await GetCurrentUser();
            await _context.Preference.LoadAsync();
            var entry = _context.Preference.FirstOrDefault(p => p.Date.Date == date && p.UserId.ToString() == user.Id);
            if (entry != default(Preference))
            {
                entry.IsPreferred = isPrefered;
            }
        }

        private void ApplyForReplacement(DateTime date)
        {
                
        }

        private void SetReplaceable(DateTime date)
        {
            
        }

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
