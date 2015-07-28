using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Payment.Gateway {
    public abstract class BaseGateway {
        public abstract void CreditCardPurchase(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName, string referenceNumber);

        protected abstract string ContentType { get; }

        protected CreditCardType ValidateCreditCard(string cardNumber, string expirationMonth, string expirationYear, out int parsedExpirationMonth, out int parsedExpirationYear) {
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

            if (!int.TryParse(expirationMonth, out parsedExpirationMonth)) {
                throw new ExpirationFormatException("Expiration month must be a number");
            }

            if (parsedExpirationMonth < 1 || parsedExpirationMonth > 12) {
                throw new ExpirationOutOfRangeException("Expiration month must be between 1 and 12");
            }

            if (!int.TryParse(expirationYear, out parsedExpirationYear)) {
                throw new ExpirationFormatException("Expiration year must be a number");
            }

            //TODO: further check the year here; and then convert to 2 digit year.

            return cardType;
        }
    }
}