using System;

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

    [Serializable]
    public class CardExpiredException : Exception {
        public CardExpiredException() { }
        public CardExpiredException(string message) : base(message) { }
        public CardExpiredException(string message, Exception inner) : base(message, inner) { }
        protected CardExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class CardSecurityCodeNullException : ArgumentNullException {
        public CardSecurityCodeNullException() { }
        public CardSecurityCodeNullException(string message) : base(message) { }
        public CardSecurityCodeNullException(string message, Exception inner) : base(message, inner) { }
        protected CardSecurityCodeNullException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class CardSecurityCodeFormatException : FormatException {
        public CardSecurityCodeFormatException() { }
        public CardSecurityCodeFormatException(string message) : base(message) { }
        public CardSecurityCodeFormatException(string message, Exception inner) : base(message, inner) { }
        protected CardSecurityCodeFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class DollarAmountNullException : Exception {
        public DollarAmountNullException() { }
        public DollarAmountNullException(string message) : base(message) { }
        public DollarAmountNullException(string message, Exception inner) : base(message, inner) { }
        protected DollarAmountNullException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class DollarAmountInvalidException : FormatException {
        public DollarAmountInvalidException() { }
        public DollarAmountInvalidException(string message) : base(message) { }
        public DollarAmountInvalidException(string message, Exception inner) : base(message, inner) { }
        protected DollarAmountInvalidException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
