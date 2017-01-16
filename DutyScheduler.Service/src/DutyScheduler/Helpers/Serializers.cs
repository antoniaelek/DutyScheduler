using System.Collections.Generic;
using System.Linq;
using DutyScheduler.Models;
using DutyScheduler.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DutyScheduler.Helpers
{
	public static class Serializers
	{
		private static readonly string DateFormat = "yyyy-MM-dd";

		#region replacement request   

		public static object SerializeReplacementRequest(this ReplacementRequest request)
		{
			if (request == default(ReplacementRequest)) return new { };
			return new
			{
				id = request.Id,
				shiftId = request.Shift.Id,
				userId = request.UserId,
				date = request.Date?.ToString(DateFormat)
			};
		}

		public static IEnumerable<object> SerializeReplacementRequests(this IEnumerable<ReplacementRequest> requests, bool serializeUser = false)
		{
			if (requests == null) return new object[1];
			if (serializeUser == true)
			{
				return requests.OrderBy(r => r.Date).Select(r => new
				{
					id = r.Id,
					shiftId = r.ShiftId,
					userId = r.UserId,
					date = r.Date?.ToString(DateFormat),
					user = r.User.SerializeUser()
				});
			}

			return requests.OrderBy(r => r.Date).Select(r => new
			{
				id = r.Id,
				shiftId = r.ShiftId,
				userId = r.UserId,
				date = r.Date?.ToString(DateFormat)
			});
		}


		public static JsonResult ToJson(this ReplacementRequest request, int statusCode = 200)
		{
			var user = SerializeUser(request.User);
			var shift = SerializeShift(request.Shift);

			var json = new JsonResult(new
			{
				id = request.Id,
				shiftId = request.Shift.Id,
				userId = request.UserId,
				date = request.Date?.ToString(DateFormat),
				user,
				shift
			});
			json.StatusCode = statusCode;
			return json;
		}

		public static JsonResult ToJson(this IEnumerable<ReplacementRequest> requests, int statusCode = 200)
		{
			var json = new JsonResult(SerializeReplacementRequests(requests));
			json.StatusCode = statusCode;
			return json;
		}

		#endregion

		#region user

		public static object SerializeUser(this User user)
		{
			if (user == default(User)) return new { };
			return new
			{
				username = user.UserName,
				name = user.Name,
				lastName = user.LastName,
				email = user.Email,
				phone = user.Phone,
				office = user.Office,
				isAdmin = user.IsAdmin
			};
		}

		public static IEnumerable<object> SerializeUsers(this IEnumerable<User> users)
		{
			if (users == null) return new object[1];

			return users.Select(user => new
			{
				username = user.UserName,
				name = user.Name,
				lastName = user.LastName,
				email = user.Email,
				phone = user.Phone,
				office = user.Office,
				isAdmin = user.IsAdmin
			});
		}

		public static JsonResult ToJson(this User user, int statusCode = 200)
		{
			var json = new JsonResult(user.SerializeUser());
			json.StatusCode = statusCode;
			return json;
		}

		public static JsonResult ToJson(this IEnumerable<User> users, int statusCode = 200)
		{
			var json = new JsonResult(users.SerializeUsers());
			json.StatusCode = statusCode;
			return json;
		}

		#endregion

		#region shift

		public static object SerializeShift(this Shift shift)
		{
			if (shift == default(Shift)) return new { };
			return new
			{
				id = shift.Id,
				userId = shift.UserId,
				date = shift.Date.ToString(DateFormat),
				isReplaceable = shift.IsRepleceable,
			};
		}

		public static IEnumerable<object> SerializeShifts(this IEnumerable<Shift> shifts)
		{
			if (shifts == null) return new object[1];

			return shifts.Select(s => new
			{
				id = s.Id,
				userId = s.UserId,
				date = s.Date.ToString(DateFormat),
				isReplaceable = s.IsRepleceable,
			});
		}

		public static JsonResult ToJson(this Shift shift, int statusCode = 200)
		{
			var user = SerializeUser(shift.User);
			var json = new JsonResult(new
			{
				id = shift.Id,
				userId = shift.UserId,
				date = shift.Date.ToString(DateFormat),
				isReplaceable = shift.IsRepleceable,
				user
			});
			json.StatusCode = statusCode;
			return json;
		}

		public static JsonResult ToJson(this IEnumerable<Shift> shifts, int statusCode = 200)
		{
			var json = new JsonResult(SerializeShifts(shifts));
			json.StatusCode = statusCode;
			return json;
		}


		public static JsonResult ToJson(this Shift shift, List<ReplacementRequest> requests, int statusCode = 200)
		{
			var replacementRequests = SerializeReplacementRequests(requests);
			var user = SerializeUser(shift.User);
			var json = new JsonResult(new
			{
				id = shift.Id,
				userId = shift.UserId,
				date = shift.Date.ToString(DateFormat),
				isReplaceable = shift.IsRepleceable,
				user,
				replacementRequests
			});
			json.StatusCode = statusCode;
			return json;
		}

		#endregion

		#region preference

		public static object SeralizePreference(this Preference preference)
		{
			if (preference == default(Preference)) return new { };
			return new
			{
				id = preference.Id,
				userId = preference.UserId,
				isPrefered = preference.IsPreferred,
				date = preference.Date.ToString(DateFormat)
			};
		}

		public static JsonResult ToJson(this Preference preference, int statusCode = 200)
		{
			var user = SerializeUser(preference.User);
			var pref = SeralizePreference(preference);

			var json = new JsonResult(new
			{
				id = preference.Id,
				userId = preference.UserId,
				isPrefered = preference.IsPreferred,
				date = preference.Date.ToString(DateFormat),
				user
			});
			json.StatusCode = statusCode;
			return json;
		}

        #endregion

        public static ActionResult DaysToJson(this ApplicationDbContext _context, IEnumerable<Day> days, User user = null)
        {
            _context.Shift.Include(s => s.User).Load();
            _context.Preference.Include(s => s.User).Load();
            _context.ReplacementRequest.Include(r => r.Shift).Include(r => r.User).Load();

            //var user = GetCurrentUser();
            ICollection<DayViewModel> dayVMs = new List<DayViewModel>();
            foreach (var d in days)
            {
                bool? isPrefered = null;
                var shiftId = _context.Shift.FirstOrDefault(s => s.Date.Date == d.Date.Date)?.Id;
                Shift shift = null;
                ICollection<ReplacementRequest> replacementRequests = null;

                if (shiftId != null)
                    shift = _context.Shift.FirstOrDefault(s => s.Id == shiftId);

                if (user != default(User))
                {
                    isPrefered = _context.Preference.FirstOrDefault(p => p.Date.Date == d.Date.Date && p.UserId == user.Id)?.IsPreferred;
                    replacementRequests = new List<ReplacementRequest>();
                }

                if (shiftId != null && user != default(User))
                {
                    // if this is current user's shift return all
                    if (shift.UserId == user.Id)
                        replacementRequests = _context.ReplacementRequest.Where(r => r.ShiftId == shiftId).ToList();

                    // else return current users' requests for shift
                    else
                        replacementRequests = _context.ReplacementRequest.Where(r => r.ShiftId == shiftId && r.UserId == user.Id).ToList();
                }

                dayVMs.Add(new DayViewModel()
                {
                    Date = d.Date.ToString(DateFormat),
                    Weekday = d.WeekDay,
                    Type = d.Type,
                    Name = d.Name,
                    IsReplaceable = _context.Shift.FirstOrDefault(s => s.Date.Date == d.Date.Date)?.IsRepleceable,
                    IsPrefered = isPrefered,
                    ShiftId = shiftId,
                    Scheduled = shift?.User.SerializeUser(),
                    ReplacementRequests = replacementRequests.SerializeReplacementRequests(true)
                });
            }
            return new JsonResult(dayVMs);
        }
    }
}
