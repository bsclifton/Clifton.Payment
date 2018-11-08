using System.Text.RegularExpressions;

namespace Clifton.Payment {
    public static class CreditCard {
        private static string SanitizeCardNumber(string cardNumber) {
            //TODO: update to just grab all numbers
            return Regex.Replace(cardNumber.Trim(), @"\s+", string.Empty);
        }

        public static CreditCardType GetCardType(string cardNumber) {
            if (string.IsNullOrWhiteSpace(cardNumber)) {
                return CreditCardType.Invalid;
            }

            cardNumber = SanitizeCardNumber(cardNumber);

            int length = cardNumber.Length;

            switch (cardNumber[0]) {
                case '3':
                    if ((length != 14) && (length != 15)) {
                        return CreditCardType.Invalid;
                    }
                    return length == 14 ? CreditCardType.Diners : CreditCardType.AmericanExpress;
                case '4':
                    if ((length == 13) || (length == 16)) {
                        return CreditCardType.Visa;
                    }
                    break;
                case '5':
                    if (length == 16) {
                        return CreditCardType.MasterCard;
                    }
                    break;
                case '6':
                    if (length == 16) {
                        return CreditCardType.Discover;
                    }
                    break;
            }

            return CreditCardType.Invalid;
        }

        /// <see cref="http://en.wikipedia.org/wiki/Luhn"/>
        public static bool HasValidLuhnChecksum(string cardNumber) {
            if (string.IsNullOrWhiteSpace(cardNumber)) {
                return false;
            }

            cardNumber = SanitizeCardNumber(cardNumber);

            int i, sum, length = (cardNumber.Length - 1);

            for (i = 0, sum = 0, length = (cardNumber.Length - 1); i < length; i++) {
                int digit = cardNumber[length - i - 1] - '0';
                int doubleIfEven = digit * (1 + ((i + 1) % 2));
                sum += (doubleIfEven < 10 ? doubleIfEven : doubleIfEven - 9);
            }

            int lastDigit = (10 - (sum % 10)) % 10; 

            return lastDigit == (cardNumber[length] - '0');
        }
    }
}
