using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public class ShiftController : Controller
    {
        private static readonly string DateFormat = "yyyy-MM-dd";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ShiftController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        /// <summary>
        /// Create a new shift.
        /// </summary>
        /// <param name="model">Shift model</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created, "Shift created successfully.")]
        [SwaggerResponse(HttpStatusCode.NotModified, "Model was empty, nothing happened.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User is not an admin.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to set shift for a past, or invalid date, or non existing user.")]
        [Authorize]
        [HttpPost]
        public ActionResult Post([FromBody] ShiftViewModel model)
        {
            if (model == default(ShiftViewModel)) return NoContent();
            return CreateShift(model);
        }

        /// <summary>
        /// Set whether the shift specified by <paramref name="id"/> is repleceable.
        /// </summary>
        /// <param name="id">Id of the shift</param>
        /// <param name="model">Model</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Setting successfully saved.")]
        [SwaggerResponse(HttpStatusCode.NotModified, "Model was empty, nothing happened.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [Authorize]
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody]SetReplacementViewModel model)
        {
            if (model == default(SetReplacementViewModel)) return NoContent();
            return SetReplaceable(_context.Shift.FirstOrDefault(s => s.Id == id), model.SetReplaceable);
        }

        /// <summary>
        /// Get replacement requests for the shift specified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Id of the shift</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement requests fetched successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to fetch replacement requests for non existing shift.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [Authorize]
        [HttpGet("{id}/replacement")]
        public ActionResult Get(int id)
        {
            _context.Shift.Include(s=>s.User).AsNoTracking().Load();
            return GetReplacementRequests(_context.Shift.FirstOrDefault(s => s.Id == id));
        }


        /// <summary>
        /// Get current month's shifts for user specified by <paramref name="username"/>.
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Shifts fetched successfully.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to fetch shifts for non existing user.")]
        [AllowAnonymous]
        [HttpGet("user/{username}")]
        public ActionResult GetCurrentShifts(string username)
        {           
            return GetUserShiftsInMonth(username, DateTime.Now);
        }

        /// <summary>
        /// Get shifts for user specified by <paramref name="username"/> and <paramref name="month"/>.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="month">Month</param>
        /// <param name="year">Year</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement requests fetched successfully.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to fetch shifts for invaid month.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to fetch shifts for non existing user.")]
        [AllowAnonymous]
        [HttpGet("user/username={username}&month={month}&year={year}")]
        public ActionResult GetShifts(string username, int month, int year)
        {
            var date = year + "-" + month + "-1";
            if (!date.ValidateDate()) return 400.ErrorStatusCode(new Dictionary<string, string>() { { "date", "Invalid date" } });

            return GetUserShiftsInMonth(username, new DateTime(year, month, 1));
        }

        private ActionResult CreateShift(ShiftViewModel model)
        {
            if (!model.Date.ValidateDate())
                return 400.ErrorStatusCode(new Dictionary<string, string>() { { "date", "Invalid date" } });

            var date = DateTime.Parse(model.Date);

            // check if date in past
            if (date < DateTime.Today)
                return
                    400.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"date", "Unable to create shift for past dates."}
                    });

            // check if date is holiday
            var month = new Month(date);
            if (month.Holidays.Select(d=>d.Date).Contains(date))
                return
                    400.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"date", "Unable to create shift on a holiday."}
                    });

            // check that user is logged in
            var currUser = GetCurrentUser();
            if (currUser == default(User)) return 401.ErrorStatusCode();

            // check that the current user is admin
            if (!currUser.IsAdmin) return 403.ErrorStatusCode();

            // check that user in model exists
            _context.Users.Load();
            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserName);
            if (user == default(User))
                return
                    404.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"user", "User with the specified Id not found"}
                    });

            // check that the shift does not exist
            _context.Shift.Include(s => s.User).Load();
            var shift = _context.Shift.FirstOrDefault(s => s.Date == date);
            if (shift != default(Shift))
                return
                    400.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"date", "Shift for that date already exists"}
                    });


            shift = new Shift()
            {
                Date = date,
                IsRepleceable = model.IsReplaceable,
                UserId = model.UserName,
                User = user
            };
            _context.Shift.Add(shift);
            _context.SaveChanges();
            return shift.ToJson(201);
        }

        private ActionResult GetReplacementRequests(Shift shift)
        {
            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode();

            // check that shift exists and the current user can modify it
            _context.Shift.Include(s => s.User).Load();
            if (shift == default(Shift)) return 404.ErrorStatusCode();
            if (user.Id != shift.UserId && !user.IsAdmin) return 403.ErrorStatusCode();

            _context.ReplacementRequest.Include(r=>r.User).Load();
            var requests = _context.ReplacementRequest.Where(r => r.ShiftId == shift.Id).ToList();

            return shift.ToJson(requests);
        }

        private ActionResult GetUserShiftsInMonth(string username, DateTime date)
        {
            _context.Users.AsNoTracking().Load();
            if (_context.Users.FirstOrDefault(u => u.UserName == username) == default(User))
                return 404.ErrorStatusCode(new Dictionary<string, string>() {{"user", "Invalid username"}});

            _context.Shift.Include(s => s.User).AsNoTracking().Load();
            IEnumerable<Shift> ret = _context.Shift.Where(s => s.UserId == username && 
                                                s.Date.Month == date.Month && 
                                                s.Date.Year == date.Year)
												.OrderBy(s => s.Date);
            if (ret == null) ret = new List<Shift>();
            return ret.ToJson();
        }

        private ActionResult SetReplaceable(Shift entry, bool setReplaceable)
        {
            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode();

            // check that shift exists and the current user can modify it
            _context.Shift.Include(s => s.User).Load();
            if (entry == default(Shift)) return 404.ErrorStatusCode();
            if (user.Id != entry.UserId && !user.IsAdmin) return 403.ErrorStatusCode();

            // check if date in past
            if (entry.Date < DateTime.Today)
                return 400.ErrorStatusCode(
                    new Dictionary<string, string>() { { "date", "Unable to change settings for past dates." } }
                );


            entry.IsRepleceable = setReplaceable;

            if (!setReplaceable)
            {
                // delete associated replacement requests 
                _context.ReplacementRequest.Include(r => r.Shift).Include(r => r.User).Load();
                var requests = _context.ReplacementRequest.Where(r => r.Shift.Id == entry.Id);
                _context.RemoveRange(requests);
            }

            _context.SaveChanges();
            return entry.ToJson();
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
