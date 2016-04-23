using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common xml operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XmlUtils<T> where T : class, new()
    {
        private static XmlSerializer _serializer = null;
        /// <summary>
        /// Serializes object instance to string representation.
        /// </summary>
        /// <param name="outputStream">output stream to serialize object to</param>
        /// <param name="node">object instance to serialize</param>
        /// returns>object serialized as string</returns>
        public static void Serialize(Stream outputStream, T node)
        {
            _serializer = new XmlSerializer(typeof(T));
            _serializer.Serialize(outputStream, node);
        }

        /// <summary>
        /// Deserializes object back to object instance.
        /// </summary>
        /// <param name="outputStream">input stream to deserialize object from</param>
        /// returns>object instance</returns>
        public static object Deserialize(Stream inputStream)
        {
            _serializer = new XmlSerializer(typeof(T));
            object deserializedInput = _serializer.Deserialize(inputStream);

            return deserializedInput;
        }

        /// <summary>
        /// Serializes object instance to string representation.
        /// </summary>
        /// <param name="outputStream">output stream to serialize object to</param>
        /// <param name="nodeList">object instance list to serialize</param>
        /// returns>object instance list serialized as string</returns>
        public static void Serialize(Stream outputStream, List<T> nodeList)
        {
            _serializer = new XmlSerializer(typeof(List<T>));
            _serializer.Serialize(outputStream, nodeList);
        }


        /// <summary>
        /// Deserializes stream to object instance list.
        /// </summary>
        /// <param name="outputStream">input stream to deserialize object from</param>
        /// returns>object instance</returns>
        public static object Deserialize(Stream inputStream, bool isCollection)
        {
            if (!isCollection)
                throw ExceptionUtils.CreateException(ExceptionUtils.InvalidOperationExceptionMessageFormat, "isCollection");

            _serializer = new XmlSerializer(typeof(List<T>));
            object deserializedInput = _serializer.Deserialize(inputStream);

            return deserializedInput;
        }
    }
}
