using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DutyScheduler.Helpers;
using DutyScheduler.Models;
using DutyScheduler.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.SwaggerGen.Annotations;

namespace DutyScheduler.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class StatistcsController : Controller
    {
        private static readonly string DateFormat = "yyyy-MM-dd";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public StatistcsController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [SwaggerResponse(HttpStatusCode.OK, "Success.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [Authorize]
        [HttpGet]
        public ActionResult Get()
        {
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode();

            return GetUserStatistics(DateTime.Now.Year, user.UserName);
        }

        private ActionResult GetUserStatistics(int year, string username)
        {
            _context.Shift.Include(s => s.User).AsNoTracking().Load();
            _context.ReplacementHistory.Include(h => h.ReplacedUser).Include(h => h.ReplacingUser).Load();

            ICollection<UserStatisticsViewModel> stats = new List<UserStatisticsViewModel>();
            
            // Get past shifts
            var shifts = _context.Shift.Where(s => s.UserId == username && s.Date.Year == year);

            // replacing
            var replacing = _context.ReplacementHistory.Where(h => h.ReplacingUserId == username && h.Date.Year == year);
            
            // replaced
            var replaced = _context.ReplacementHistory.Where(h => h.ReplacedUserId == username && h.Date.Year == year);

            var serializedShifts = shifts.Select(s => new {Date = s.Date.ToString(DateFormat), GetDay(s.Date).Type});

            var serializedReplacing =
                replacing.Select(
                    r =>
                        new
                        {
                            Date = r.Date.ToString(DateFormat),
                            GetDay(r.Date).Type,
                            r.ReplacingUserId,
                            replacingUser = r.ReplacingUser.SerializeUser()
                        });
            var serializedReplaced =
                replaced.Select(
                    r =>
                        new
                        {
                            Date = r.Date.ToString(DateFormat),
                            GetDay(r.Date).Type,
                            r.ReplacedUserId,
                            replacedUser = r.ReplacedUser.SerializeUser()
                        });

            return new JsonResult(new
            {
                shifts = serializedShifts,
                replacing = serializedReplacing,
                replaced = serializedReplaced
            });
        }

        private Day GetDay(DateTime date)
        {
            var month = new Month(date);
            var holiday = month.Holidays.FirstOrDefault(d => d.Date == date);
            var specialDay = month.SpecialDays.FirstOrDefault(d => d.Date == date);
            var nonWorkingDay = month.NonWorkingDays.FirstOrDefault(d => d.Date == date);

            if (holiday != default(Holiday)) return holiday;
            if (specialDay != default(SpecialDay)) return specialDay;
            if (nonWorkingDay != default(NonWorkingDay)) return nonWorkingDay;
            return new OrdinaryDay(date);
        }

        private User GetCurrentUser()
        {
            _context.Users.AsNoTracking().Load();
            var user = _userManager.GetUserId(User);
            if (user == null) return null;
            var userObj = _context.Users.FirstOrDefault(u => u.Id == user);
            if (userObj == default(User)) return null;
            return userObj;
        }
    }
}
