using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;

namespace WCFServiceWebRole.Extension
{
    public static class StringExtensions
    {
        /// <summary>
        /// Check if the string is a valid json
        /// </summary>
        /// <param name="jsonData">string to check</param>
        /// <returns></returns>
        public static bool IsJson(this string json)
        {
            return json.Trim().Substring(0, 1).IndexOfAny(new[] { '[', '{' }) == 0;
        }

        /// <summary>
        /// Check if the string is a valid email
        /// </summary>
        /// <param name="emailaddress">string to check</param>
        public static bool IsValidEmail(this string emailaddress)
        {
            if (new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$").IsMatch(emailaddress))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        /// <summary>
        /// Return a string with the fist letter as upper case
        /// </summary>
        /// <param name="input">string needs the first letter as upper case</param>
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            if (input.Length > 1)
            {
                return char.ToUpper(input[0]) + input.Substring(1);
            }
            return input.ToUpper();
        }
    }
}