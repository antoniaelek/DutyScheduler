using System.Collections.Generic;
using System.Linq;
using DutyScheduler.Models;
using Microsoft.AspNetCore.Mvc;

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
            if (serializeUser == true)
            {
                return requests.Select(r => new
                {
                    id = r.Id,
                    shiftId = r.ShiftId,
                    userId = r.UserId,
                    date = r.Date?.ToString(DateFormat),
                    user = r.User.SerializeUser()
                });
            }

            return requests.Select(r => new
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
            if (shift == default(Shift)) return new {};
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


        public static JsonResult ToJson(this Shift shift, List<ReplacementRequest> requests,  int statusCode = 200)
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
                isPrefered = preference.IsPreferred
            };
        }

        public static JsonResult ToJson (this Preference preference, int statusCode = 200)
        {
            var user = SerializeUser(preference.User);
            var pref = SeralizePreference(preference);

            var json = new JsonResult(new
            {
                id = preference.Id,
                userId = preference.UserId,
                isPrefered = preference.IsPreferred,
                user
            });
            json.StatusCode = statusCode;
            return json;
        }

        #endregion
    }
}
