using System;
using System.Web.Security;
using System.Text;

namespace System.Web.Security
{
    
    /// <summary>
    /// A class to encode, decode and validate strings based on the MachineKey
    /// </summary>
    public static class MachineKeyCryptography {

        /// <summary>
        /// Encodes a string and protects it from tampering
        /// </summary>
        /// <param name="text">String to encode</param>
        /// <returns>Encoded string</returns>
        public static string Encode(string text) {
            return Encode(text, CookieProtection.All);
        }

        /// <summary>
        /// Encodes a string
        /// </summary>
        /// <param name="text">String to encode</param>
        /// <param name="cookieProtection">The method in which the string is protected</param>
        /// <returns></returns>
        public static string Encode(string text, CookieProtection cookieProtection) {
            if (string.IsNullOrEmpty(text) || cookieProtection == CookieProtection.None) {
                return text;
            }
            byte[] buf = Encoding.UTF8.GetBytes(text);
            return CookieProtectionHelperWrapper.Encode(cookieProtection, buf, buf.Length); 
        }

        /// <summary>
        /// Decodes a string and returns null if the string is tampered
        /// </summary>
        /// <param name="text">String to decode</param>
        /// <returns>The decoded string or throws InvalidCypherTextException if tampered with</returns>
        public static string Decode(string text) {
            return Decode(text, CookieProtection.All);
        }

        /// <summary>
        /// Decodes a string
        /// </summary>
        /// <param name="text">String to decode</param>
        /// <param name="cookieProtection">The method in which the string is protected</param>
        /// <returns>The decoded string or throws InvalidCypherTextException if tampered with</returns>
        public static string Decode(string text, CookieProtection cookieProtection) {
            if (string.IsNullOrEmpty(text)) {
                return text;
            }
            byte[] buf;
            try {
                buf = CookieProtectionHelperWrapper.Decode(cookieProtection, text);
            }
            catch(Exception ex) {
                throw new InvalidCypherTextException("Unable to decode the text", ex.InnerException);
            }
            if (buf == null || buf.Length == 0) {
                throw new InvalidCypherTextException("Unable to decode the text");
            }
            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }
    }
}
