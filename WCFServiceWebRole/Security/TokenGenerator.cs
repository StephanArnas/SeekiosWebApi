using System;
using System.Security.Cryptography;
using System.Text;

namespace WCFServiceWebRole.Security
{
    public static class TokenGenerator
    {
        private static string _lastGeneratedToken = string.Empty;

        /// <summary>
        /// Generate a random string
        /// </summary>
        /// <param name="email">the email is part of the genarated string</param>
        public static string Generate(string email)
        {
            var emailHash = HashString(email + new Random().Next(1, 20000).ToString());
            _lastGeneratedToken = HashString(emailHash + _lastGeneratedToken);
            return _lastGeneratedToken;
        }

        /// <summary>
        /// Hash a string
        /// </summary>
        /// <param name="toHash">string to hash</param>
        private static string HashString(string toHash)
        {
            SHA256Managed hashstring = new SHA256Managed();
            byte[] bytes = Encoding.Unicode.GetBytes(toHash);
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }
    }
}