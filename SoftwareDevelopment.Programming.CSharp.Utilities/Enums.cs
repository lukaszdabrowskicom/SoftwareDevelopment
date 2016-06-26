using System;
using System.Reflection;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Type of operation.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public enum OperationTypeEnum
    {
        /// <summary>
        /// Represents SQL INSERT operation
        /// </summary>
        INSERT,

        /// <summary>
        /// Represents SQL UPDATE operation
        /// </summary>
        UPDATE,

        /// <summary>
        /// Represents SQL DELETE operation
        /// </summary>
        DELETE,

        /// <summary>
        /// Represents logging operation
        /// </summary>
        LOG
    }


    /// <summary>
    /// Type of INSERT operation to create.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public enum OperationTypeInsertType
    {
        /// <summary>
        /// Represents SQL INSERT type of query
        /// </summary>
        SELECT_UNION,

        /// <summary>
        /// Represents SQL INSERT type of query
        /// </summary>
        INSERT_VALUES
    }

    /// <summary>
    /// Microsoft Excel connection string versions.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public enum ExcelVersion
    {
        /// <summary>
        /// Standard (Excel 97-2003) [Microsoft Jet OLE DB 4.0].
        /// </summary>
        EXCEL_STANDARD_CONNECTION_STRING,


        /// <summary>
        /// Standard alternative (Excel 97-2003) [Microsoft Jet OLE DB 4.0].
        /// </summary>
        EXCEL_STANDARD_ALTERNATIVE_CONNECTION_STRING,


        /// <summary>
        /// Connect to Excel 2007 and later files with the xlsx file extension [Microsoft ACE OLEDB 12.0].
        /// This option assumes that first row of the spreadsheet is a header row consisting of columns names.
        /// </summary>
        EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_HEADER_ROW,


        /// <summary>
        /// Connect to Excel 2007 and later files with the xlsx file extension [Microsoft ACE OLEDB 12.0].
        /// This option assumes that there is no header row consisting of column names. The whole spreadsheet contains data only.
        /// </summary>
        EXCEL_2007_AND_LATER_XLSX_CONNECTION_STRING_WITH_DATA_ONLY,


        /// <summary>
        /// Connect to Excel 2007 and later files with the xlsb file extension [Microsoft ACE OLEDB 12.0].
        /// That is the Office Open XML format saved in a binary format.
        /// The structure is similar but it is not saved in a text readable format as the xlsx files and can improve performance if the file contains lots of data.
        /// This option assumes that first row of the spreadsheet is a header row consisting of columns names.
        /// </summary>
        EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_HEADER_ROW,


        /// <summary>
        /// Connect to Excel 2007 and later files with the xlsb file extension [Microsoft ACE OLEDB 12.0].
        /// That is the Office Open XML format saved in a binary format.
        /// The structure is similar but it is not saved in a text readable format as the xlsx files and can improve performance if the file contains lots of data.
        /// This option assumes that there is no header row consisting of column names. The whole spreadsheet contains data only.
        /// </summary>
        EXCEL_2007_AND_LATER_XLSB_CONNECTION_STRING_WITH_DATA_ONLY,


        /// <summary>
        /// Connect to Excel 2007 and later files with the xlsm file extension [Microsoft ACE OLEDB 12.0].
        /// This option assumes that first row of the spreadsheet is a header row consisting of columns names.
        /// </summary>
        EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_HEADER_ROW,


        /// <summary>
        /// Connect to Excel 2007 and later files with the xlsm file extension [Microsoft ACE OLEDB 12.0].
        /// This option assumes that there is no header row consisting of column names. The whole spreadsheet contains data only.
        /// </summary>
        EXCEL_2007_AND_LATER_XLSM_CONNECTION_STRING_WITH_DATA_ONLY,


        /// <summary>
        /// You can use this connection string to use the Microsoft Office 2007 OLEDB driver to connect to older 97-2003 Excel workbooks [Microsoft ACE OLEDB 12.0].
        /// This option assumes that first row of the spreadsheet is a header row consisting of columns names.
        /// </summary>
        EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_HEADER_ROW,


        /// <summary>
        /// You can use this connection string to use the Microsoft Office 2007 OLEDB driver to connect to older 97-2003 Excel workbooks [Microsoft ACE OLEDB 12.0].
        /// This option assumes that there is no header row consisting of column names. The whole spreadsheet contains data only.
        /// </summary>
        EXCEL_97_2003_XLS_CONNECTION_STRING_WITH_DATA_ONLY,


        /// <summary>
        /// Connect to Excel 2007 and later files with the xlsx file extension.
        /// Use this option when you want to treat all data in the file as text, overriding Excels column type [Microsoft ACE OLEDB 12.0].
        /// </summary>
        EXCEL_2007_AND_LATER_XLSX_TEXT_CONNECTION_STRING

    }

    /// <summary>
    /// Type of log operation.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public enum LogOperationTypeEnum
    {
        /// <summary>
        /// Represents logging info message
        /// </summary>
        INFO,

        /// <summary>
        /// Represents logging warning message
        /// </summary>
        WARNING,

        /// <summary>
        /// Represents logging error message
        /// </summary>
        ERROR
    }

    /// <summary>
    /// Type of Windows log operation.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public enum WindowsEventLogType
    {
        /// <summary>
        /// Represents Application log type
        /// </summary>
        Application,

        /// <summary>
        /// Represents Security log type
        /// </summary>
        Security,

        /// <summary>
        /// Represents Setup log type
        /// </summary>
        Setup,

        /// <summary>
        /// Represents System log type
        /// </summary>
        System
    }

}
