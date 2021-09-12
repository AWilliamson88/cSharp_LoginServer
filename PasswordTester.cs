using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    class PasswordTester
    {

        public bool TestPasswords(string adminPassword, string testPassword)
        {
            string salt = SaltGenerator.GetSaltString();

            string admin = GeneratePasswordHash(adminPassword, salt);
            string user = GeneratePasswordHash(testPassword, salt);

            if (user == admin)
            {
                return true;
            }

            return false;
        }

        private string GeneratePasswordHash(string password, string salt)
        {
            string str = password + salt;

            // Use SHA512 to generate the hash from this salted password
            SHA512 sha = new SHA512CryptoServiceProvider();

            byte[] dataBytes = Utility.GetBytes(str);
            byte[] resultBytes = sha.ComputeHash(dataBytes);

            return Utility.GetString(resultBytes);
        }


    }
}
