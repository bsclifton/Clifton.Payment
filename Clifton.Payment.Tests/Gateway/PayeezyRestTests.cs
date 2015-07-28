using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway {
    [TestClass]
    public class PayeezyRestTests {
        protected const string cardHolderName = "Bubba Smith";
        protected const string validVisa = "4111111111111111";
        protected const string invalidCard = "1234567812345678";
        protected const string invalidVisa = "4111111111111112";
        protected const string twoDigitYear = "20";
        protected const string sampleDollarAmount = "125";

        [TestMethod]
        public void PayeezyRest_CreditCardPurchase_HappyPath() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void PayeezyRest_CreditCardRefund_HappyPath() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardRefund(validVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(CardNumberNullException))]
        public void PayeezyRest_CreditCardPurchase_CardNull() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(string.Empty, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyRest_CreditCardPurchase_ExpMonthNull() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, string.Empty, twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyRest_CreditCardPurchase_ExpYearNull() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "01", "  ", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardTypeNotSupportedException))]
        public void PayeezyRest_CreditCardPurchase_CardTypeNotSupported() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(invalidCard, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardNumberInvalidException))]
        public void PayeezyRest_CreditCardPurchase_CardNumberInvalid() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(invalidVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyRest_CreditCardPurchase_ExpMonthNotNumeric() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "abc", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyRest_CreditCardPurchase_ExpMonthTooSmall() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "0", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyRest_CreditCardPurchase_ExpMonthTooLarge() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "13", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyRest_CreditCardPurchase_ExpYearNotNumeric() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "01", "abc", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyRest_CreditCardPurchase_ExpYearNotFourDigit() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "01", "-50", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardExpiredException))]
        public void PayeezyRest_CreditCardPurchase_CardExpired() {
            PayeezyRest gateway = new PayeezyRest();
            gateway.CreditCardPurchase(validVisa, "01", "01", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }
    }
}