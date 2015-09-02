using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    public partial class PayeezyGatewayTests {
        [TestMethod]
        [ExpectedException(typeof(CardNumberNullException))]
        public void PayeezyCC_Purchase_CardNull() {
            GetReference().CreditCardPurchase(string.Empty, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyCC_Purchase_ExpMonthNull() {
            GetReference().CreditCardPurchase(validVisa, string.Empty, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyCC_Purchase_ExpYearNull() {
            GetReference().CreditCardPurchase(validVisa, month, "  ", dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardTypeNotSupportedException))]
        public void PayeezyCC_Purchase_CardTypeNotSupported() {
            GetReference().CreditCardPurchase(invalidCard, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardTypeNotSupportedException))]
        public void PayeezyCC_Purchase_CardTypeNotSupportedByGateway() {
            GetReference().CreditCardPurchase(validDiners, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardNumberInvalidException))]
        public void PayeezyCC_Purchase_CardNumberInvalid() {
            GetReference().CreditCardPurchase(invalidVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyCC_Purchase_ExpMonthNotNumeric() {
            GetReference().CreditCardPurchase(validVisa, "abc", twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyCC_Purchase_ExpMonthTooSmall() {
            GetReference().CreditCardPurchase(validVisa, "0", twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyCC_Purchase_ExpMonthTooLarge() {
            GetReference().CreditCardPurchase(validVisa, "13", twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyCC_Purchase_ExpYearNotNumeric() {
            GetReference().CreditCardPurchase(validVisa, month, "abc", dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyCC_Purchase_ExpYearNotFourDigit() {
            GetReference().CreditCardPurchase(validVisa, month, "-50", dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardExpiredException))]
        public void PayeezyCC_Purchase_CardExpired() {
            GetReference().CreditCardPurchase(validVisa, month, "01", dollarAmount1, cardHolderName, cvv, string.Empty);
        }
    }
}