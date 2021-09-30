using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    /// <summary>
    /// Contains utility methods that are used multiple times within the program.
    /// </summary>
    static class Utility
    {
        /// <summary>
        /// Used for checking the login details.
        /// Convert a string to a byte array.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The byte array.</returns>
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Used for checking the login details.
        /// Converts a byte array to a string.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The string.</returns>
        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// Used for messaging with the clients.
        /// Converts a string to a byte[].
        /// </summary>
        /// <param name="str">the string to convert.</param>
        /// <returns>The byte[]</returns>
        public static byte[] ConvertToBytes(string str)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] array = encoder.GetBytes(str);
            return array;
        }

        /// <summary>
        /// Used for messaging with the clients.
        /// Converts a byte[] to a string.
        /// </summary>
        /// <param name="array">The byte array to convert.</param>
        /// <returns>The string.</returns>
        public static string ConvertToString(byte[] array)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            string str = encoder.GetString(array, 0, array.Length);
            return str;
        }
    }
}
