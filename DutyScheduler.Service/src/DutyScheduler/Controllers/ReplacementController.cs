﻿using System;
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
        /// Apply for replacement.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created, "Application successfully saved.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to send application for own shift, or a non replaceable shift, or the shift that already has application by this user.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to send application for non existing shift.")]
        [Authorize]
        [HttpPost]
        public ActionResult Post([FromBody] ApplyForReplacementViewModel shift)
        {
            return ApplyForReplacement(shift);
        }


        /// <summary>
        /// Delete replacement application specified by <paramref name="request"/>
        /// </summary>
        /// <param name="request">Request id</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement application deleted successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to reference non existing replacement application.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to send application for a past date, or invalid date.")]
        [Authorize]
        [HttpDelete("{request}")]
        public ActionResult Delete(int request)
        {
            _context.ReplacementRequest.AsNoTracking().Load();
            return DeleteReplacementRequest(_context.ReplacementRequest.FirstOrDefault(s => s.Id == request));
        }

        /// <summary>
        /// Get replacement application specified by <paramref name="request"/>
        /// </summary>
        /// <param name="request">Request id</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Replacement application returned successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to reference non existing replacement application.")]
        [Authorize]
        [HttpGet("{request}")]
        public ActionResult Get(int request)
        {
            return GetReplacementRequest(request);
        }


        private ActionResult GetUsersReplacementRequest(string userId)
        {
            _context.Users.AsNoTracking().Load();
            var user = _context.Users.FirstOrDefault(u => u.UserName == userId);
            if (user == default(User))
            {
                return
                    404.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"userId", "The user with the specified userId was not found"}
                    });
            }

            _context.ReplacementRequest.Include(r => r.Shift).Include(r => r.User).AsNoTracking().Load();

            var entries = _context.ReplacementRequest.Where(r => r.UserId == userId);

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

        private ActionResult ApplyForReplacement(ApplyForReplacementViewModel model)
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
                return 400.ErrorStatusCode(new Dictionary<string, string>() {{"shiftId", "User cannot apply to replace their own shift."}});

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
            var request = _context.ReplacementRequest.FirstOrDefault(r => r.ShiftId == shift.Id && r.UserId == user.Id);
            if (request != default(ReplacementRequest))
                return 400.ErrorStatusCode(new Dictionary<string, string>() { {"shiftId","Application for this shift and user aready exists."} });

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

        #region unused

        ///// <summary>
        ///// Apply for replacement for the date specified by <paramref name="year"/>, 
        ///// <paramref name="month"/> and <paramref name="day"/>.
        ///// </summary>
        ///// <param name="year">Year part of date</param>
        ///// <param name="month">Month part of date</param>
        ///// <param name="day">Day part of date</param>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.Created, "Application successfully saved.")]
        //[SwaggerResponse(HttpStatusCode.NotModified, "Application already exists, nothing happened.")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        //[SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        //[SwaggerResponse(HttpStatusCode.BadRequest, "Trying to send application for a past date, or invalid date.")]
        //[Authorize]
        //[HttpPost("year={year}&month={month}&day={day}")]
        //public ActionResult Post(int year, int month, int day)
        //{
        //    if (!Utils.ValidateDate(year, month, day))
        //        return 400.ErrorStatusCode(
        //            new Dictionary<string, string>() { { "date", "Invalid date." } }
        //        );

        //    return ApplyForReplacement(new DateTime(year, month, day));
        //}

        ///// <summary>
        ///// Delete replacement application for date specified by 
        ///// <paramref name="year"/>, <paramref name="month"/> 
        ///// and <paramref name="day"/>
        ///// </summary>
        ///// <param name="year">Year part of date</param>
        ///// <param name="month">Month part of date</param>
        ///// <param name="day">Day part of date</param>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.OK, "Replacement application deleted successfully.")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        //[SwaggerResponse(HttpStatusCode.NotFound, "Trying to reference non existing replacement application.")]
        //[SwaggerResponse(HttpStatusCode.BadRequest, "Trying to send application for a past date, or invalid date.")]
        //[Authorize]
        //[HttpDelete("year={year}&month={month}&day={day}")]
        //public ActionResult Delete(int year, int month, int day)
        //{
        //    if (!Utils.ValidateDate(year, month, day))
        //        return 400.ErrorStatusCode(
        //            new Dictionary<string, string>() { { "date", "Invalid date." } }
        //        );

        //    return DeleteReplacementRequest(new DateTime(year, month, day));
        //}

        ///// <summary>
        ///// Set whether the date specified by <paramref name="year"/>, 
        ///// <paramref name="month"/> and <paramref name="day"/> is repleceable.
        ///// </summary>
        ///// <param name="year">Year part of date</param>
        ///// <param name="month">Month part of date</param>
        ///// <param name="day">Day part of date</param>
        ///// <param name="model">Model</param>
        ///// <returns></returns>
        //[SwaggerResponse(HttpStatusCode.OK, "Setting successfully saved.")]
        //[SwaggerResponse(HttpStatusCode.NotModified, "Model was empty, nothing happened.")]
        //[SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        //[SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        //[SwaggerResponse(HttpStatusCode.BadRequest, "Trying to set this setting for a past date, or invalid date.")]
        //[Authorize]
        //[HttpPut("year={year}&month={month}&day={day}")]
        //public ActionResult Put(int year, int month, int day, [FromBody]SetReplacementViewModel model)
        //{
        //    if (model == default(SetReplacementViewModel)) return NoContent();

        //    if (!Utils.ValidateDate(year, month, day))
        //        return 400.ErrorStatusCode(
        //            new Dictionary<string, string>() { { "date", "Invalid date." } }
        //        );

        //    return SetReplaceable(new DateTime(year, month, day), model.SetReplaceable);
        //}

        //private ActionResult SetReplaceable(DateTime date, bool setReplaceable)
        //{
        //    // check that user is logged in
        //    var user = GetCurrentUser();
        //    if (user == default(User)) return 401.ErrorStatusCode();

        //    // check that shift exists and the current user can modify it
        //    _context.Shift.Load();
        //    var entry = _context.Shift.FirstOrDefault(p => p.Date.Date == date && p.UserId == user.Id);
        //    if (entry == default(Shift)) return 404.ErrorStatusCode();
        //    if (user.Id != entry.UserId && !user.IsAdmin) return 403.ErrorStatusCode();

        //    // check if date in past
        //    if (date < DateTime.Today)
        //        return 400.ErrorStatusCode(
        //            new Dictionary<string, string>() { { "date", "Unable to change settings for past dates." } }
        //        );


        //    entry.IsRepleceable = setReplaceable;
        //    _context.SaveChanges();
        //    return 200.SuccessStatusCode();
        //}

        //private ActionResult ApplyForReplacement(DateTime date)
        //{
        //    // check that user is logged in
        //    var user = GetCurrentUser();
        //    if (user == default(User)) return 401.ErrorStatusCode();

        //    // check that shift exists and is repleceable
        //    _context.Shift.Load();
        //    var shift = _context.Shift.FirstOrDefault(p => p.Date.Date == date);
        //    if (shift == default(Shift)) return 404.ErrorStatusCode();
        //    if (!shift.IsRepleceable)
        //        return 400.ErrorStatusCode(new Dictionary<string, string>
        //        {
        //            {"setReplaceable", "The specified shift is not replaceable."}
        //        });

        //    // make sure this request does not already exist
        //    _context.ReplacementRequest.Load();
        //    var request = _context.ReplacementRequest.FirstOrDefault(r => r.ShiftId == shift.Id && r.UserId == user.Id);
        //    if (request != default(ReplacementRequest)) return 304.SuccessStatusCode();

        //    // create a new request
        //    request = new ReplacementRequest()
        //    {
        //        Date = DateTime.Now,
        //        Shift = shift,
        //        ShiftId = shift.Id,
        //        UserId = user.Id,
        //        User = user
        //    };
        //    _context.Add(request);
        //    return 201.SuccessStatusCode();
        //}

        //private ActionResult DeleteReplacementRequest(DateTime date)
        //{
        //    // check that user is logged in
        //    var user = GetCurrentUser();
        //    if (user == default(User)) return 401.ErrorStatusCode();

        //    // get preference
        //    _context.ReplacementRequest.Load();
        //    var entry = _context.ReplacementRequest
        //        .FirstOrDefault(r => r.Date != null &&
        //                        r.Date.Value == date &&
        //                        r.UserId == user.Id);

        //    if (entry != default(ReplacementRequest))
        //    {
        //        _context.Remove(entry);
        //        _context.SaveChanges();
        //        return 200.SuccessStatusCode();
        //    }
        //    return 404.ErrorStatusCode();
        //}

        #endregion

    }
}
