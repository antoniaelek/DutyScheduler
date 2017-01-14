using System.Collections.Generic;

namespace DutyScheduler.Helpers
{
    public static class Constants
    {
        public static KeyValuePair<string,string> UserNotFound => new KeyValuePair<string, string>("User", "User not found.");
        public static KeyValuePair<string,string> ShiftNotFound => new KeyValuePair<string, string>("Shift", "Shift not found.");
        public static KeyValuePair<string,string> PreferenceNotFound => new KeyValuePair<string, string>("Preference", "Preference not found.");
        public static KeyValuePair<string,string> ReplacementRequestNotFound => new KeyValuePair<string, string>("ReplacementRequest", "ReplacementRequest not found.");

        public static KeyValuePair<string,string> Unauthorized => new KeyValuePair<string, string>("User", "Not logged in.");
        public static KeyValuePair<string,string> Forbidden => new KeyValuePair<string, string>("User", "User does not have sufficient rights to perform this action.");

        public static KeyValuePair<string,string> BadRequest => new KeyValuePair<string, string>("Error", "Bad request.");

        public static KeyValuePair<string,string> OK => new KeyValuePair<string, string>("Success", "OK.");
        public static KeyValuePair<string,string> Created => new KeyValuePair<string, string>("Success", "Created.");
        public static KeyValuePair<string,string> NoContent => new KeyValuePair<string, string>("Success", "No content.");

        public static KeyValuePair<string,string> NotModified => new KeyValuePair<string, string>("Status", "Not modified.");
    }
}
