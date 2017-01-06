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
                r.UserId,
                r.User.Name,
                r.User.LastName,
                r.User.Email,
                r.User.Phone,
                r.User.Office,
                r.User.IsAdmin
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
    }
}
