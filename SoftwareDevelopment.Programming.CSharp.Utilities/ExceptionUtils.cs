using System;
using System.Configuration;
using System.Reflection;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for exception throwing operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class ExceptionUtils
	{
        /// <summary>
        /// Value for this parameter is null or empty '{0}'. Provide value instead.
        /// </summary>
		public const string ArgumentNullException_MessageFormat = "Value for this parameter is null or empty '{0}'. Provide value instead.";

        /// <summary>
        /// File '{0}' extension should end with dot and have 3 to 4 letters. Provide valid extension instead.
        /// </summary>
		public const string FormatException_MessageFormat = "File '{0}' extension should end with dot and have 3 to 4 letters. Provide valid extension instead.";

        /// <summary>
        /// Configuration error: '{0}' key value is missing '{1}' variable that should be put after '{2}' flag.
        /// </summary>
		public const string ConfigurationErrors_MessageFormat = "Configuration error: '{0}' key value is missing '{1}' variable that should be put after '{2}' flag";

        /// <summary>
        /// This operation is invalid because parameter '{0}' is set to 'false'.\r\nTo perform this operation set this paramter to 'true'.
        /// </summary>
		public const string InvalidOperationExceptionMessageFormat = "This operation is invalid because parameter '{0}' is set to 'false'.\r\nTo perform this operation set this paramter to 'true'";

        /// <summary>
        /// This operation is invalid because operation type '{0}' does not allow following clauses: '{1}', '{2}'. They are reeserved for DELETE or UPDATE.
        /// </summary>
        public const string InvalidOperationExceptionMessageFormat2 = "This operation is invalid because operation type '{0}' does not allow following clauses: '{1}', '{2}'. They are reeserved for DELETE or UPDATE";

        /// <summary>
        /// Number of supplied '{0}' per row exceeds the number of specified '{1}'.
        /// </summary>
        public const string InvalidOperationExceptionMessageFormat3 = "Number of supplied '{0}' per row exceeds the number of specified '{1}'";

        /// <summary>
        /// Possible SQL Injection attact: '{0}'.
        /// </summary>
        public const string SqlInjectionException_MessageFormat = "Possible SQL Injection attact: '{0}'";

        /// <summary>
        /// Future reserved feature.
        /// </summary>
        public const string NotImplementedException_MessageFormat = "Future reserved feature";

        /// <summary>
        /// Formats exception message according to provided format. 
        /// </summary>
        /// <param name="format">exception message format</param>
        /// <param name="args">exception message format placeholders values array</param>
        /// <returns></returns>
        public static string Format(string format, params object[] args) 
		{
			return string.Format(format, args); 
		}

        /// <summary>
        /// Creates exception message with given format and values.
        /// </summary>
        /// <param name="format">exception message format</param>
        /// <param name="args">exception message format placeholders values array</param>
        /// <returns></returns>
        public static Exception CreateException(string format, params object[] args)
        {
            string outputFormat = Format(format, args);
            switch (format)
            {
                case ArgumentNullException_MessageFormat:
                        return new ArgumentException(outputFormat);
                case FormatException_MessageFormat:
                        return new FormatException(outputFormat);
                case ConfigurationErrors_MessageFormat:
                        return new ConfigurationErrorsException(outputFormat);
                case InvalidOperationExceptionMessageFormat:
                        return new InvalidOperationException(outputFormat);
                case InvalidOperationExceptionMessageFormat2:
                    return new InvalidOperationException(outputFormat);
                case InvalidOperationExceptionMessageFormat3:
                    return new InvalidOperationException(outputFormat);
                case SqlInjectionException_MessageFormat:
                    return new Exception(outputFormat);
                case NotImplementedException_MessageFormat:
                    return new NotImplementedException(outputFormat);
                default:
                        return new Exception(outputFormat);
            }

        }
	}
}