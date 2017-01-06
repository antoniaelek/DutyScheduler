using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DutyScheduler.Models;
using Microsoft.AspNetCore.Mvc;

namespace DutyScheduler.Helpers
{
    public static class Serializers
    {
        private static readonly string DateFormat = "yyyy-MM-dd";

        #region replacement request

        public static JsonResult ToJson(this ReplacementRequest request, int statusCode = 200)
        {
            var user = new
            {
                username = request.User.UserName,
                name = request.User.Name,
                lastName = request.User.LastName,
                email = request.User.Email,
                phone = request.User.Phone,
                office = request.User.Office,
                isAdmin = request.User.IsAdmin
            };
            var json = new JsonResult(new
            {
                id = request.Id,
                userId = request.UserId,
                date = request.Date?.ToString(DateFormat),
                user
            });
            json.StatusCode = statusCode;
            return json;
        }

        #endregion

        #region shift

        public static JsonResult ToJson(this Shift shift, int statusCode = 200)
        {
            var user = new
            {
                username = shift.User.UserName,
                name = shift.User.Name,
                lastName = shift.User.LastName,
                email = shift.User.Email,
                phone = shift.User.Phone,
                office = shift.User.Office,
                isAdmin = shift.User.IsAdmin
            };
            var json =  new JsonResult(new
            {
                id = shift.Id,
                userId =shift.UserId,
                date = shift.Date.ToString(DateFormat),
                isReplaceable = shift.IsRepleceable,
                user
            });
            json.StatusCode = statusCode;
            return json;
        }

        public static JsonResult ToJson(this Shift shift, List<ReplacementRequest> requests,  int statusCode = 200)
        {
            var replacementApplications = requests.Select(r => new
            {
                r.Id,
                r.ShiftId,
                r.UserId,
                r.Date
                //r.User.Name,
                //r.User.LastName,
                //r.User.Email,
                //r.User.Phone,
                //r.User.Office,
                //r.User.IsAdmin
            });
            var user = new
            {
                username = shift.User.UserName,
                name = shift.User.Name,
                lastName = shift.User.LastName,
                email = shift.User.Email,
                phone = shift.User.Phone,
                office = shift.User.Office,
                isAdmin = shift.User.IsAdmin
            };
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
    }
}
