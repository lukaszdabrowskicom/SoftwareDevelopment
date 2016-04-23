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
}
