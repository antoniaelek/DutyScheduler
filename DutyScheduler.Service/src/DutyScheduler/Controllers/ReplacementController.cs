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
    [Authorize]
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
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to send request for own shift, or a non replaceable shift, " +
                                                    "or the shift that already has identical request by this user. " +
                                                    "Or trying to switch date when user has no shift scheduled")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to send request for non existing shift.")]
        [Authorize]
        [HttpPost]
        public ActionResult Post([FromBody] RequestReplacementViewModel shift)
        {
            return RequestReplacement(shift);
        }


        /// <summary>
        /// Accept replacement.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created, "Replacement successfully performed.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to accept a non existing replacement request, or other errors.")]
        [Authorize]
        [HttpPost("accept")]
        public ActionResult Accept([FromBody]AcceptReplacementRequestViewModel shift)
        {
            return AcceptReplacement(shift.RequestId);
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


        private ActionResult AcceptReplacement(int requestId)
        {
            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode();

            // check that request which we are accepting exist
            _context.ReplacementRequest.Include(r => r.User).Include(r => r.Shift).Load();
            var request = _context.ReplacementRequest.FirstOrDefault(r => r.Id == requestId);

            if (request == default(ReplacementRequest))
                return 404.ErrorStatusCode(new Dictionary<string, string>() { {"request","Request not found."} });

            // check that shift that the request refers to belongs to current user
            if (request.Shift.UserId != user.Id)
                return 403.ErrorStatusCode(new Dictionary<string, string>() { { "request", "Current user is not allowed to accept the specified request." } });

            // if replacing date set, check that shift exists
            _context.Shift.Include(s => s.User).Load();
            Shift otherShift = null;
            if (request.Date != null)
            {
                otherShift = _context.Shift.FirstOrDefault(s => s.UserId == request.UserId && s.Date.Date == request.Date.Value.Date);
                if (otherShift == default(Shift))
                    return 404.ErrorStatusCode(new Dictionary<string, string>() { { "shift", "Unable to accept replacement request - replacing shift not found." } });
            }

            // switch users and reset isReplaceable
            var shiftToChange = _context.Shift.FirstOrDefault(s => s.Id == request.ShiftId);
            shiftToChange.UserId = request.UserId;
            shiftToChange.User = _context.Users.FirstOrDefault(u => u.Id == shiftToChange.UserId);
            shiftToChange.IsRepleceable = false;

            if (request.Date != null && otherShift != null)
            {
                otherShift.UserId = user.Id;
                otherShift.User = _context.Users.FirstOrDefault(u => u.Id == otherShift.UserId);
                otherShift.IsRepleceable = false;
            }

            // delete other requests
            var others = _context.ReplacementRequest.Where(r => r.ShiftId == request.ShiftId);
            _context.ReplacementRequest.RemoveRange(others);

            // add to change to history
            _context.ReplacementHistory.Include(r => r.ReplacedUser).Include(r => r.ReplacingUser).Load();
            var history = new ReplacementHistory()
            {
                ReplacedUser = user,
                ReplacedUserId = user.Id,
                ReplacingUser = request.User,
                ReplacingUserId = request.UserId,
                Date = request.Shift.Date,
                DateCreated = DateTime.Now
            };
            _context.ReplacementHistory.Add(history);

            // save changes
            _context.SaveChanges();

            return shiftToChange.ToJson();
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

            _context.ReplacementRequest.Include(r => r.Shift).Include(r => r.User).Load();

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


                // check that date belongs to user's shift
                var ownShift = _context.Shift.FirstOrDefault(s => s.Date == date.Value);
                if (ownShift == default(Shift))
                    return
                        400.ErrorStatusCode(new Dictionary<string, string>()
                        {
                        {"date", "Unable to request replacement becaause user has no shift scheduled on specified date."}
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
