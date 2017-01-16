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
    public class StatisticsController : Controller
    {
        private static readonly string DateFormat = "yyyy-MM-dd";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public StatisticsController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        /// <summary>
        /// Get statistics for logged in user and and current year.
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, "Success.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [Authorize]
        [HttpGet]
        public ActionResult Get()
        {
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            return GetUserStatistics(DateTime.Now.Year, user.UserName);
        }

        /// <summary>
        /// Get statistics for <paramref name="username"/> and current year.
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Success.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [Authorize]
        [HttpGet("{username}")]
        public ActionResult Get(string username)
        {
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());
            
            if (!user.IsAdmin && user.UserName != username) return 403.ErrorStatusCode(Constants.Forbidden.ToDict());

            return GetUserStatistics(DateTime.Now.Year, username);
        }

        /// <summary>
        /// Get statistics for logged in user and <paramref name="year"/>.
        /// </summary>
        /// <param name="year">Year</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Success.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Invalid  year.")]
        [Authorize]
        [HttpGet("year={year}")]
        public ActionResult Get(int year)
        {
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            return GetUserStatistics(year, user.UserName);
        }

        /// <summary>
        /// Get statistics for <paramref name="username"/> and <paramref name="year"/>.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="year">Year</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Success.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Invalid  year.")]
        [Authorize]
        [HttpGet("username={username}&year={year}")]
        public ActionResult Get(string username, int year)
        {
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            if (!user.IsAdmin && user.UserName != username) return 403.ErrorStatusCode(Constants.Forbidden.ToDict());

            return GetUserStatistics(DateTime.Now.Year, username);
        }


        private ActionResult GetUserStatistics(int year, string username)
        {
            if (year < 1 || year > DateTime.Now.Year)
                return 400.ErrorStatusCode(new Dictionary<string, string>() { { "year", "Invalid year specified." } });

            _context.Users.Load();
            var otherUser = _context.Users.FirstOrDefault(u => u.UserName == username);
            if (otherUser == default(User)) return 404.ErrorStatusCode(Constants.UserNotFound);

            _context.Shift.Include(s => s.User).AsNoTracking().Load();
            _context.ReplacementHistory.Include(h => h.ReplacedUser).Include(h => h.ReplacingUser).Load();


            ICollection<UserStatisticsViewModel> stats = new List<UserStatisticsViewModel>();

            var date = DateTime.Now;

            // Get past shifts
            var shifts = _context.Shift.Where(s => s.UserId == username && s.Date.Year == year && s.Date < date);

            // replacing
            var replacing = _context.ReplacementHistory.Where(h => h.ReplacingUserId == username && h.Date.Year == year && h.Date < date);
            
            // replaced
            var replaced = _context.ReplacementHistory.Where(h => h.ReplacedUserId == username && h.Date.Year == year && h.Date < date);

            // serialize
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
