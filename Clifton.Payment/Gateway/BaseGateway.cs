using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Clifton.Payment.Gateway {
    public abstract class BaseGateway {
        public abstract void CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber);
        public abstract void CreditCardRefund(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string cardVerificationValue, string referenceNumber);

        protected abstract string ContentType { get; }

        protected string FormatCardExpirationDate(DateTime expirationDate) {
            return string.Format("{0}{1}", expirationDate.ToString("MM"), expirationDate.ToString("yy"));
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