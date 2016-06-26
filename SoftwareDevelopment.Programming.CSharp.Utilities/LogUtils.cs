using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        /// <summary>
        /// Converts DateTime object to string representation, e.g. 2016-03-24 18:41:00:234
        /// </summary>
        public const string DATETIME_PATTERN_FOR_LOGGING = "yyyy-MM-dd HH:mm:sss:fff";

        private static DateTime _startDate = DateTime.MinValue;
		private static DateTime _endDate = DateTime.MinValue;

		private static StringBuilder _inMemoryLoggerStorage = new StringBuilder();
		private static bool _redirectToInMemoryLogger = false;

        private static string _textFileLoggerStorage = String.Empty;
        private static bool _redirectToTextFileLogger = false;

        private static SqlConnection _databaseConnection = null;
        private static int _commandTimeout = -1;
        private static string _databaseTableNameLoggerStorage = String.Empty;
        private static bool _redirectToDatabaseLogger = false;

        private static bool _applySystemLogging = false;


        private static EventLog _eventLog = null;
        private static WindowsEventLogType _eventLogType = WindowsEventLogType.Application;
        private static bool _redirectToWindowsEventLog = false;


        /// <summary>
        /// Applies system logging, [full date and time]  [type of logging] [full path to method]: user logging goes here.
        /// Otherwise user custom logging is applied.
        /// </summary>
        /// <param name="apply">whether to apply system logging or not</param>
        /// <returns>void</returns>
        public static void ApplySystemLogging(bool apply)
        {
            _applySystemLogging = apply;
        }

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
            }
            _redirectToTextFileLogger = redirect;
        }

        /// <summary>
        /// Redirects all logging output to database table making it default or one of the logging outputs for the whole of the running program.
        /// </summary>
        /// <param name="connectionString">connection string to database</param>
        /// <param name="loggerTableName">database table name</param>
        /// <param name="commandTimeout">timeout for operation completion</param>
        /// <param name="redirect">whether to redirect logging output to database or not</param>
        /// <returns>void</returns>
        public static void RedirectToDatabaseLogger(string connectionString, string loggerTableName, int commandTimeout, bool redirect)
        {
            if (redirect)
            {
                if (String.IsNullOrEmpty(connectionString))
                    throw ExceptionUtils.CreateException("Connection string is null or empty. Provide valid connection string instead.");

                if (String.IsNullOrEmpty(loggerTableName))
                    throw ExceptionUtils.CreateException("Table name for storing log data is null or empty. Provide valid table name instead.");

                _databaseConnection = DatabaseUtils.CreateAndOptionallyOpenSqlServerConnection(connectionString, true);

               string validationMessage;
               bool validationPassed = ValidateLoggerTableSchema(loggerTableName, commandTimeout, out validationMessage);
               if (!validationPassed)
               {
                  DatabaseUtils.CloseSqlServerDatabaseConnection(ref _databaseConnection);
                  throw ExceptionUtils.CreateException("Invalid table schema: '{0}' in log table called '{1}'", validationMessage, loggerTableName);
               }
            }
            _commandTimeout = commandTimeout;
            _databaseTableNameLoggerStorage = loggerTableName;
            _redirectToDatabaseLogger = redirect;
        }


        /// <summary>
        /// Redirects all logging output to Windows event log making it default or one of the logging outputs for the whole of the running program.
        /// </summary>
        /// <param name="source">source name to register</param>
        /// <param name="redirect">whether to redirect logging output to Windows event log or not</param>
        /// <param name="machineName">specifies name of a computer on which to read from or write to events</param>
        /// <param name="maximumKilobytes">specifies maximum log size</param>
        /// <param name="windowsEventLogType">specifies type of the EventLog</param>
        /// <param name="modifyOverflowPolicy">specifies whether to modify overflow policy</param>
        /// <param name="overflowAction">specifies how to deal with new entries when current log reaches its maximum size. This parameter is considered only if modifyOverflowPolicy is set to true</param>
        /// <param name="retentionDays">specifies number of days to retain entries in the current log. This parameter is considered only if modifyOverflowPolicy is set to true</param>
        public static void RedirectToWindowsEventLog(string source, bool redirect, string machineName = "", long maximumKilobytes = -1, WindowsEventLogType windowsEventLogType = WindowsEventLogType.Application,
                                                     bool modifyOverflowPolicy = false, OverflowAction overflowAction = OverflowAction.OverwriteAsNeeded, int retentionDays = -1)
        {
            if (redirect)
            {
                _eventLog = new EventLog();

                _eventLog.Source = source;

                if (!String.IsNullOrEmpty(machineName))
                    _eventLog.MachineName = machineName;

                if(maximumKilobytes != -1)
                _eventLog.MaximumKilobytes = maximumKilobytes;

                _eventLogType = windowsEventLogType;
                _eventLog.Log = _eventLogType.ToString();

                if (modifyOverflowPolicy)
                    _eventLog.ModifyOverflowPolicy(overflowAction, retentionDays);
            }
            _redirectToWindowsEventLog = redirect;
        }

        /// <summary>
        /// Changes Windows EventLog type to which write or read from. 
        /// </summary>
        /// <param name="windowsEventLogType">specifies type of the EventLog</param>
        /// <param name="modifyOverflowPolicy">specifies whether to modify overflow policy</param>
        /// <param name="overflowAction">specifies how to deal with new entries when current log reaches its maximum size. This parameter is considered only if modifyOverflowPolicy is set to true</param>
        /// <param name="retentionDays">specifies number of days to retain entries in the current log. This parameter is considered only if modifyOverflowPolicy is set to true</param>
        public static void ChangeWindowsEventLogType(WindowsEventLogType windowsEventLogType, bool modifyOverflowPolicy = false, OverflowAction overflowAction = OverflowAction.OverwriteAsNeeded, int retentionDays = -1)
        {
            if(!_redirectToWindowsEventLog)
                throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "_redirectToWindowsEventLog");

            _eventLogType = windowsEventLogType;
            _eventLog.Log = _eventLogType.ToString();

            if (modifyOverflowPolicy)
                _eventLog.ModifyOverflowPolicy(overflowAction, retentionDays);
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
        /// Fetches logger output.
        /// </summary>
        /// <param name="onlyCreatedByThisProgram">specifies whether to return entiries created by this program only or all entries from the log they were written to</param>
        /// <returns>logger content</returns>
        public static List<EventLogEntry> FetchWindowsEventLogOuput(bool onlyCreatedByThisProgram = true)
        {
            if (!_redirectToWindowsEventLog)
                throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "_redirectToWindowsEventLog");

            List<EventLogEntry> list = new List<EventLogEntry>();

            if (onlyCreatedByThisProgram)
            {
                foreach (EventLogEntry item in _eventLog.Entries)
                {
                    if(item.TimeWritten >= _startDate)
                        list.Add(item);
                }
            }
            else
            {
                foreach (EventLogEntry item in _eventLog.Entries)
                {
                    list.Add(item);
                }
            }

            return list;
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
        /// Clears logger content, so that it can be reused.
        /// <param name="releaseEventLogAcquiredResources">specifies whether to release resources' handles used by this EventLog instance</param>
        /// </summary>
        public static void ClearWindowsEventLog(bool releaseEventLogAcquiredResources = false)
        {
            {
                if (!_redirectToWindowsEventLog)
                    throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "_redirectToWindowsEventLog");

                _eventLog.Clear();

                if (releaseEventLogAcquiredResources)
                    ReleaseWindowsEventLogAcquiredResources();
            }
        }


        /// <summary>
        /// Releases resources' handles used by this EventLog instance
        /// </summary>
        public static void ReleaseWindowsEventLogAcquiredResources()
        {
            if (!_redirectToWindowsEventLog)
                throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "_redirectToWindowsEventLog");

            _eventLog.Close();
        }


        /// <summary>
        /// Logs start time of the operation with logging options.
        /// Logging options will be applied if system logging is active. Otherwise user custom logging takes place.
        /// You can activate system logging with ApplySystemLogging(bool apply) method.
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        /// </summary>
        public static void LogStartTime(LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true)
		{
			_startDate = DateTime.Now;
			Log("Service started at: {0}", true, logOperationType, includeInvocationTime, _startDate.ToString(DATETIME_PATTERN));
		}

        /// <summary>
        /// Logs end time of the operation with logging options.
        /// Logging options will be applied if system logging is active. Otherwise user custom logging takes place.
        /// You can activate system logging with ApplySystemLogging(bool apply) method.
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        /// </summary>
        public static void LogEndTime(LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true)
		{
			_endDate = DateTime.Now;
            Log("Service ended at: {0}", true, logOperationType, includeInvocationTime, _endDate.ToString(DATETIME_PATTERN));
            Log("Operation took {0} seconds", true, logOperationType, includeInvocationTime, _endDate.Subtract(_startDate).TotalSeconds.ToString());
        }

        /// <summary>
        /// Logs message to the default output.
        /// Logging options will be applied if system logging is active. Otherwise user custom logging takes place.
        /// You can activate system logging with ApplySystemLogging(bool apply) method.
        /// </summary>
        /// <param name="format">format of a message</param>
        /// <param name="goToNewLine">whether to break the line or not</param>
        /// <param name="formatParameters">values for format parameter</param>
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        /// <returns>void</returns>
        public static void Log(string format, bool goToNewLine, LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true, params string[] formatParameters)
        {
            if (_redirectToInMemoryLogger || _redirectToTextFileLogger || _redirectToDatabaseLogger)
            {
                if (_redirectToInMemoryLogger)
                    LogToInMemoryLogger(format, goToNewLine, logOperationType, includeInvocationTime, formatParameters);
                if(_redirectToTextFileLogger)
                    LogToTextFileLogger(format, goToNewLine, logOperationType, includeInvocationTime, formatParameters);
                if(_redirectToDatabaseLogger)
                    LogToDatabaseLogger(format, goToNewLine, logOperationType, includeInvocationTime, formatParameters);
                if(_redirectToWindowsEventLog)
                    LogToWindowsEventLog(format, goToNewLine, logOperationType, includeInvocationTime, formatParameters);
            }
            else
                LogToConsoleOutput(format, goToNewLine, logOperationType, includeInvocationTime, formatParameters);
        }

        /// <summary>
        /// Appends 'numberOfLines' empty lines to the current log. With default paramters invocation [ ComposeLoggingOutputLayout() ] it acts like deprecated metohod MoveToTheNextSection.
        /// In a nutchel what is does is the following: it adds horizontally or vertically number of spaces or tabulators to the appropriate logger or loggers depending on which loggers are active.
        /// Logging options will be applied if system logging is active. Otherwise user custom logging takes place.
        /// You can activate system logging with ApplySystemLogging(bool apply) method.
        /// </summary>
        /// <param name="numberOfLines">number of lines to move cursor downward</param>
        /// <param name="applyPreviousParameterValueOfTabulatorsInstead">whether to append tabulators instead of empty strings</param>
        /// <param name="goToNewLine">whether to break the line or not</param>
        /// <param name="applyOneSpaceString">whether to append one space striing instead of empty string</param>
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        /// <returns>void</returns>
        public static void ComposeLoggingOutputLayout(
                                                      int numberOfLines = 2, bool applyPreviousParameterValueOfTabulatorsInstead = false, bool goToNewLine = true, bool applyOneSpaceString = false,
                                                      LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true
                                                     )
        {
            const string oneSpace = " ";
            if (applyPreviousParameterValueOfTabulatorsInstead)
            {
                if (_redirectToInMemoryLogger || _redirectToTextFileLogger || _redirectToDatabaseLogger)
                {
                    if (_redirectToInMemoryLogger)
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToInMemoryLogger("\t", false, logOperationType, includeInvocationTime);
                        }
                        if (goToNewLine)
                            LogToInMemoryLogger(String.Empty, true, logOperationType, includeInvocationTime);
                    }
                    if (_redirectToTextFileLogger)
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToTextFileLogger("\t", false, logOperationType, includeInvocationTime);
                        }
                        if (goToNewLine)
                            LogToTextFileLogger(String.Empty, true, logOperationType, includeInvocationTime);
                    }
                    if (_redirectToDatabaseLogger)
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToDatabaseLogger("\t", false, logOperationType, includeInvocationTime);
                        }
                        if (goToNewLine)
                            LogToDatabaseLogger(String.Empty, true, logOperationType, includeInvocationTime);
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfLines; i++)
                    {
                        LogToConsoleOutput("\t", false, logOperationType, includeInvocationTime);
                    }
                    if (goToNewLine)
                        LogToConsoleOutput(String.Empty, true, logOperationType, includeInvocationTime);
                }
            }
            else
            {
                if (_redirectToInMemoryLogger || _redirectToTextFileLogger || _redirectToDatabaseLogger)
                {
                    if (_redirectToInMemoryLogger)
                    {
                        if (applyOneSpaceString)
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToInMemoryLogger(oneSpace, false, logOperationType, includeInvocationTime);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToInMemoryLogger(String.Empty, true, logOperationType, includeInvocationTime);
                            }
                        }
                        if (goToNewLine)
                            LogToInMemoryLogger(String.Empty, true, logOperationType, includeInvocationTime);
                    }
                    if (_redirectToTextFileLogger)
                    {
                        if (applyOneSpaceString)
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToTextFileLogger(oneSpace, false, logOperationType, includeInvocationTime);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToTextFileLogger(String.Empty, true, logOperationType, includeInvocationTime);
                            }
                        }
                        if (goToNewLine)
                            LogToTextFileLogger(String.Empty, true, logOperationType, includeInvocationTime);
                    }
                    if (_redirectToDatabaseLogger)
                    {
                        if (applyOneSpaceString)
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToDatabaseLogger(oneSpace, false, logOperationType, includeInvocationTime);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < numberOfLines; i++)
                            {
                                LogToDatabaseLogger(String.Empty, true, logOperationType, includeInvocationTime);
                            }
                        }
                        if (goToNewLine)
                            LogToDatabaseLogger(String.Empty, true, logOperationType, includeInvocationTime);
                    }
                }
                else
                {
                    if (applyOneSpaceString)
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToConsoleOutput(oneSpace, false, logOperationType, includeInvocationTime);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < numberOfLines; i++)
                        {
                            LogToConsoleOutput(String.Empty, true, logOperationType, includeInvocationTime);
                        }
                    }
                    if (goToNewLine)
                        LogToConsoleOutput(String.Empty, true, logOperationType, includeInvocationTime);
                }
            }
        }

        /// <summary>
        /// Logs summary info about number of records to insert, update, delete, log.
        /// Can be used for any other type of logging. The idea is to provide some kind of summary information.
        /// Logging options will be applied if system logging is active. Otherwise user custom logging takes place.
        /// You can activate system logging with ApplySystemLogging(bool apply) method.
        /// </summary>
        /// <param name="labelValueItemsToInsert">records to insert - description + number of records (in the shape of key-value pairs: new string[] {"new items added", "5", ...})</param>
        /// <param name="labelValueItemsToUpdate">records to update - description + number of records (in the shape of key-value pairs: new string[] {"old items updated", "3", ...})</param>
        /// <param name="labelValueItemsToDelete">records to delete - description + number of records (in the shape of key-value pairs: new string[] {"old items deleted", "2", ...})</param>
        /// <param name="labelValueItemsToLog">records to log - description + number of records (in the shape of key-value pairs: new string[] {"new items logged", "7", ...})</param>
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        /// <returns>void</returns>
        public static void LogSummary(
                                       string[] labelValueItemsToInsert, string[] labelValueItemsToUpdate, string[] labelValueItemsToDelete, string[] labelValueItemsToLog,
                                       LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true
                                     )
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

            ProcessLoop(labelValueItemsToInsert, insertLength, OperationTypeEnum.INSERT, logOperationType, includeInvocationTime);
            ProcessLoop(labelValueItemsToUpdate, updateLength, OperationTypeEnum.UPDATE, logOperationType, includeInvocationTime);
            ProcessLoop(labelValueItemsToDelete, deleteLength, OperationTypeEnum.DELETE, logOperationType, includeInvocationTime);
            ProcessLoop(labelValueItemsToLog, logLength, OperationTypeEnum.LOG, logOperationType, includeInvocationTime);
        }

        /// <summary>
        /// Logs object to active log storage or storages.
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="item">object instance</param>
        /// <param name="applyUserCustomFormat">specifies whether apply user custom format or system default format</param>
        /// <param name="goToNewLine">whether to break the line or not</param>
        /// <param name="format">user custom format</param>
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        /// <param name="formatParameters">values for format parameter</param>
        public static void LogObjectOnSuccess<T>(T item, bool applyUserCustomFormat, bool goToNewLine, string format = "",
                                                 LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true, params string[] formatParameters) where T : class, new()
        {
            if (applyUserCustomFormat)
            {
                LogUtils.Log(format, goToNewLine, logOperationType, includeInvocationTime, formatParameters);
            }
            else
            {
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

                StringBuilder objectLogBuilder = new StringBuilder();

                objectLogBuilder.AppendFormat("Object of type: {0}, properties: ", type.FullName);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    objectLogBuilder.AppendFormat(propertyInfo.Name + " = '{0}', ", propertyInfo.GetValue(item, null));
                }

                string loggedObjectToString = objectLogBuilder.ToString();
                loggedObjectToString = loggedObjectToString.Substring(0, loggedObjectToString.Length - 2) + " was logged on successful operation invocation.";
                

                LogUtils.Log(loggedObjectToString, goToNewLine, logOperationType, includeInvocationTime, String.Empty);
            }
        }


        /// <summary>
        /// Logs object to active log storage or storages.
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="item">object instance</param>
        /// <param name="exceptionMessage">exception message</param>
        /// <param name="applyUserCustomFormat">specifies whether apply user custom format or system default format</param>
        /// <param name="goToNewLine">whether to break the line or not</param>
        /// <param name="format">user custom format</param>
        /// <param name="logOperationType">specifies type of operation</param>
        /// <param name="includeInvocationTime">specifies whether to include timestamp</param>
        /// <param name="formatParameters">values for format parameter</param>
        public static void LogObjectOnFailure<T>(T item, string exceptionMessage, bool applyUserCustomFormat, bool goToNewLine, string format = "",
                                                 LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true, params string[] formatParameters) where T : class, new()
        {
            if (applyUserCustomFormat)
            {
                LogUtils.Log(format, goToNewLine, logOperationType, includeInvocationTime, formatParameters);
            }
            else
            {
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

                StringBuilder objectLogBuilder = new StringBuilder();

                objectLogBuilder.AppendFormat("Object of type: {0}, properties: ", type.FullName);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    objectLogBuilder.AppendFormat(propertyInfo.Name + " = '{0}', ", propertyInfo.GetValue(item, null));
                }

                string loggedObjectToString = objectLogBuilder.ToString();
                loggedObjectToString = loggedObjectToString.Substring(0, loggedObjectToString.Length - 2) + " was logged on failed operation invocation. Exception message: '" + exceptionMessage + "'.";


                LogUtils.Log(loggedObjectToString, goToNewLine, logOperationType, includeInvocationTime, String.Empty);
            }
        }


        private static void LogToInMemoryLogger(string format, bool goToNewLine, LogOperationTypeEnum logOperationType, bool includeInvocationTime, params string[] formatParameters)
        {
            if (_applySystemLogging)
            {
                string currentMethodStackTrace = GetCurrentInvokedMethodStackTrace(logOperationType, includeInvocationTime, true).Replace("#", ":");
                if (goToNewLine)
                    _inMemoryLoggerStorage.Append(currentMethodStackTrace + ":\t").AppendFormat(format, formatParameters).AppendLine();
                else
                    _inMemoryLoggerStorage.Append(currentMethodStackTrace).AppendFormat(format, formatParameters);
            }
            else
            {
                if (goToNewLine)
                    _inMemoryLoggerStorage.AppendFormat(format, formatParameters).AppendLine();
                else
                    _inMemoryLoggerStorage.AppendFormat(format, formatParameters);
            }
        }

        private static void LogToTextFileLogger(string format, bool goToNewLine, LogOperationTypeEnum logOperationType, bool includeInvocationTime, params string[] formatParameters)
        {
            if (_applySystemLogging)
            {
                StreamWriter writer = File.AppendText(_textFileLoggerStorage);
                writer.AutoFlush = true;

                string currentMethodStackTrace = GetCurrentInvokedMethodStackTrace(logOperationType, includeInvocationTime, true).Replace("#", ":");
                string content = currentMethodStackTrace + ":\t" + String.Format(format, formatParameters);

                if (goToNewLine)
                    writer.WriteLine(content);
                else
                    writer.Write(content);

                writer.Close();
            }
            else
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
        }

        private static void LogToDatabaseLogger(string format, bool goToNewLine, LogOperationTypeEnum logOperationType, bool includeInvocationTime, params string[] formatParameters)
        {
            if (_applySystemLogging)
            {
                string currentMethodStackTrace = GetCurrentInvokedMethodStackTrace(logOperationType, includeInvocationTime, true) + ":\t";
                string log = TransformToTableRow(currentMethodStackTrace, String.Format(format, formatParameters));

                DatabaseUtils.CreateSqlCommand(_databaseConnection, log, CommandType.Text, _commandTimeout).ExecuteNonQuery();
            }
            else
            {
                string log = TransformToTableRow(String.Empty, String.Format(format, formatParameters));
                DatabaseUtils.CreateSqlCommand(_databaseConnection, log, CommandType.Text, _commandTimeout).ExecuteNonQuery();
            }
        }

        private static void LogToWindowsEventLog(string format, bool goToNewLine, LogOperationTypeEnum logOperationType, bool includeInvocationTime, string[] formatParameters)
        {
            EventLogEntryType eventLogEntryType;

            if (logOperationType == LogOperationTypeEnum.INFO)
                eventLogEntryType = EventLogEntryType.Information;
            else if (logOperationType == LogOperationTypeEnum.WARNING)
                eventLogEntryType = EventLogEntryType.Warning;
            else if (logOperationType == LogOperationTypeEnum.ERROR)
                eventLogEntryType = EventLogEntryType.Error;
            else
                eventLogEntryType = EventLogEntryType.Information;

            if (_applySystemLogging)
            {
                string currentMethodStackTrace = GetCurrentInvokedMethodStackTrace(logOperationType, includeInvocationTime, true).Replace("#", ":");
                string content = currentMethodStackTrace + ":\t" + String.Format(format, formatParameters);

                _eventLog.WriteEntry(content, eventLogEntryType);
            }
            else
            {
                _eventLog.WriteEntry(String.Format(format, formatParameters), eventLogEntryType);
            }
        }

        private static void LogToConsoleOutput(string format, bool goToNewLine, LogOperationTypeEnum logOperationType, bool includeInvocationTime, params string[] formatParameters)
        {
            if (_applySystemLogging)
            {
                string currentMethodStackTrace = GetCurrentInvokedMethodStackTrace(logOperationType, includeInvocationTime).Replace("#", ":");

                if (goToNewLine)
                    Console.WriteLine(currentMethodStackTrace + string.Format(format, formatParameters));
                else
                    Console.Write(currentMethodStackTrace + string.Format(format, formatParameters));
            }
            else
            {
                if (goToNewLine)
                    Console.WriteLine(string.Format(format, formatParameters));
                else
                    Console.Write(string.Format(format, formatParameters));
            }
        }

        private static void ProcessLoop(string[] items, int length, OperationTypeEnum operationType, LogOperationTypeEnum logOperationType, bool includeInvocationTime)
        {
            string insertMessageFormat = "Records to insert ({0}): {1}";
            string updateMessageFormat = "Records to update ({0}): {1}";
            string deleteMessageFormat = "Records to delete ({0}): {1}";
            string logMessageFormat = "Records to log ({0}): {1}";

            if (operationType == OperationTypeEnum.INSERT)
            {
                SendDataToOutput(insertMessageFormat, items, length, logOperationType, includeInvocationTime);
            }
            else if (operationType == OperationTypeEnum.UPDATE)
            {
                SendDataToOutput(updateMessageFormat, items, length, logOperationType, includeInvocationTime);
            }
            else if (operationType == OperationTypeEnum.DELETE)
            {
                SendDataToOutput(deleteMessageFormat, items, length, logOperationType, includeInvocationTime);
            }
            else if (operationType == OperationTypeEnum.LOG)
            {
                SendDataToOutput(logMessageFormat, items, length, logOperationType, includeInvocationTime);
            }
        }

        private static void SendDataToOutput(string format, string[] items, int length, LogOperationTypeEnum logOperationType, bool includeInvocationTime)
        {
           for (int i = 0; i < length; i += 2)
           {
                Log(format, true, logOperationType, includeInvocationTime, items[i], items[i + 1]);
           }
        }

        private static bool ValidateLoggerTableSchema(string loggerTableName, int commandTimeout, out string validationMessage)
        {
            string commandText = String.Format(@"
                                                    DECLARE @VALIDATION_MESSAGE AS VARCHAR(MAX) = ''

													IF NOT EXISTS (
														SELECT 1/0 FROM INFORMATION_SCHEMA.COLUMNS AS ISC WHERE ISC.TABLE_NAME = '{0}'
													)
													 SELECT @VALIDATION_MESSAGE = 'Table of logs does not exist.'

													IF (SELECT COUNT(ISC.ORDINAL_POSITION) FROM INFORMATION_SCHEMA.COLUMNS AS ISC WHERE ISC.TABLE_NAME = '{0}') < 4
													 SELECT @VALIDATION_MESSAGE = 'Table of logs has to have 4 columns of type DATETIME or DATETIME2, VARCHAR(7), VARCHAR(MAX), VARCHAR(MAX) respectively.'
													ELSE
													 BEGIN
														SELECT 
															@VALIDATION_MESSAGE = @VALIDATION_MESSAGE +
															CASE
															 WHEN ISC.ORDINAL_POSITION = 1 AND ISC.DATA_TYPE NOT IN ('datetime', 'datetime2')
															  THEN 'First column has to be of type datetime or datetime2, '
															 WHEN ISC.ORDINAL_POSITION = 2 AND (ISC.DATA_TYPE <> 'varchar' OR ISC.CHARACTER_MAXIMUM_LENGTH < 7)
															  THEN 'Second column has to be of type varchar(7), '
															 WHEN ISC.ORDINAL_POSITION IN (3, 4) AND (ISC.DATA_TYPE <> 'varchar' OR ISC.CHARACTER_MAXIMUM_LENGTH <> -1)
															  THEN 'Third or fourth column has to be of type varchar(MAX)'
															 ELSE ''
															END 
														FROM INFORMATION_SCHEMA.COLUMNS AS ISC
														WHERE ISC.TABLE_NAME = '{0}'
														ORDER BY ISC.ORDINAL_POSITION

														IF LEFT(@VALIDATION_MESSAGE, 1) = 'F'
														 BEGIN
															SET @VALIDATION_MESSAGE = REPLACE(@VALIDATION_MESSAGE, 'Second', 'second')
															SET @VALIDATION_MESSAGE = REPLACE(@VALIDATION_MESSAGE, 'Third', 'third')
														 END
														IF LEFT(@VALIDATION_MESSAGE, 1) = 'S'
														 SET @VALIDATION_MESSAGE = REPLACE(@VALIDATION_MESSAGE, 'Third', 'third')
													 END

                                                    SELECT @VALIDATION_MESSAGE
                                                ",
                                                loggerTableName
                                              );

            validationMessage = (string)DatabaseUtils.CreateSqlCommand(_databaseConnection, commandText, CommandType.Text, commandTimeout).ExecuteScalar();

            if (String.IsNullOrEmpty(validationMessage))
                return true;
            return false;
        }

        private static string TransformToTableRow(string currentMethodStackTrace, string userLog)
        {
            string logTableRowFormat = String.Empty;
            string[] values = new string[4];

            if (String.IsNullOrEmpty(currentMethodStackTrace))
            {
                values[0] = "NULL";
                values[1] = values[2] = "'" + String.Empty + "'";
                values[3] = "'" + userLog + "'";
            }
            else
            {
                string[] systemData = currentMethodStackTrace.Split(new[] { '\t' });
                values[0] = "'" + systemData[0].Replace("[", String.Empty).Replace("]", String.Empty) + "'";
                values[1] = "'" + systemData[1].Replace("[", String.Empty).Replace("]", String.Empty) + "'";
                values[2] = "'" + systemData[2].Replace(":", String.Empty).Replace("#", ":") + "'";
                values[3] = "'" + userLog + "'";
            }

            logTableRowFormat = "INSERT " + _databaseTableNameLoggerStorage + " VALUES (" + MiscUtils.StringJoin(",", values) + ")";

            return logTableRowFormat;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetCurrentInvokedMethodStackTrace(LogOperationTypeEnum logOperationType = LogOperationTypeEnum.INFO, bool includeInvocationTime = true, bool reverseFrameStackOrder = false)
        {
            string path = String.Empty;
            IList<string> methodInvocationStack = new List<string>();

            if (includeInvocationTime)
                path += ("[" + DateTimeUtils.DateTimeToString(DATETIME_PATTERN_FOR_LOGGING, DateTime.Now) + "]\t");

            path += ("[" + logOperationType + "]\t");

            StackTrace stackTrace = new StackTrace(true);
            StackFrame stackFrame = null;
            MethodBase methodBase = null;
            for (int i = 3; i < stackTrace.FrameCount; i++)
            {
                stackFrame = stackTrace.GetFrame(i);
                methodBase = stackFrame.GetMethod();
                if (methodBase.Name == "_nExecuteAssembly")
                    break;
                methodInvocationStack.Add("[" + methodBase.DeclaringType.FullName + "." + methodBase.Name + "# " + stackFrame.GetFileLineNumber() + "]");
            }

            string[] methodInvocationArray = MiscUtils.ConvertListToArray<string>(methodInvocationStack);
            if(reverseFrameStackOrder)
                Array.Reverse(methodInvocationArray);

            path += MiscUtils.StringJoin(".", methodInvocationArray);

            return path; 
        }
    }
}
