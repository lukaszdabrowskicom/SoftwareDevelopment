using System;
using System.Reflection;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common DateTime operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public class DateTimeUtils
    {
        /// <summary>
        /// Returns string representation of provided DateTime object formatted with provided format.
        /// </summary>
        /// <param name="format">Custom date, time or dateTime format that is recognized by .NET</param>
        /// <param name="dateTime">DateTime instance object</param>
        /// <returns>string</returns>
        public static string DateTimeToString(string format, DateTime dateTime)
        {
            return dateTime.ToString(format);
        }

        /// <summary>
        /// Converts string representation to DateTime object.
        /// </summary>
        /// <param name="dateTime">DateTime object in the form of 'yyyy-MM-dd'</param>
        /// <returns>DateTime</returns>
        public static DateTime StringToDateTime(string dateTime)
        {
            return DateTime.Parse(dateTime);
        }

        /// <summary>
        /// Converts unix timestamp to DateTime.
        /// </summary>
        /// <param name="unixTimestamp">unix timestamp</param>
        /// <returns>DateTime</returns>
        public static DateTime ConvertUnixTimestampToDateTime(double unixTimestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimestamp).ToLocalTime();

            return dateTime;
        }

        /// <summary>
        /// Converts DateTime to unix timestamp.
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <returns></returns>
        public static double ConvertDateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Converts DateTime to unix timestamp.
        /// </summary>
        /// <param name="dateTime">string representation of DateTime object</param>
        /// <returns></returns>
        public static double ConvertDateTimeToUnixTimestamp(string dateTime)
        {
            DateTime dt = StringToDateTime(dateTime);

            return ConvertDateTimeToUnixTimestamp(dt);
        }
    }
}
