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
        /// Get replacement aplications for the shift specified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Id of the shift</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement applications fetched successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to fetch replacement applications for non existing shift.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [Authorize]
        [HttpGet("{id}/replacement")]
        public ActionResult Get(int id)
        {
            _context.Shift.AsNoTracking().Load();
            return GetReplacementApplications(_context.Shift.FirstOrDefault(s => s.Id == id));
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

            // check that user is logged in
            var currUser = GetCurrentUser();
            if (currUser == default(User)) return 401.ErrorStatusCode();

            // check that the current user is admin
            if (!currUser.IsAdmin) return 403.ErrorStatusCode();

            // check that user in model exists
            _context.Users.Load();
            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId);
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
                UserId = model.UserId,
                User = user
            };
            _context.Shift.Add(shift);
            _context.SaveChanges();
            return shift.ToJson(201);
        }

        private ActionResult GetReplacementApplications(Shift shift)
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
