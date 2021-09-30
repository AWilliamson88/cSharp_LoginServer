﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    /// <summary>
    /// Contains methods to generate password hashes and test them.
    /// </summary>
    class PasswordTester
    {
        /// <summary>
        /// Tests the admin and client passwords to see if they match.
        /// </summary>
        /// <param name="adminPassword">The admin password.</param>
        /// <param name="testPassword">The password to test.</param>
        /// <returns>True only if they match, otherwise false.</returns>
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

        /// <summary>
        /// Hashes the password with the salt.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>The salted and hashed password.</returns>
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
