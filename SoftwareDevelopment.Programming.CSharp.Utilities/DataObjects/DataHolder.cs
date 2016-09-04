using System;

namespace SoftwareDevelopment.Programming.CSharp.Utilities.DataObjects
{
    /// <summary>
    /// Class to store data.
    /// </summary>
    public class DataHolder
    {
        /// <summary>
        /// Name of column.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Value of column.
        /// </summary>
        public string ColumnValue { get; set; }

        /// <summary>
        /// Specifies whether column value is a reserved one word.
        /// </summary>
        public bool ColumnValueIsReservedOne { get; set; }

        /// <summary>
        /// Specifies whether column value is a string value.
        /// </summary>
        public bool ColumnValueIsString { get; set; }
    }
}
