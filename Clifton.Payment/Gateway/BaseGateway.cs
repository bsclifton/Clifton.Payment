using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Payment.Gateway {
    public abstract class BaseGateway {
        public abstract void ProcessCreditCard(string cardNumber, string expirationMonth, string expirationYear, string dollarAmount, string cardHoldersName);

        protected void ValidateCreditCard(string cardNumber, string expirationMonth, string expirationYear, out int parsedExpirationMonth, out int parsedExpirationYear) {
            if (string.IsNullOrWhiteSpace(cardNumber)) {
                throw new Exception("cardNumber is null / empty");
            }

            if (string.IsNullOrWhiteSpace(expirationMonth)) {
                throw new Exception("expirationMonth is null / empty"); 
            }

            if (string.IsNullOrWhiteSpace(expirationYear)) {
                throw new Exception("expirationYear is null / empty");
            }
            
            CreditCardType cardType = CreditCard.GetCardType(cardNumber);
            if (cardType == CreditCardType.Invalid) {
                throw new Exception("Card type is unsupported or cardNumber is invalid");
	        }

	        if(!CreditCard.HasValidLuhnChecksum(cardNumber)){
                throw new Exception("cardNumber is invalid");
	        }

            if (!int.TryParse(expirationMonth, out parsedExpirationMonth)) {
                throw new Exception("expirationMonth must be a number");
            }

            if (parsedExpirationMonth < 1 || parsedExpirationMonth > 12) {
                throw new Exception("expirationMonth must be between 1 and 12");
            }

            if (!int.TryParse(expirationYear, out parsedExpirationYear)) {
                throw new Exception("expirationYear must be a number");
            }
        }
    }
}