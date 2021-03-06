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

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DutyScheduler.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	public class AlgorithmController : Controller
	{
		private static readonly string DateFormat = "yyyy-MM-dd";
		private readonly ApplicationDbContext _context;
		private readonly UserManager<User> _userManager;

		public AlgorithmController(ApplicationDbContext context,
			UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		/// <summary>
		/// Run scheduler algorithm.
		/// </summary>
		/// <param name="year">Year  for which to generate the schedule</param>
		/// <param name="month">Month for which to generate the schedule</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.OK, "Algorithm generated successfully.")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
		[SwaggerResponse(HttpStatusCode.Forbidden, "User is not an admin.")]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Trying to set shift for a past, or invalid date, or non existing user.")]
		[Authorize]
		[HttpGet("year={year}&month={month}")]
		public ActionResult Run(int year, int month)
		{
			_context.Users.Load();

			// check that user is logged in
			var currUser = GetCurrentUser();
			if (currUser == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized);

			// check that the current user is admin
			if (!currUser.IsAdmin) return 403.ErrorStatusCode(Constants.Forbidden.ToDict());

			// check date
			var date = year + "-" + month + "-1";
			if (!date.ValidateDate()) return 400.ErrorStatusCode(Constants.InvalidDate);

            // run
            if (!Algorithm(new DateTime(year, month, 1))) return 400.ErrorStatusCode(Constants.InvalidDate);
		    var m = new Month(new DateTime(year, month, 1));
			return _context.DaysToJson(m.GetMonth(), currUser);
		}

		private bool Algorithm(DateTime date)
		{
			// check if date is invalid
			if (date.Month <= DateTime.Now.Month || date.Year < DateTime.Now.Year) return false;
			else
			{
				var m = new Month(new DateTime(date.Year, date.Month, 1));

				// list of all existing users.
				_context.Users.Load();
				var availableusers = _context.Users.Select(u => u.Id).ToList();

				// counted preferences for all dates in month
				Dictionary<DateTime, int> preferencesForDuty = CountPreferencesForMonth(new DateTime(date.Year, date.Month, 1));

				bool special = false;
				string userMinNmb = "";
				// loop through sorted dict with preferences - make shifts
				foreach (KeyValuePair<DateTime, int> item in preferencesForDuty.OrderBy(key => key.Value))
				{
                    // check if shift exits 
				    if (_context.Shift.Any(s => s.Date == item.Key)) continue;

					// check if day is type special
					special = isSpecial(item.Key);

					// choose available user with min number od previous duties and preference for that date
					userMinNmb = UserWithMinNumAndPreference(special, item.Key, availableusers);
					if (userMinNmb == "")
					{
						// no users with preferences available for duty (all scheduled), remove date to be considered for date without preference 
						preferencesForDuty.Remove(item.Key);
						continue;
					}

					// create new shift
					var entry = new Shift
					{
						UserId = userMinNmb,
						Date = item.Key,
						IsRepleceable = false
					};
					// add to shifts
					_context.Shift.Add(entry);
					_context.SaveChanges();

					// remove user from availableuser since he has a duty now
					availableusers.Remove(userMinNmb);

					// if all users have a duty already
					if (!availableusers.Any())
					{
						availableusers = _context.Users.Select(u => u.Id).ToList();
					}
				}

				// loop through all days of month left
				DateTime currentDate = m.First;
				while (currentDate <= m.Last)
				{
                    // check if shift exits 
				    if (_context.Shift.Any(s => s.Date == currentDate))
				    {
                        currentDate = currentDate.AddDays(1);
                        continue;
				    }

                    // exclude all days in holidays, non-working days, dates in preferencesForDuty
                    if (m.Holidays.FirstOrDefault(h => h.Date == currentDate) == default(Holiday) &&
						m.NonWorkingDays.FirstOrDefault(h => h.Date == currentDate) == default(NonWorkingDay) &&
						!preferencesForDuty.ContainsKey(currentDate))
					{
						// check if day is type special
						special = isSpecial(currentDate);

						// choose available user with min number of previous duties without negative preference for that date
						userMinNmb = UserWithMinNumAndNoNegPreference(special, currentDate, availableusers);
						if (userMinNmb == "")
						{
							// no user without negative preference, force available with with min number of previous duties for duty
							userMinNmb = UserWithMinNum(special, currentDate, availableusers);
						}

						// create new shift
						var entry = new Shift
						{
							UserId = userMinNmb,
							Date = currentDate,
							IsRepleceable = false
						};
						// add to shifts
						_context.Shift.Add(entry);
						_context.SaveChanges();

						// remove user from availableusers since has has a duty now
						availableusers.Remove(userMinNmb);
						// if all users have a duty already
						if (!availableusers.Any())
						{
							availableusers = _context.Users.Select(u => u.Id).ToList();
						}

					}
					currentDate = currentDate.AddDays(1);
				}
				return true;
			}

		}

		// check if date is special
		private bool isSpecial(DateTime date)
		{
			var month = new Month(date);
			var specialDay = month.SpecialDays.FirstOrDefault(d => d.Date == date);

			if (specialDay != default(SpecialDay)) return true;
			return false;
		}

		private Dictionary<string, int> GetUsersWithNumberOfDuties(bool special)
		{
			// list of all existing users
			_context.Users.Load();
			List<string> users = _context.Users.Select(u => u.Id).ToList();

			// counted number od previous duties - ordinary and special
			_context.Shift.Load();
			Dictionary<string, int> previousDuties = new Dictionary<string, int>();

			foreach (var user in users)
			{
				var shifts = _context.Shift.Where(s => s.UserId == user && s.Date.Year == DateTime.Now.Year);
				int noOfDuties = 0;
				foreach (var shift in shifts)
				{
					var month = new Month(shift.Date);
					if (special)
					{
						var specialDay = month.SpecialDays.FirstOrDefault(d => d.Date == shift.Date);
						if (specialDay != default(SpecialDay)) noOfDuties++;
					}
					else
					{
						var specialDay = month.SpecialDays.FirstOrDefault(d => d.Date == shift.Date);
						if (specialDay == default(SpecialDay)) noOfDuties++;
					}
				}
				previousDuties.Add(user, noOfDuties);
			}
			return previousDuties;
		}

		private Dictionary<DateTime, int> CountPreferencesForMonth(DateTime date)
		{
			Dictionary<DateTime, int> preferences = new Dictionary<DateTime, int>();
			_context.Preference.Load();
			int num = 0;

			var month = new Month(date);
			DateTime currentDate = month.First;
			var preferencesList = _context.Preference.ToList();

			while (currentDate <= month.Last)
			{
				num = preferencesList.Count(p => p.Date == currentDate && (p.IsPreferred ?? false));

				if (num > 0)
					preferences.Add(currentDate, num);

				currentDate = currentDate.AddDays(1);
			}

			return preferences;
		}


		private string UserWithMinNumAndPreference(bool special, DateTime date, List<string> available)
		{
			_context.Preference.Load();
			// choose available user with min number od previous duties and preference for that date
			string user = "";
			var usersWithPreference = _context.Preference.ToList().Where(d => d.Date == date && d.IsPreferred == true);

			Dictionary<string, int> previousDuties = GetUsersWithNumberOfDuties(special);
			foreach (KeyValuePair<string, int> item in previousDuties.OrderBy(key => key.Value))
			{
				// if user is available, give him new duty
				if (available.Any(u => u == item.Key) && usersWithPreference.Any(p => p.UserId == item.Key))
				{
					user = item.Key;
				}
			}
			return user;
		}

		private string UserWithMinNumAndNoNegPreference(bool special, DateTime date, List<string> available)
		{
			_context.Preference.Load();
			// choose available user with min number of previous duties and no negative preference for that date
			string user = "";
			var usersWithNegPreference = _context.Preference.ToList().Where(d => d.Date == date && d.IsPreferred == false);

			Dictionary<string, int> previousDuties = GetUsersWithNumberOfDuties(special);
            foreach (KeyValuePair<string, int> item in previousDuties.OrderBy(key => key.Value))
			{
				// if user is available, give him new duty
				if (available.Any(u => u == item.Key) && !usersWithNegPreference.Any(p => p.UserId == item.Key))
				{
					user = item.Key;
				    break;
				}
			}
			return user;
		}

		// there aren't any users with no neg preference, force one with min num for duty
		private string UserWithMinNum(bool special, DateTime date, List<string> available)
		{
			_context.Preference.Load();
			// choose available user with min number of previous duties
			string user = "";

			Dictionary<string, int> previousDuties = GetUsersWithNumberOfDuties(special);
			foreach (KeyValuePair<string, int> item in previousDuties.OrderBy(key => key.Value))
			{
				// if user is available, give him new duty
				if (available.Any(u => u == item.Key))
				{
					user = item.Key;
				}
			}
			return user;
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
