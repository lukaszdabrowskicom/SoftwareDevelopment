using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common Excel operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class ExcelUtils
    {

        /// <summary>
        /// Initializes Microsoft Excel connection string for specific Excel version.
        /// </summary>
        /// <param name="excelVersion">version of Microsoft Excel</param>
        /// <param name="fileName">path to Excel file</param>
        /// <param name="excelConnectionString">this variable will be initialized with correct Excel version</param>
        public static void GetConnectionStringForExcelVersion(ExcelVersion excelVersion, string fileName, out string excelConnectionString)
        {
            switch (excelVersion)
            {
                case ExcelVersion.EXCEL_STANDARD_CONNECTION_STRING:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_STANDARD_CONNECTION_STRING, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_STANDARD_ALTERNATIVE_CONNECTION_STRING:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_STANDARD_ALTERNATIVE_CONNECTION_STRING, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_HEADER_ROW:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_HEADER_ROW, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_DATA_ONLY:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_DATA_ONLY, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_HEADER_ROW:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_HEADER_ROW, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_DATA_ONLY:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_DATA_ONLY, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_HEADER_ROW:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_HEADER_ROW, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_DATA_ONLY:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_DATA_ONLY, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_HEADER_ROW:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_HEADER_ROW, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_DATA_ONLY:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_DATA_ONLY, "$1", fileName);
                    break;

                case ExcelVersion.EXCEL_2007_AND_LATER_XLSX_TEXT_CONNECTION_STRING:
                    excelConnectionString = MiscUtils.StringReplace(EXCEL_2007_AND_LATER_XLSX_TEXT_CONNECTION_STRING, "$1", fileName);
                    break;

                default:
                    excelConnectionString = String.Empty;
                    break;
            }
        }

        /// <summary>
        /// Retrieves DataSet object consisting of all worksheets converted to DataTable objects.
        /// Tables in the returned DataSet object are named after exactWorkbookNameArray values respectively.
        /// </summary>
        /// <param name="pathToExcelFile">relative or absolute path to Excel file</param>
        /// <param name="excelVersion">version of Microsoft Excel</param>
        /// <param name="exactWorkbookNameArray">exact workbook name array</param>
        /// <param name="isRelativePath">specifies wheter excel file resides in the application executing directory</param>
        /// <returns>DataSet consisting of all worksheets converted to DataTable objects</returns>
        public static DataSet ReadExcelFromFile(string pathToExcelFile, ExcelVersion excelVersion, string[] exactWorkbookNameArray, bool isRelativePath = false)
        {
            string fileName = String.Empty;

            if (isRelativePath)
                fileName = FileAndDirectoryUtils.ComposeFullPath(Directory.GetCurrentDirectory(), pathToExcelFile);
            else
                fileName = pathToExcelFile;

            string connectionString;
            GetConnectionStringForExcelVersion(excelVersion, pathToExcelFile, out connectionString);

            DataSet dataSet = new DataSet();
            OleDbDataAdapter oleDbDataAdapter = null;

            string query = String.Empty;
            foreach (string worksheetName in exactWorkbookNameArray)
            {
                query = "SELECT * FROM [" + worksheetName + "$]";
                oleDbDataAdapter = new OleDbDataAdapter(query, connectionString);

                oleDbDataAdapter.Fill(dataSet, worksheetName);
            }


            return dataSet;
        }

        /// <summary>
        /// Retrieves array of columns names.
        /// </summary>
        /// <param name="dataTable">DataTable to retrieve data from</param>
        /// <returns>array of strings</returns>
        public static string[] RetrieveDataTableColumnsNamesArray(DataTable dataTable)
        {
          DataColumn[] arrayOfDataColumns = MiscUtils.RetrieveArrayOfT<DataColumn>(dataTable.Columns);

            string[] names = new string[arrayOfDataColumns.Length];

            for (int i = 0, length = names.Length ; i < length; i++)
            {
                names[i] = arrayOfDataColumns[i].ColumnName;
            }

            return names;
        }

        /// <summary>
        /// Retrieves array of columns types.
        /// </summary>
        /// <param name="dataTable">DataTable to retrieve data from</param>
        /// <returns>array of Type</returns>
        public static Type[] RetrieveDataTableColumnsTypesArray(DataTable dataTable)
        {
            DataColumn[] arrayOfDataColumns = MiscUtils.RetrieveArrayOfT<DataColumn>(dataTable.Columns);

            Type[] types = new Type[arrayOfDataColumns.Length];

            for (int i = 0, length = types.Length; i < length; i++)
            {
                types[i] = arrayOfDataColumns[i].DataType;
            }

            return types;
        }

        /// <summary>
        /// Returns true or false depending on whether DataTableCollection contains any rows.
        /// </summary>
        /// <param name="dataTableCollection">collection of DataTable objects</param>
        /// <returns>bool</returns>
        public static bool CheckIfAnyExcelWorksheetContainsData(DataTableCollection dataTableCollection)
        {
            bool dataTableHasData = false;

            foreach (DataTable item in dataTableCollection)
            {
                if (item.Rows.Count > 0)
                {
                    dataTableHasData = true;
                    break;
                }
            }

            return dataTableHasData;
        }

        private static string EXCEL_STANDARD_CONNECTION_STRING = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$1;Extended Properties =\"Excel 8.0;HDR=YES;IMEX=1\";";

        private static string EXCEL_STANDARD_ALTERNATIVE_CONNECTION_STRING = "OLEDB;Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$1;Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\";";

        private static string EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_HEADER_ROW = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 12.0 Xml;HDR=YES\";";

        private static string EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_DATA_ONLY = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 12.0 Xml;HDR=NO\";";

        private static string EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_HEADER_ROW = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 12.0;HDR=YES\";";

        private static string EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_DATA_ONLY = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 12.0;HDR=NO\";";

        private static string EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_HEADER_ROW = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 12.0 Macro;HDR=YES\";";

        private static string EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_DATA_ONLY = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 12.0 Macro;HDR=NO\";";

        private static string EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_HEADER_ROW = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 8.0;HDR=YES\";";

        private static string EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_DATA_ONLY = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 8.0;HDR=NO\";";

        private static string EXCEL_2007_AND_LATER_XLSX_TEXT_CONNECTION_STRING = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$1;Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";";
    }
}
