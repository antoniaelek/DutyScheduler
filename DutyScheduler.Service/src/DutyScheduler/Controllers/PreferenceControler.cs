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
    [Authorize]
    [Route("api/[controller]")]
    public class PreferenceController : Controller
    {
        private static readonly string DateFormat = "yyyy-MM-dd";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PreferenceController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Create user's preference for the specified date.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Preference saved successfully.")]
        [SwaggerResponse(HttpStatusCode.NotModified, "Model was empty, nothing happened.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to set preference to a past date, or invalid date, or the datw which already has preference set.")]
        [Authorize]
        [HttpPost]
        public ActionResult Post([FromBody]CreatePreferenceViewModel model)
        {
            if (model == default(CreatePreferenceViewModel)) return 400.ErrorStatusCode(Constants.BadRequest.ToDict());

            if (!model.Date.ValidateDate())
                return 400.ErrorStatusCode(
                    new Dictionary<string, string>() { { "date", "Invalid date." } }
                );

            return CreatePrefered(DateTime.Parse(model.Date), model.SetPrefered);
        }

        /// <summary>
        /// Create or update user's preference.
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Preference saved successfully.")]
        [SwaggerResponse(HttpStatusCode.NotModified, "Model was empty, nothing happened.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to set preference to a past date, or invalid date, or the datw which already has preference set.")]
        [Authorize]
        [HttpPut]
        public ActionResult Put([FromBody]CreateOrUpdatePreferenceViewModel model)
        {
            if (model == default(CreateOrUpdatePreferenceViewModel)) return 204.SuccessStatusCode(Constants.NoContent.ToDict());

            _context.Preference.Include(p => p.User).Load();

            if (!model.Date.ValidateDate())
                return 400.ErrorStatusCode(
                    new Dictionary<string, string>() { { "date", "Invalid date." } }
                );

            var user = GetCurrentUser();

			// if already exists, update
			var preference = _context.Preference.FirstOrDefault(p => p.Date.ToString(DateFormat) == model.Date && p.UserId == user.Id);

            if (preference != default(Preference) && model.SetPrefered != null)
                return UpdatePrefered(preference, model.SetPrefered.Value);

            if (preference != default(Preference) && model.SetPrefered == null)
                return DeletePreference(preference);

            // if not, create new
            if (model.SetPrefered != null) return CreatePrefered(DateTime.Parse(model.Date), model.SetPrefered.Value);
            return
                400.ErrorStatusCode(new Dictionary<string, string>()
                {
                    {"SetPrefered", "SetPrefered property not set."},
                    {"Date", "Date property not set."}
                });
        }

        /// <summary>
        /// Update user's preference specified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Preference id</param>
        /// <param name="model">Preference</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created, "Preference saved successfully.")]
        [SwaggerResponse(HttpStatusCode.NotModified, "Model was empty, nothing happened.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Trying to set preference to a past date, or invalid date.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to update a non existing preference.")]
        [Authorize]
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody]UpdatePreferenceViewModel model)
        {
            if (model == default(UpdatePreferenceViewModel)) return 400.ErrorStatusCode(Constants.BadRequest.ToDict());
            _context.Preference.Include(p => p.User).Load();
            return UpdatePrefered(_context.Preference.FirstOrDefault(p => p.Id == id), model.SetPrefered);
        }

        /// <summary>
        /// Delete user's preference specified by <paramref name="id"/>
        /// </summary>
        /// <param name="id">Preference id</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Preference deleted successfully.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Trying to reference non existing preference.")]
        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            return DeletePreference(_context.Preference.FirstOrDefault(p => p.Id == id));
        }

        private ActionResult CreatePrefered(DateTime date, bool isPrefered)
        {
            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            // check if date in past
            if (date < DateTime.Today)
                return 400.ErrorStatusCode(
                    new Dictionary<string, string>() { { "date", "Unable to set preferences for past dates." } }
                );

            // check that date is not weekend or holiday
            var m = new Month(new DateTime(date.Year, date.Month, 1));
            if (m.Holidays.Select(d=>d.Date).Contains(date))
                return 400.ErrorStatusCode(
                    new Dictionary<string, string>() { { "date", "Unable to set preferences for holidays." } }
                );
            if (m.NonWorkingDays.Select(d => d.Date).Contains(date))
                return 400.ErrorStatusCode(
                    new Dictionary<string, string>() { { "date", "Unable to set preferences for non working days." } }
                );

            // check if preference already exists
            _context.Preference.Include(p => p.User).Load();
            var entry = _context.Preference.FirstOrDefault(p => p.Date == date && p.UserId == user.Id);

            if (entry != default(Preference))
                return
                    400.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"date", "Preference for the selected date already exists"}
                    });

            // add new preference
            entry = new Preference
            {
                Date = date,
                IsPreferred = isPrefered,
                UserId = user.Id,
                User = user
            };
            _context.Preference.Add(entry);
            
            _context.SaveChanges();
            return entry.ToJson(201);
        }

        private ActionResult UpdatePrefered(Preference entry, bool isPrefered)
        {
            // check if preference already exists
            _context.Preference.Include(p => p.User).Load();
            
            if (entry == default(Preference))
                return
                    404.ErrorStatusCode(Constants.UserNotFound.ToDict());            
            
            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            // check if date in past
            if (entry.Date < DateTime.Today)
                return 400.ErrorStatusCode(
                    new Dictionary<string, string>() { { "date", "Unable to set preferences for past dates." } }
                );

            // update existing preference
            entry.IsPreferred = isPrefered;
            _context.SaveChanges();
            return entry.ToJson();
        }

        private ActionResult DeletePreference(Preference preference)
        {
            _context.Preference.Include(p => p.User).Load();
           

            // check that user is logged in
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            if (preference != default(Preference))
            {
                _context.Remove(preference);
                _context.SaveChanges();
                return 200.SuccessStatusCode(Constants.OK.ToDict());
            }
            return 404.ErrorStatusCode(Constants.PreferenceNotFound.ToDict());
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
