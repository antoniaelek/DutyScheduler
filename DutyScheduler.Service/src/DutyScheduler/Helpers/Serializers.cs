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

        private static object SerializeReplacementRequest(ReplacementRequest request)
        {
            return new
            {
                id = request.Id,
                shiftId = request.Shift.Id,
                userId = request.UserId,
                date = request.Date?.ToString(DateFormat)
            };
        }

        private static IEnumerable<object> SerializeReplacementRequests(IEnumerable<ReplacementRequest> requests)
        {
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

        private static object SerializeUser(User user)
        {
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

        public static JsonResult ToJson(this User user, int statusCode = 200)
        {
            var json = new JsonResult(new
            {
                username = user.UserName,
                name = user.Name,
                lastName = user.LastName,
                email = user.Email,
                phone = user.Phone,
                office = user.Office,
                isAdmin = user.IsAdmin
            });
            json.StatusCode = statusCode;
            return json;
        }

        #endregion

        #region shift

        private static object SerializeShift(Shift shift)
        {
            return new
            {
                id = shift.Id,
                userId = shift.UserId,
                date = shift.Date.ToString(DateFormat),
                isReplaceable = shift.IsRepleceable,
            };
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



        public static JsonResult ToJson(this Shift shift, List<ReplacementRequest> requests,  int statusCode = 200) // todo
        {
            var replacementApplications = SerializeReplacementRequests(requests);
            var user = SerializeUser(shift.User);
            var json = new JsonResult(new
            {
                id = shift.Id,
                userId = shift.UserId,
                date = shift.Date.ToString(DateFormat),
                isReplaceable = shift.IsRepleceable,
                user,
                replacementApplications
            });
            json.StatusCode = statusCode;
            return json;
        }

        #endregion

        #region preference

        private static object SeralizePreference(Preference preference)
        {
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
