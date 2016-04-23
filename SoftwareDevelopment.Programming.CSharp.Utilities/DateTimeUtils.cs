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
        /// <returns></returns>
        public static string DateTimeToString(string format, DateTime dateTime)
        {
            return dateTime.ToString(format);
        }
    }
}
