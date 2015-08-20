using System;
using System.Globalization;
using System.Security.Cryptography;

namespace Clifton.Payment.Gateway {
    public static class MimeTypes {
        ///<summary>JavaScript Object Notation JSON; Defined in RFC 4627</summary>
        public const string ApplicationJson = "application/json";
    }

    public abstract class BaseGateway {
        protected string GetEpochTimestampInMilliseconds() {
            long millisecondsSinceEpoch = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return millisecondsSinceEpoch.ToString();
        }

        /// <summary>
        /// Generates a cryptographically strong random number.
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/Cryptographic_nonce"/>
        protected int GetNonce() {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(bytes);
                }
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        protected string FormatCardExpirationDate(DateTime expirationDate) {
            return string.Format("{0}{1}", expirationDate.ToString("MM"), expirationDate.ToString("yy"));
        }

        protected string ValidateDollarAmount(string dollarAmount) {
            if (string.IsNullOrWhiteSpace(dollarAmount)) {
                throw new DollarAmountNullException("Dollar amount is null / empty");
            }

            dollarAmount = dollarAmount.Trim();

            //TODO: validate amount (decimal place, etc)

            return dollarAmount;
        }

        protected string GetDollarAmountAsCents(string dollarAmount) {
            //TODO: ...

            return dollarAmount;
        }

        protected string ValidateCardSecurityCode(CreditCardType cardType, string cardVerificationValue) {
            if (string.IsNullOrWhiteSpace(cardVerificationValue)) {
                throw new CardSecurityCodeNullException("Card security code is null / empty");
            }

            cardVerificationValue = cardVerificationValue.Trim();

            switch (cardType) {
                case CreditCardType.AmericanExpress:
                    // American Express cards have a four-digit code printed on the front side of the card above the number.
                    if (cardVerificationValue.Length != 4) {
                        throw new CardSecurityCodeFormatException("Card security code must be 4 digits");
                    }
                    break;

                case CreditCardType.Diners:
                case CreditCardType.Discover:
                case CreditCardType.MasterCard:
                case CreditCardType.Visa:
                    // Diners Club, Discover, JCB, MasterCard, and Visa credit and debit cards have a three-digit card security code.
                    // The code is the final group of numbers printed on the back signature panel of the card.
                    if (cardVerificationValue.Length != 3) {
                        throw new CardSecurityCodeFormatException("Card security code must be 3 digits");
                    }
                    break;

                default:
                    throw new CardTypeNotSupportedException("Card type does not support card security code");
            }

            int parsedCardSecurityCode;
            if (!int.TryParse(cardVerificationValue, out parsedCardSecurityCode)) {
                throw new CardSecurityCodeFormatException("Card security code must be numeric");
            }

            return cardVerificationValue;
        }

        protected CreditCardType ValidateCreditCard(string cardNumber, string expirationMonth, string expirationYear, out DateTime parsedExpirationDate) {
            if (string.IsNullOrWhiteSpace(cardNumber)) {
                throw new CardNumberNullException("Card number is null / empty");
            }

            if (string.IsNullOrWhiteSpace(expirationMonth)) {
                throw new ExpirationNullException("Expiration month is null / empty");
            }

            if (string.IsNullOrWhiteSpace(expirationYear)) {
                throw new ExpirationNullException("Expiration year is null / empty");
            }

            CreditCardType cardType = CreditCard.GetCardType(cardNumber);
            if (cardType == CreditCardType.Invalid) {
                throw new CardTypeNotSupportedException("Card type is unsupported or card number is invalid");
            }

            if (!CreditCard.HasValidLuhnChecksum(cardNumber)) {
                throw new CardNumberInvalidException("Card number is invalid");
            }

            int parsedExpirationMonth;
            if (!int.TryParse(expirationMonth, out parsedExpirationMonth)) {
                throw new ExpirationFormatException("Expiration month must be a number");
            }

            if (parsedExpirationMonth < 1 || parsedExpirationMonth > 12) {
                throw new ExpirationOutOfRangeException("Expiration month must be between 1 and 12");
            }

            int parsedExpirationYear;
            if (!int.TryParse(expirationYear, out parsedExpirationYear)) {
                throw new ExpirationFormatException("Expiration year must be a number");
            }

            int fourDigitYear;
            try {
                fourDigitYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(parsedExpirationYear);
            } catch (Exception ex) {
                throw new ExpirationFormatException("Error converting expiration year to a four digit year", ex);
            }

            int daysInMonth = DateTime.DaysInMonth(fourDigitYear, parsedExpirationMonth);
            parsedExpirationDate = new DateTime(fourDigitYear, parsedExpirationMonth, daysInMonth, 23, 59, 59);

            if (DateTime.Now > parsedExpirationDate) {
                throw new CardExpiredException("Card has expired");
            }

            return cardType;
        }
    }
}