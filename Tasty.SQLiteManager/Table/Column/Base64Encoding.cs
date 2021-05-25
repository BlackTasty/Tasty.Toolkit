using System;
using System.Text;

namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// Utility class to parse strings from and to Base64
    /// </summary>
    public static class Base64Encoding
    {
        /// <summary>
        /// Encodes a string into a Base64 string
        /// </summary>
        /// <param name="encoding">The type of <see cref="Encoding"/></param>
        /// <param name="text">Input text</param>
        /// <returns>The encoded Base64 string</returns>
        public static string EncodeBase64(Encoding encoding, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        /// <summary>
        /// Decodes a Base64 string into a string
        /// </summary>
        /// <param name="encoding">The type of <see cref="Encoding"/></param>
        /// <param name="encodedText">Input Base64 string</param>
        /// <returns>The encoded Base64 string</returns>
        public static string DecodeBase64(Encoding encoding, string encodedText)
        {
            if (string.IsNullOrEmpty(encodedText))
            {
                return "";
            }

            byte[] textAsBytes = Convert.FromBase64String(encodedText);
            return encoding.GetString(textAsBytes);
        }
    }
}
