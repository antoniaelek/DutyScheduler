using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DutyScheduler.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

namespace DutyScheduler.Helpers
{
    public static class Utils
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static bool ValidateDate(this string date)
        {
            if (date == null) return false;
            DateTime outRes;
            if (!DateTime.TryParse(date, out outRes)) return false;
            if (outRes.Year < 0) return false;
            if (outRes.Month < 1 || outRes.Month > 12) return false;
            if (outRes.Day < 1 || outRes.Day > DateTime.DaysInMonth(outRes.Year, outRes.Month)) return false;
            return true;
        }

        public static Dictionary<string, string> ValidationErrors(this ModelStateDictionary modelState)
        {
            var keys = modelState.Keys;
            var allErrors = new Dictionary<string, string>();

            foreach(var key in keys)
            {
                ModelStateEntry val;
                if (!modelState.TryGetValue(key, out val)) continue;
                allErrors.Add(key,val.Errors.Select(err => err.ErrorMessage).FirstOrDefault());
            }
            return allErrors;
        }

        public static JsonResult ErrorStatusCode(this int status, Dictionary<string, string> errors = null)
        {
            var ret = new JsonResult(new
            {
                Success = false
            });

            if (errors != null)
            {
                ret = new JsonResult(new { errors });
            }

            ret.StatusCode = status;
            return ret;
        }

        public static JsonResult SuccessStatusCode(this int status, Dictionary<string, string> messages = null)
        {
            var ret = new JsonResult(new
            {
                Success = true
            });

            if (messages != null)
            {
                ret = new JsonResult(new { messages });
            }

            ret.StatusCode = status;
            return ret;
        }

        public static async Task<User> GetUser(this UserManager<User> userManager, string name)
        {
            //var name = User.Identity.Name;
            if (name == null) return null;
            var user = await userManager.FindByNameAsync(name);
            if (user == null) return null;
            return user;
        }

        public static string ReadConfig(this string param1, string param2)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var config = builder.Build();
            return config[param1 + ":" + param2];
        }
    }
}