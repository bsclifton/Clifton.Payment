using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Payment {
    [Serializable]
    public class CardNumberNullException : ArgumentNullException {
        public CardNumberNullException() { }
        public CardNumberNullException(string message) : base(message) { }
        public CardNumberNullException(string message, Exception inner) : base(message, inner) { }
        protected CardNumberNullException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class CardNumberInvalidException : FormatException {
        public CardNumberInvalidException() { }
        public CardNumberInvalidException(string message) : base(message) { }
        public CardNumberInvalidException(string message, Exception inner) : base(message, inner) { }
        protected CardNumberInvalidException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class CardTypeNotSupportedException : NotSupportedException {
        public CardTypeNotSupportedException() { }
        public CardTypeNotSupportedException(string message) : base(message) { }
        public CardTypeNotSupportedException(string message, Exception inner) : base(message, inner) { }
        protected CardTypeNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
    
    [Serializable]
    public class ExpirationNullException : ArgumentNullException {
        public ExpirationNullException() { }
        public ExpirationNullException(string message) : base(message) { }
        public ExpirationNullException(string message, Exception inner) : base(message, inner) { }
        protected ExpirationNullException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ExpirationOutOfRangeException : ArgumentOutOfRangeException {
        public ExpirationOutOfRangeException() { }
        public ExpirationOutOfRangeException(string message) : base(message) { }
        public ExpirationOutOfRangeException(string message, Exception inner) : base(message, inner) { }
        protected ExpirationOutOfRangeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ExpirationFormatException : FormatException {
        public ExpirationFormatException() { }
        public ExpirationFormatException(string message) : base(message) { }
        public ExpirationFormatException(string message, Exception inner) : base(message, inner) { }
        protected ExpirationFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
