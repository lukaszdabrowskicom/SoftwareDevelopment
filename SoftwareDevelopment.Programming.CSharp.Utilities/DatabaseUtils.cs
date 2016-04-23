using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

using MySql.Data.MySqlClient;
using SoftwareDevelopment.Programming.CSharp.Utilities.DataObjects;
using System.Reflection;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common ADO .NET operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)] 
    public static class DatabaseUtils
    {

        /// <summary>
        /// Returns SqlConnection object.
        /// </summary>
        /// <param name="sqlConnectionString">connection string</param>
        /// <param name="openConnection">specifies whether additionaly open beforehand created connection</param>
        /// <returns>SqlConnection object</returns>
        public static SqlConnection CreateAndOptionallyOpenConnection(string sqlConnectionString, bool openConnection = false)
        {
            SqlConnection sqlConnection = new SqlConnection(sqlConnectionString);
            if (openConnection)
                OpenDatabaseConnection(sqlConnection, true);

            return sqlConnection;
        }

        /// <summary>
        /// Opens IDbConnection object.
        /// </summary>
        /// <param name="connection">connection object that is derived from IDbConnection</param>
        /// <param name="logOpendConnection">specifies whether put information about opening connection to the default application output</param>
        /// <returns>void</returns>
        public static void OpenDatabaseConnection(IDbConnection connection, bool logOpendConnection = false)
        {
            if ((connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                connection.Open();
                if(logOpendConnection)
                    LogUtils.Log("established and opened", true);
            }
        }


        /// <summary>
        /// Closes .NET sql wrapper for MySql connection.
        /// </summary>
        /// <param name="connection">reference to .NET sql wrapper for MySql connection</param>
        /// <returns>void</returns>
        public static void CloseMySqlDatabaseConnection(ref MySqlConnection connection)
        {
            if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Closes .NET sql wrapper for MySql connection.
        /// </summary>
        /// <param name="connection">reference to .NET sql wrapper for MySql connection</param>
        /// <param name="releaseResources">specifies whether release resources after closing connection</param>
        /// <returns>void</returns>
        public static void CloseMySqlDatabaseConnection(ref MySqlConnection connection, bool releaseResources)
        {
            if (releaseResources)
            {
                if (connection != null && ((connection.State & ConnectionState.Open) == ConnectionState.Open))
                {
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                }
                else if (connection != null && ((connection.State & ConnectionState.Closed) == ConnectionState.Closed))
                {
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                }
            }
            else
                CloseMySqlDatabaseConnection(ref connection);
        }

        /// <summary>
        /// Closes .NET connection.
        /// </summary>
        /// <param name="connection">.NET connection object to close</param>
        /// <returns>void</returns>
        public static void CloseSqlServerDatabaseConnection(ref SqlConnection connection)
        {
            if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Closes .NET connection.
        /// </summary>
        /// <param name="connection">.NET connection object to close</param>
        /// <param name="releaseResources">specifies whether release resources after closing connection</param>
        /// <returns>void</returns>
        public static void CloseSqlServerDatabaseConnection(ref SqlConnection connection, bool releaseResources)
        {
            if (releaseResources)
            {
                if (connection != null && ((connection.State & ConnectionState.Open) == ConnectionState.Open))
                {
                    connection.Close();
                    connection.Dispose();
                    connection = null;
                }
                else if (connection != null && ((connection.State & ConnectionState.Closed) == ConnectionState.Closed))
                {
                    connection.Dispose();
                    connection = null;
                }
            }
            else
                CloseSqlServerDatabaseConnection(ref connection);
        }

        /// <summary>
        /// Closes .NET sql reader.
        /// </summary>
        /// <param name="reader">.NET sql reader object to close</param>
        /// <returns>void</returns>
        public static void CloseDataReader(IDataReader reader)
        {
            if (!reader.IsClosed)
                reader.Close();
        }

        /// <summary>
        /// Converts sql query string containing FOR XML clause into sql query string, that is capable of returning XML data from within ADO. NET C# object.
        /// It simply creates T-SQL XML type variable that holds the xml output from query execution.
        /// In case sqlQuery variable does not contain FOR XML clause, appropriate exception is thrown.
        /// </summary>
        /// <param name="sqlQuery">sql input query string</param>
        /// <param name="xmlVariableNameToStoreXmlQuery">variable to store sql input query result</param>
        /// <param name="generateSelectQuery">whether to generate SQL SELECT clause</param>
        /// <returns>modified input query capable of returning xml data</returns>
        public static string ConvertSqlQueryIntoXmlCapableQuery(string sqlQuery, string xmlVariableNameToStoreXmlQuery = "@XML_QUERY_RESULT", bool generateSelectQuery = true)
        {
            string matchPattern = "for\\s+xml\\s+.+";
            bool isMatch = Regex.IsMatch(sqlQuery, matchPattern, RegexOptions.IgnoreCase);
            if (!isMatch)
                throw ExceptionUtils.CreateException("This query '{0}' is not xml query", sqlQuery);

            StringBuilder xmlBuilder = new StringBuilder();

            xmlBuilder.AppendFormat("DECLARE {0} AS XML = ''", xmlVariableNameToStoreXmlQuery).AppendLine();
            xmlBuilder.AppendFormat("SET {0} = ({1})", xmlVariableNameToStoreXmlQuery, sqlQuery).AppendLine();
            if(generateSelectQuery)
                xmlBuilder.AppendFormat("SELECT {0} AS {1}", xmlVariableNameToStoreXmlQuery, xmlVariableNameToStoreXmlQuery.Substring(1)).AppendLine();

            return xmlBuilder.ToString();
        }

        /// <summary>
        /// Creates valid T-SQL INSERT query in one of two ways: [INSERT ... VALUES(...)] or [INSERT .... SELECT ... UNION ALL ... SELECT ...].
        /// </summary>
        /// <param name="tableName">name of a table</param>
        /// <param name="arrayOfRowDataArray">array of arrays of row data</param>
        /// <param name="columnArray">array of columns</param>
        /// <param name="typeArray">array holding types for each column</param>
        /// <param name="operationTypeInsertType">type of INSERT</param>
        /// <returns>string containing INSERT query</returns>
        public static string CreateSQLInsertQuery(string tableName, string[] columnArray, string[][] arrayOfRowDataArray, Type[] typeArray, OperationTypeInsertType operationTypeInsertType = OperationTypeInsertType.INSERT_VALUES)
        {
            //perform initial validation
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(columnArray, typeArray, "columns" , "types");
            //perform validation for input data
            foreach (string[] rowDataArray in arrayOfRowDataArray)
            {
                ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(columnArray, rowDataArray, "values", "columns");
                ThrowExceptionForColumnValueValidationFailure(columnArray, rowDataArray, OperationTypeEnum.INSERT);
            }
            //validation passed

            //preparing string format for columns
            StringBuilder queryBuilder = new StringBuilder();
            string format = String.Empty;

            //create INSERT query data
            if (operationTypeInsertType == OperationTypeInsertType.INSERT_VALUES)
            {
                //create INSERT query body
                for (int i = 0, length = arrayOfRowDataArray.Length; i < length; i++)
                {
                    format = CreateStringInternal(OperationTypeEnum.INSERT, tableName, false, arrayOfRowDataArray[i], typeArray);
                    CreateSqlQueryInternal(queryBuilder, format);
                }
            }
            else if (operationTypeInsertType == OperationTypeInsertType.SELECT_UNION)
            {
                //create INSERT query header
                format = CreateStringInternal(OperationTypeEnum.INSERT, tableName, true, columnArray, typeArray, null, null, OperationTypeInsertType.SELECT_UNION);
                CreateSqlQueryInternal(queryBuilder, format);

                //create INSERT query body
                for (int i = 0, length = arrayOfRowDataArray.Length; i < length; i++)
                {
                    format = CreateStringInternal(OperationTypeEnum.INSERT, tableName, false, arrayOfRowDataArray[i], typeArray, null, null, OperationTypeInsertType.SELECT_UNION, i == length-1);
                    CreateSqlQueryInternal(queryBuilder, format);
                }
            }


            return queryBuilder.ToString();
        }


        /// <summary>
        /// Creates valid T-SQL DELETE query.
        /// </summary>
        /// <param name="tableName">name of a table</param>
        /// <param name="columnArray">array of columns</param>
        /// <param name="columnDataArray">array of data for each colummn</param>
        /// <param name="typeArray">array holding types for each column</param>
        /// <param name="sqlAndOrOperatorArray">array of operators for (column, column data) pairs. Applicable for columns ranging from 1 to array.Length-2</param>
        /// <returns>SQL DELETE query</returns>
        public static string CreateSQLDeleteQuery(string tableName, string[] columnArray, string[] columnDataArray, Type[] typeArray, string[] sqlAndOrOperatorArray)
        {
            //perform initial validation
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(columnArray, typeArray, "columns", "types");
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(columnArray, columnDataArray, "columns", "array of columns data");

            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(columnDataArray, sqlAndOrOperatorArray, "array of columns data", "(AND, OR) operator Array");
            ThrowExceptionForColumnValueValidationFailure(columnArray, columnDataArray, OperationTypeEnum.DELETE);
            //validation passed

            StringBuilder queryBuilder = new StringBuilder();
            string format = String.Empty;

            //create DELETE query header
            format = CreateStringInternal(OperationTypeEnum.DELETE, tableName, true, columnArray, typeArray);
            CreateSqlQueryInternal(queryBuilder, format);

            ////create DELETE query body
            format = CreateStringInternal(OperationTypeEnum.DELETE, tableName, false, columnDataArray, typeArray, columnArray, sqlAndOrOperatorArray);
            CreateSqlQueryInternal(queryBuilder, format);


            return queryBuilder.ToString();
        }

        /// <summary>
        /// Creates valid T-SQL UPDATE query.
        /// </summary>
        /// <param name="tableName">name of a table</param>
        /// <param name="setClauseColumnArray">array of columns of SET clause</param>
        /// <param name="setClauseColumnDataArray">array of data of columns of SET clause</param>
        /// <param name="whereClauseColumnArray">array of columns of SET clause</param>
        /// <param name="whereClauseColumnDataArray">array of data of columns of SET clause</param>
        /// <param name="setTypeArray">array of types of SET columns</param>
        /// <param name="whereTypeArray">array of types of WHERE columns</param>
        /// <param name="sqlAndOrOperatorArray">array of operators for (column, column data) pairs. Applicable for columns ranging from 1 to array.Length-2</param>
        /// <returns>SQL DELETE query</returns>       
        public static string CreateSQLUpdateQuery(
                                                    string tableName,
                                                    string[] setClauseColumnArray, string[] setClauseColumnDataArray,
                                                    string[] whereClauseColumnArray, string[] whereClauseColumnDataArray,
                                                    Type[] setTypeArray,
                                                    Type[] whereTypeArray,
                                                    string[] sqlAndOrOperatorArray
                                                 )
        {
            //perform initial validation
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(setClauseColumnArray, setClauseColumnDataArray, "SET columns", "SET columns data");
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(setClauseColumnArray, setTypeArray, "SET columns", "SET types");

            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(whereClauseColumnArray, whereClauseColumnDataArray, "WHERE columns", "WHERE columns data");
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(whereClauseColumnArray, whereTypeArray, "WHERE columns", "WHERE types");
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(whereClauseColumnDataArray, sqlAndOrOperatorArray, "WHERE columns data", "(AND, OR) operator Array");


            ThrowExceptionForColumnValueValidationFailure(setClauseColumnArray, setClauseColumnDataArray, OperationTypeEnum.DELETE);
            ThrowExceptionForColumnValueValidationFailure(whereClauseColumnArray, whereClauseColumnDataArray, OperationTypeEnum.DELETE);
            //validation passed

            StringBuilder queryBuilder = new StringBuilder();
            string format = String.Empty;

            //create UPDATE query header
            format = CreateStringInternal(OperationTypeEnum.UPDATE, tableName, true, null, null);
            CreateSqlQueryInternal(queryBuilder, format);

            ////create UPDATE query body
            format = CreateStringInternal(OperationTypeEnum.UPDATE, tableName, false, setClauseColumnDataArray, setTypeArray, setClauseColumnArray, null);
            CreateSqlQueryInternal(queryBuilder, format);

            format = "WHERE";
            CreateSqlQueryInternal(queryBuilder, format);

            format = CreateStringInternal(OperationTypeEnum.UPDATE, tableName, false, whereClauseColumnDataArray, whereTypeArray, whereClauseColumnArray, sqlAndOrOperatorArray);
            CreateSqlQueryInternal(queryBuilder, format);


            return queryBuilder.ToString();
        }
        

        /// <summary>
        /// Creates SqlCommand object.
        /// </summary>
        /// <param name="connection">valid SqlConnection object</param>
        /// <param name="commandText">sql query string</param>
        /// <param name="commandType">type of Sql command</param>
        /// <param name="commandTimeout">time for executing query</param>
        /// <returns>SqlCommand object</returns>
        public static SqlCommand CreateSqlCommand(SqlConnection connection, string commandText, CommandType commandType, int commandTimeout)
        {
            SqlCommand sqlCommand = new SqlCommand(commandText, connection);
            sqlCommand.CommandType = commandType;
            sqlCommand.CommandTimeout = commandTimeout;

            return sqlCommand;
        }

        /// <summary>
        /// Prepares ADO .NET SqlCommand procedure object by passing two arrays, parameters and values respectively.
        /// Function parameters are self-explaining.
        /// </summary>
        /// <param name="sqlCommand">valid SqlCommand object</param>
        /// <param name="parameterArray">array of parameters</param>
        /// <param name="parameterArrayValues">array of parameters values</param>
        /// <returns>SqlCommand object</returns>
        public static SqlCommand PrepareSQLStoredProcedure(SqlCommand sqlCommand, string[] parameterArray, string[] parameterArrayValues)
        {
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(parameterArray, parameterArrayValues, "arrayOfProcedureParameters", "arrayOfProcedureParametersValues");

            for (int i = 0, length = parameterArray.Length; i < length; i++)
            {
                sqlCommand.Parameters.Add(new SqlParameter(parameterArray[i], parameterArrayValues[i]));
            }

            return sqlCommand;
        }

        /// <summary>
        /// Prepares ADO .NET SqlCommand procedure object by passing two arrays, parameters and values respectively.
        /// </summary>
        /// <param name="arrayOfArraysOfItems">Intended for creating server side table based on this array of arrays.(kind of PIVOT function in T-SQL)</param>
        /// <param name="arraySeparator">arrayOfItems separator</param>
        /// <param name="arrayItemSeparator">single array item separator</param>
        /// <returns>string consisting of T-SQL procedure parameter</returns>
        public static string PrepareSQLStoredProcedureParameter(string[][] arrayOfArraysOfItems, string arraySeparator, string arrayItemSeparator)
        {
            string parameterValue = String.Empty;
            StringBuilder parameterValueBuilder = new StringBuilder();

            for (int i = 0, length = arrayOfArraysOfItems.Length; i < length; i++)
            {
                if (i == 0)
                {
                    for (int j = 0, length2 = arrayOfArraysOfItems[i].Length; j < length2; j++)
                    {
                        parameterValueBuilder.AppendFormat("{0}{1}", arrayOfArraysOfItems[i][j], arrayItemSeparator);
                    }
                }
                else
                {
                    for (int j = 0, length2 = arrayOfArraysOfItems[i].Length; j < length2; j++)
                    {
                        if(j == length2 - 1)
                         parameterValueBuilder.AppendFormat("{0}{1}{2}", arrayItemSeparator, arrayOfArraysOfItems[i][j], arrayItemSeparator);
                        else
                         parameterValueBuilder.AppendFormat("{0}{1}", arrayItemSeparator, arrayOfArraysOfItems[i][j]);
                    }
                }
                if(i != length - 1)
                    parameterValueBuilder.Append(arraySeparator);
            }
            parameterValue = parameterValueBuilder.ToString();

            return parameterValue;
        }



        private const string SQL_IS_NULL_CLAUSE = "IS NULL";
        private const string SQL_IS_NOT_NULL_CLAUSE = "IS NOT NULL";

        private static void ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(string[] columnArray, Type[] typesArray, string  message1, string message2)
        {
            string[] typeToStringArray = ConvertToStringRepresentationInternal(typesArray);
            ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(columnArray, typeToStringArray, message1, message2);
        }

        private static void ThrowInvalidOperationExceptionIfNumberOfColumnsAndTypesMismatch(string[] array1, string[] array2, string message1, string message2)
        {
            if (!AssertTwoArraysAreTheSameLengthInternal(array1, array2))
                throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat3, message1, message2);
        }

        private static void ThrowExceptionForColumnValueValidationFailure(string[] columnArray, string[] rowDataArray, OperationTypeEnum operationType)
        {
            bool valueIsReservedOne;
            bool valueIsString;

            for (int i = 0, length = rowDataArray.Length; i < length; i++)
            {
                if (!ValidateColumnValueInternal(rowDataArray[i], operationType, out valueIsReservedOne, out valueIsString))
                    throw ExceptionUtils.CreateException("Value '{0}' for column '{1}' failed to pass validation.", rowDataArray[i], columnArray[i]);
                    //_columnsInfo.Add(
                    //                new DataHolder {
                    //                    ColumnName = columnArray[i],
                    //                    ColumnValue = rowDataArray[i],
                    //                    ColumnValueIsReservedOne = valueIsReservedOne,
                    //                    ColumnValueIsString = valueIsString
                    //                }
                    //               );
            }
        }

        private static string[] ConvertToStringRepresentationInternal(Type[] typesArray)
        {
            string[] array = new string[typesArray.Length];

            for (int i = 0, length = typesArray.Length; i < length; i++)
            {
                array[i] = typesArray[i].Name;
            }

            return array;
        }

        private static bool AssertTwoArraysAreTheSameLengthInternal(string[] arrayOne, string[] arrayTwo)
        {
            return arrayOne.Length == arrayTwo.Length;
        }

        private static bool ValidateColumnValueInternal(string value, OperationTypeEnum sqlOerationType, out bool valueIsReservedOne, out bool valueIsString)
        {
            int parsedIntValue = -1;
            bool passed = false;

            passed = int.TryParse(value, out parsedIntValue);
            if (passed)
            {
                valueIsReservedOne = false;
                valueIsString = false;
                return passed;
            }

            else if (value != null && value != String.Empty)
            {
                //text or reserved clause
                if ((value.Trim() == SQL_IS_NULL_CLAUSE || value.Trim() == SQL_IS_NOT_NULL_CLAUSE) && (sqlOerationType != OperationTypeEnum.INSERT))
                {
                    passed = true;
                    valueIsReservedOne = true;
                    valueIsString = true;
                    return passed;
                }
                else if ((value.Trim() == SQL_IS_NULL_CLAUSE || value.Trim() == SQL_IS_NOT_NULL_CLAUSE) && (sqlOerationType == OperationTypeEnum.INSERT))
                {
                    passed = false;
                    valueIsReservedOne = true;
                    valueIsString = true;
                    throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat2, OperationTypeEnum.INSERT, SQL_IS_NULL_CLAUSE, SQL_IS_NOT_NULL_CLAUSE);
                }
                else
                {
                    //some text passed by user - checking deferred to SQL engine
                    //try to find special harmful phrases - if so throw exception
                    if (Regex.IsMatch(value.ToLower(), @"(drop(\s+|/\*.*\*\\)table)|(delete(\s+|/\*.*\*\\)from)"))
                        throw ExceptionUtils.CreateException(ExceptionUtils.SqlInjectionException_MessageFormat, value);

                    passed = true;
                    valueIsReservedOne = false;
                    valueIsString = true;
                    return passed;
                }
            }
            else
            {
                valueIsReservedOne = false;
                valueIsString = true;
                return passed;
            }
        }

        private static string CreateStringInternal(
                                                   OperationTypeEnum operationType,
                                                   string tableName,
                                                   bool isHeader,
                                                   string[] dataArray,
                                                   Type[] typesArray,
                                                   string[] columnArray = null,
                                                   string[] sqlAndOrOperatorsArray = null,
                                                   OperationTypeInsertType operationTypeInsertType = OperationTypeInsertType.INSERT_VALUES,
                                                   bool lastLoopIterationWasMet = false
                                                  )
        {
            StringBuilder formatBuilder = new StringBuilder();
            string space = " ";
            string comma = ",";
            string equals = "=";
            string openingBrace = "(";
            string closingBrace = ")";

            string selectClause = "SELECT";
            string insertClause = "INSERT";
            string deleteClause = "DELETE";
            string updateClause = "UPDATE";

            string setClause = "SET";
            string whereClause = "WHERE";
            string fromClause = "FROM";

            string values_SelectClause = "VALUES";
            string union_SelectClause = "UNION";

            if (isHeader)
            {
                if (operationType == OperationTypeEnum.INSERT)
                {
                    formatBuilder.Append(operationType.ToString() + space + tableName + openingBrace);
                    for (int i = 0, length = dataArray.Length; i < length; i++)
                    {
                        formatBuilder.Append(dataArray[i]).Append(i == length - 1 ? String.Empty : comma);
                    }
                    formatBuilder.AppendLine(closingBrace);
                }
                else if (operationType == OperationTypeEnum.DELETE)
                {
                    formatBuilder.Append(operationType.ToString() + space + fromClause + space + tableName).AppendLine().Append(whereClause + space); 
                }
                else if (operationType == OperationTypeEnum.UPDATE)
                {
                    formatBuilder.AppendLine(operationType.ToString() + space + tableName).Append(setClause);
                }
                else if (operationType == OperationTypeEnum.LOG)
                {
                    throw ExceptionUtils.CreateException(ExceptionUtils.NotImplementedException_MessageFormat);
                }
            }
            else
            {
                if (operationType == OperationTypeEnum.INSERT)
                {
                    if (operationTypeInsertType == OperationTypeInsertType.INSERT_VALUES)
                    {
                        formatBuilder.Append(insertClause + space + tableName + space + values_SelectClause + openingBrace);
                        for (int i = 0, length = dataArray.Length; i < length; i++)
                        {
                            string quantifier = GetQuantifierForType(typesArray[i]);
                            formatBuilder.Append(quantifier + dataArray[i] + quantifier).Append(i == length - 1 ? String.Empty : comma);
                        }
                        formatBuilder.AppendLine(closingBrace);
                    }
                    
                    else if(operationTypeInsertType == OperationTypeInsertType.SELECT_UNION)
                    {
                      formatBuilder.Append(selectClause + space);
                      for (int i = 0, length = dataArray.Length; i < length; i++)
                      {
                        string quantifier = GetQuantifierForType(typesArray[i]);
                        formatBuilder.Append(quantifier + dataArray[i] + quantifier).Append(i == length - 1 ? String.Empty : comma);
                      }
                        if (!lastLoopIterationWasMet)
                        {
                            formatBuilder.AppendLine().AppendLine();
                            formatBuilder.Append(union_SelectClause);
                            formatBuilder.AppendLine();
                        }
                    }
                }
                else if (operationType == OperationTypeEnum.DELETE)
                {
                    for (int i = 0, length = dataArray.Length; i < length; i++)
                    {
                        string quantifier = GetQuantifierForType(typesArray[i]);
                        if (dataArray[i] == SQL_IS_NULL_CLAUSE || dataArray[i] == SQL_IS_NOT_NULL_CLAUSE)
                            formatBuilder.Append(space + columnArray[i] + space + dataArray[i]).Append(i == length - 1 ? String.Empty : space + sqlAndOrOperatorsArray[i] + space).AppendLine();
                        else
                            formatBuilder.Append(space + columnArray[i] + space + equals + space + quantifier + dataArray[i] + quantifier).Append(i == length - 1 ? String.Empty : space + sqlAndOrOperatorsArray[i] + space).AppendLine();
                    }
                }
                else if (operationType == OperationTypeEnum.UPDATE)
                {
                    if(sqlAndOrOperatorsArray != null)
                    {
                        for (int i = 0, length = dataArray.Length; i < length; i++)
                        {
                            string quantifier = GetQuantifierForType(typesArray[i]);
                            if (dataArray[i] == SQL_IS_NULL_CLAUSE || dataArray[i] == SQL_IS_NOT_NULL_CLAUSE)
                                formatBuilder.Append(space + columnArray[i] + space + dataArray[i]).Append(i == length - 1 ? String.Empty : space + sqlAndOrOperatorsArray[i] + space).AppendLine();
                            else
                                formatBuilder.Append(space + columnArray[i] + space + equals + space + quantifier + dataArray[i] + quantifier).Append(i == length - 1 ? String.Empty : space + sqlAndOrOperatorsArray[i] + space).AppendLine();
                        }
                    }
                    else
                    {
                        for (int i = 0, length = dataArray.Length; i < length; i++)
                        {
                            string quantifier = GetQuantifierForType(typesArray[i]);
                            formatBuilder.Append(space + columnArray[i] + space + equals + space + quantifier + dataArray[i] + quantifier);
                            if (i < length - 1)
                                formatBuilder.AppendLine(comma);
                        }
                    }
                }
                else if (operationType == OperationTypeEnum.LOG)
                {
                    throw ExceptionUtils.CreateException(ExceptionUtils.NotImplementedException_MessageFormat);
                }
            }

            return formatBuilder.ToString();
        }

        private static void CreateSqlQueryInternal(StringBuilder sqlQueryBuilder, string format, params string[] formatParams)
        {
            sqlQueryBuilder.AppendLine(String.Format(format, formatParams));
        }

        private static string GetQuantifierForType(Type type)
        {
            string apostrophe = "'";
            string emptyString = String.Empty; 
            NotSupportedException nse = new NotSupportedException("This type for column value is not supported yet");

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    throw nse;

                case TypeCode.Object:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return apostrophe;

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                default:
                    return emptyString;
            }
        }

        //private static IList<DataHolder> _columnsInfo = null;
    }
}
