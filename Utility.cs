using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    static class Utility
    {
        // utilty function to convert string to byte[]
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        // utilty function to convert byte[] to string
        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static byte[] ConvertToBytes(string str)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] array = encoder.GetBytes(str);
            return array;
        }

        public static string ConvertToString(byte[] array)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            string str = encoder.GetString(array, 0, array.Length);
            return str;
        }
    }
}
