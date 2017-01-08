using System;
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
    [Route("api/[controller]")]
    public class ReplacementController : Controller
    {
        private static readonly string DateFormat = "yyyy-MM-dd";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReplacementController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Request replacement.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created, "Request successfully saved.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to send request for own shift, or a non replaceable shift, or the shift that already has identical request by this user.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to send request for non existing shift.")]
        [Authorize]
        [HttpPost]
        public ActionResult Post([FromBody] RequestReplacementViewModel shift)
        {
            return RequestReplacement(shift);
        }


        /// <summary>
        /// Delete replacement request specified by <paramref name="request"/>
        /// </summary>
        /// <param name="request">request id</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement request deleted successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to reference non existing replacement request.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to send request for a past date, or invalid date.")]
        [Authorize]
        [HttpDelete("{request}")]
        public ActionResult Delete(int request)
        {
            _context.ReplacementRequest.AsNoTracking().Load();
            return DeleteReplacementRequest(_context.ReplacementRequest.FirstOrDefault(s => s.Id == request));
        }

        /// <summary>
        /// Get replacement request specified by <paramref name="request"/>
        /// </summary>
        /// <param name="request">request id</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement request returned successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to reference non existing replacement request.")]
        [Authorize]
        [HttpGet("{request}")]
        public ActionResult Get(int request)
        {
            return GetReplacementRequest(request);
        }

        /// <summary>
        /// Get <paramref name="user"/>'s replacement requests for <paramref name="shift"/>.
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="shift">Shift id</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement request returned successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to reference non existing replacement request.")]
        [Authorize]
        [HttpGet("user={user}&shift={shift}")]
        public ActionResult Get(string user, int shift)
        {
            return GetReplacementRequests(user, shift);
        }

        private ActionResult GetReplacementRequests(string userId, int shiftid)
        {
            // check that user exists
            _context.Users.AsNoTracking().Load();
            var user = _context.Users.FirstOrDefault(u => u.UserName == userId);
            if (user == default(User))
            {
                return
                    404.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"user", "The specified user does not exist"}
                    });
            }

            // check that shift exists
            _context.Shift.AsNoTracking().Load();
            var shift = _context.Shift.FirstOrDefault(s => s.Id == shiftid);
            if (shift == default(Shift))
                return
                    404.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"shift", "The specified shift does not exist"}
                    });

            _context.ReplacementRequest.Include(r => r.Shift).Include(r => r.User).AsNoTracking().Load();

            var entries = _context.ReplacementRequest.Where(r => r.UserId == userId && r.ShiftId == shiftid);

            if (entries != null)
            {
                return entries.ToJson();
            }
            return new List<ReplacementRequest>().ToJson();
        }

        private ActionResult GetReplacementRequest(int requestId)
        {
            _context.ReplacementRequest.Include(r => r.Shift).Include(r => r.User).Load();

            var entry = _context.ReplacementRequest.FirstOrDefault(r => r.Id == requestId);

            if (entry != default(ReplacementRequest))
            {
                return entry.ToJson();
            }
            return 404.ErrorStatusCode();
        }

        private ActionResult DeleteReplacementRequest(ReplacementRequest entry)
        {
            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode();

            if (entry != default(ReplacementRequest))
            {
                _context.Remove(entry);
                _context.SaveChanges();
                return 200.SuccessStatusCode();
            }
            return 404.ErrorStatusCode();
        }

        private ActionResult RequestReplacement(RequestReplacementViewModel model)
        {
            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode();

            _context.Shift.Include(s=>s.User).Load();
            var shift = _context.Shift.FirstOrDefault(s => s.Id == model.ShiftId);

            // check that shift exists and is repleceable
            if (shift == default(Shift)) return 404.ErrorStatusCode();
            if (!shift.IsRepleceable)
                return 400.ErrorStatusCode(new Dictionary<string, string>
                {
                    {"setReplaceable", "The specified shift is not replaceable."}
                });

            // check that user in not trying to replace their own shift
            if (user.UserName == shift.UserId)
                return 400.ErrorStatusCode(new Dictionary<string, string>() {{"shiftId", "User cannot replace their own shift."}});

            // is date set?
            DateTime? date = null;
            // check date
            if (model.Date != null)
            {
                if (!model.Date.ValidateDate())
                    return 400.ErrorStatusCode(new Dictionary<string, string>() {{"date", "Invalid date"}});

                date = DateTime.Parse(model.Date);

                // check if date in past
                if (date < DateTime.Today)
                    return
                        400.ErrorStatusCode(new Dictionary<string, string>()
                        {
                            {"date", "Unable to create shift for past dates."}
                        });
            }

            // make sure this request does not already exist
            _context.ReplacementRequest.Include(r=>r.Shift).Load();
            var request = date != null
                ? _context.ReplacementRequest.FirstOrDefault(
                    r => r.ShiftId == shift.Id && r.UserId == user.Id && r.Date != null && r.Date.Value.Date == date.Value.Date)
                : _context.ReplacementRequest.FirstOrDefault(
                    r => r.ShiftId == shift.Id && r.UserId == user.Id && r.Date == null);
            if (request != default(ReplacementRequest))
                return
                    400.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"shiftId", "Identical request for this shift from this user aready exists."}
                    });

            // create a new request
            request = new ReplacementRequest()
            {
                Date = date,
                Shift = shift,
                ShiftId = shift.Id,
                UserId = user.Id,
                User = user
            };
            _context.Add(request);
            _context.SaveChanges();
            return request.ToJson(201);
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
