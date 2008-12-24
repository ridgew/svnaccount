using System;
using System.Collections.Generic;
using System.Text;

namespace System.Web.Security
{

    /// <summary>
    /// Represents errors that occur when a text can't be decoded or the text is tampered 
    /// </summary>
    public class InvalidCypherTextException : Exception {
        
        /// <summary>
        /// Constructor
        /// </summary>
        public InvalidCypherTextException() : base() {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InvalidCypherTextException(string message) : base(message) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InvalidCypherTextException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
