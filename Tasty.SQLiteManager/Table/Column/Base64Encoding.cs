using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    public static class Base64Encoding
    {
        public static string EncodeBase64(Encoding encoding, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

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
