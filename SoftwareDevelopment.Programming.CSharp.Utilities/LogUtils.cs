using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common logging operations.
    /// </summary>
    [Obfuscation(ApplyToMembers=true, Exclude=false, StripAfterObfuscation=true)]
    public static class LogUtils
	{
        /// <summary>
        /// Converts DateTime object to string representation, e.g. 2016-03-24 18:41:00
        /// </summary>
		public const string DATETIME_PATTERN = "yyyy-MM-dd HH:mm:sss";
        /// <summary>
        /// Converts DateTime object to string representation, e.g. 2016-03-24
        /// </summary>
        public const string DATE_PATTERN = "yyyy-MM-dd";
        /// <summary>
        /// Converts DateTime object to string representation, e.g. 2016-03-24 18:41:00:234
        /// </summary>
        public const string DATETIME_PATTERN_FOR_VERSIONING = "yyyy-MM-dd-HH-mm-sss-fff";

        private static DateTime _startDate = DateTime.MinValue;
		private static DateTime _endDate = DateTime.MinValue;

		private static StringBuilder _inMemoryLoggerStorage = new StringBuilder();
		private static bool _redirectToInMemoryLogger = false;

        private static string _textFileLoggerStorage = String.Empty;
        private static bool _redirectToTextFileLogger = false;


        /// <summary>
        /// Redirects all logging output to custom location making it default logging output for the whole of the running program.
        /// </summary>
        /// <param name="redirect">whether to redirect logging output to custom location or not</param>
        /// <returns>void</returns>

        [Obsolete("This method will be removed in the future releases of this library. Please use RedirectToInMemoryLogger method instead.")]
        public static void RedirectToCustomOutput(bool redirect)
		{
            RedirectToInMemoryLogger(redirect);
		}


        /// <summary>
        /// Redirects all logging output to memory location making it default or one of the logging outputs for the whole of the running program.
        /// </summary>
        /// <param name="redirect">whether to redirect logging output to memory location or not</param>
        /// <returns>void</returns>
        public static void RedirectToInMemoryLogger(bool redirect)
        {
            _redirectToInMemoryLogger = redirect;
        }

        /// <summary>
        /// Redirects all logging output to file making it default or one of the logging outputs for the whole of the running program.
        /// </summary>
        /// <param name="fullPathToFile">full path to a log file</param>
        /// <param name="redirect">whether to redirect logging output to a file location or not</param>
        /// <param name="appendExecutionInvocationTimestamp">whether to append execution invocation date and time suffix</param>
        /// <returns>void</returns>
        public static void RedirectToTextFileLogger(string fullPathToFile, bool redirect, bool appendExecutionInvocationTimestamp = false)
        {
            if (redirect)
            {
                string[] filePart = FileAndDirectoryUtils.GetFileNameAndExtensionSplitted(fullPathToFile);
                if (filePart.Length != 2)
                    throw ExceptionUtils.CreateException("Full path to file has to end with dot followed by name of the extension, i.e [.txt], [.log] etc.");

                string tempFileName = filePart[0];
                if (appendExecutionInvocationTimestamp)
                    tempFileName = FileAndDirectoryUtils.ComposeFileNameWithoutExtension(new[] { tempFileName, "_", DateTimeUtils.DateTimeToString(DATETIME_PATTERN_FOR_VERSIONING, DateTime.Now) });

                _textFileLoggerStorage = tempFileName + "." + filePart[1];
                FileAndDirectoryUtils.CreateOrOverrideExistingFile(_textFileLoggerStorage, false);
                _redirectToTextFileLogger = redirect;
            }
            else
                _redirectToTextFileLogger = redirect;
        }

        /// <summary>
        /// Fetches logger output.
        /// </summary>
        /// <returns>logger content</returns>

        [Obsolete("This method will be removed in the future releases of this library. Please use FetchInMemoryLoggerOuput method instead.")]
        public static string FetchLoggerOuput()
        {
            return FetchInMemoryLoggerOuput();
        }


        /// <summary>
        /// Fetches logger output.
        /// </summary>
        /// <returns>logger content</returns>
        public static string FetchInMemoryLoggerOuput()
        {
            if (!_redirectToInMemoryLogger)
                throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "_redirectToInMemoryLogger");
            return _inMemoryLoggerStorage.ToString();
        }

        /// <summary>
        /// Fetches logger output.
        /// </summary>
        /// <returns>logger content</returns>
        public static string FetchFileLoggerOuput()
        {
            if (!_redirectToTextFileLogger)
                throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "_redirectToTextFileLogger");
            return File.ReadAllText(_textFileLoggerStorage);
        }


        /// <summary>
        /// Clears logger content, so that it can be reused.
        /// </summary>
        public static void ClearInMemoryLogger()
        {
            {
                if (!_redirectToInMemoryLogger)
                    throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "_redirectToInMemoryLogger");
                _inMemoryLoggerStorage.Clear();
            }
        }


        /// <summary>
        /// Logs start time of the operation.
        /// </summary>
        public static void LogStartTime()
		{
			_startDate = DateTime.Now;
			Log("Service started at: {0}", true, _startDate.ToString(DATETIME_PATTERN));
		}

        /// <summary>
        /// Logs end time of the operation.
        /// </summary>
		public static void LogEndTime()
		{
			_endDate = DateTime.Now;
            Log("Service ended at: {0}", true, _endDate.ToString(DATETIME_PATTERN));
            Log("Operation took {0} seconds", true, _endDate.Subtract(_startDate).TotalSeconds.ToString());
        }

        /// <summary>
        /// Logs message to the default output.
        /// </summary>
        /// <param name="format">format of a message</param>
        /// <param name="goToNewLine">whether to break the line or not</param>
        /// <param name="formatParameters">values for format parameter</param>
        /// <returns>void</returns>
        public static void Log(string format, bool goToNewLine, params string[] formatParameters)
        {
            if (_redirectToInMemoryLogger || _redirectToTextFileLogger)
            {
                if (_redirectToInMemoryLogger)
                    LogToInMemoryLogger(format, goToNewLine, formatParameters);
                if(_redirectToTextFileLogger)
                    LogToTextFileLogger(format, goToNewLine, formatParameters);
            }
            else
                LogToConsoleOutput(format, goToNewLine, formatParameters);
        }

        /// <summary>
        /// Moves logging to the next paragraph.
        /// </summary>
        /// <param name="numberOfLines">number of lines to move cursor downward</param>
        [Obsolete("This method will be removed in the future releases of this library. Please use ComposeLoggingOutputLayout method instead.")]
        public static void MoveToTheNextSection(int numberOfLines = 2)
        {
            for (int i = 0; i < numberOfLines; i++)
            {
                LogToConsoleOutput(String.Empty, true);
            }
        }

        /// <summary>
        /// Appends 'numberOfLines' empty lines to the current log. With default paramters invocation [ ComposeLoggingOutputLayout() ] it acts like deprecated metohod MoveToTheNextSection.
        /// In a nutchel what is does is the following: it adds horizontally or vertically number of spaces or tabulators to the appropriate logger or loggers depending on which loggers are active.
        /// </summary>
        /// <param name="numberOfLines">number of lines to move cursor downward</param>
        /// <param name="applyPreviousParameterValueOfTabulatorsInstead">whether to append tabulators instead of empty strings</param>
        /// <param name="goToNewLine">whether to break the line or not</param>
        /// <param name="applyOneSpaceString">whether to append one space striing instead of empty string</param>
        /// <returns>void</returns>
        public static void ComposeLoggingOutputLayout(int numberOfLines = 2, bool applyPreviousParameterValueOfTabulatorsInstead = false, bool goToNewLine = true, bool applyOneSpaceString = false)
        {
            const string oneSpace = " ";
            if (applyPreviousParameterValueOfTabulatorsInstead)
            {
                if (_redirectToInMemoryLogger || _redirectToTextFileLogger)
                {
                    if (_redirectToInMemoryLogger)
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToInMemoryLogger("\t", false);
                        }
                        if (goToNewLine)
                            LogToInMemoryLogger(String.Empty, true);
                    }
                    if (_redirectToTextFileLogger)
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToTextFileLogger("\t", false);
                        }
                        if (goToNewLine)
                            LogToTextFileLogger(String.Empty, true);
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfLines; i++)
                    {
                        LogToConsoleOutput("\t", false);
                    }
                    if (goToNewLine)
                        LogToConsoleOutput(String.Empty, true);
                }
            }
            else
            {
                if (_redirectToInMemoryLogger || _redirectToTextFileLogger)
                {
                    if (_redirectToInMemoryLogger)
                    {
                        if (applyOneSpaceString)
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToInMemoryLogger(oneSpace, false);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToInMemoryLogger(String.Empty, true);
                            }
                        }
                        if (goToNewLine)
                            LogToInMemoryLogger(String.Empty, true);
                    }
                    if (_redirectToTextFileLogger)
                    {
                        if (applyOneSpaceString)
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToTextFileLogger(oneSpace, false);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToTextFileLogger(String.Empty, true);
                            }
                        }
                        if (goToNewLine)
                            LogToTextFileLogger(String.Empty, true);
                    }
                }
                else
                {
                    if (applyOneSpaceString)
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToConsoleOutput(oneSpace, false);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToConsoleOutput(String.Empty, true);
                        }
                    }
                    if (goToNewLine)
                        LogToConsoleOutput(String.Empty, true);
                }
            }
        }

        /// <summary>
        /// Logs summary info about number of records to insert, update, delete, log.
        /// Can be used for any other type of logging. The idea is to provide some kind of summary information.
        /// </summary>
        /// <param name="labelValueItemsToInsert">records to insert - description + number of records (in the shape of key-value pairs: new string[] {"new items added", "5", ...})</param>
        /// <param name="labelValueItemsToUpdate">records to update - description + number of records (in the shape of key-value pairs: new string[] {"old items updated", "3", ...})</param>
        /// <param name="labelValueItemsToDelete">records to delete - description + number of records (in the shape of key-value pairs: new string[] {"old items deleted", "2", ...})</param>
        /// <param name="labelValueItemsToLog">records to log - description + number of records (in the shape of key-value pairs: new string[] {"new items logged", "7", ...})</param>
        /// <returns>void</returns>
        public static void LogSummary(string[] labelValueItemsToInsert, string[] labelValueItemsToUpdate, string[] labelValueItemsToDelete, string[] labelValueItemsToLog)
        {
            if (labelValueItemsToInsert.Length % 2 != 0)
               throw ExceptionUtils.CreateException("Number of label - value INSERT pairs are odd. They have to be even.");
            if (labelValueItemsToUpdate.Length % 2 != 0)
               throw ExceptionUtils.CreateException("Number of label - value UPDATE pairs are odd. They have to be even.");
            if (labelValueItemsToDelete.Length % 2 != 0)
                throw ExceptionUtils.CreateException("Number of label - value DELETE pairs are odd. They have to be even.");
            if (labelValueItemsToLog.Length % 2 != 0)
                throw ExceptionUtils.CreateException("Number of label - value LOG pairs are odd. They have to be even.");

            int insertLength = labelValueItemsToInsert.Length;
            int updateLength = labelValueItemsToUpdate.Length;
            int deleteLength = labelValueItemsToDelete.Length;
            int logLength = labelValueItemsToLog.Length;

            ProcessLoop(labelValueItemsToInsert, insertLength, OperationTypeEnum.INSERT);
            ProcessLoop(labelValueItemsToUpdate, updateLength, OperationTypeEnum.UPDATE);
            ProcessLoop(labelValueItemsToDelete, deleteLength, OperationTypeEnum.DELETE);
            ProcessLoop(labelValueItemsToLog, logLength, OperationTypeEnum.LOG);
        }

        private static void LogToInMemoryLogger(string format, bool goToNewLine, params string[] formatParameters)
        {
            if (goToNewLine)
                _inMemoryLoggerStorage.AppendFormat(format, formatParameters).AppendLine();
            else
                _inMemoryLoggerStorage.AppendFormat(format, formatParameters);
        }

        private static void LogToTextFileLogger(string format, bool goToNewLine, params string[] formatParameters)
        {
            StreamWriter writer = File.AppendText(_textFileLoggerStorage);
            writer.AutoFlush = true;

            string content = String.Format(format, formatParameters);

            if (goToNewLine)
                writer.WriteLine(content);
            else
                writer.Write(content);

            writer.Close();
        }

        private static void LogToConsoleOutput(string format, bool goToNewLine, params string[] formatParameters)
        {
            if (goToNewLine)
                Console.WriteLine(string.Format(format, formatParameters));
            else
                Console.Write(string.Format(format, formatParameters));
        }

        private static void ProcessLoop(string[] items, int length, OperationTypeEnum operationType)
        {
            string insertMessageFormat = "Records to insert ({0}): {1}";
            string updateMessageFormat = "Records to update ({0}): {1}";
            string deleteMessageFormat = "Records to delete ({0}): {1}";
            string logMessageFormat = "Records to log ({0}): {1}";

            if (operationType == OperationTypeEnum.INSERT)
            {
                SendDataToOutput(insertMessageFormat, items, length);
            }
            else if (operationType == OperationTypeEnum.UPDATE)
            {
                SendDataToOutput(updateMessageFormat, items, length);
            }
            else if (operationType == OperationTypeEnum.DELETE)
            {
                SendDataToOutput(deleteMessageFormat, items, length);
            }
            else if (operationType == OperationTypeEnum.LOG)
            {
                SendDataToOutput(logMessageFormat, items, length);
            }
        }

        private static void SendDataToOutput(string format, string[] items, int length)
        {
           for (int i = 0; i < length; i += 2)
           {
                Log(format, true, items[i], items[i + 1]);
           }
        }
	}
}
