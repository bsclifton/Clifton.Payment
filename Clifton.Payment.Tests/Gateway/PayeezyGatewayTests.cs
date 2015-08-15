using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway {
    [TestClass]
    public class PayeezyGatewayTests {
        protected const string cardHolderName = "Bubba Smith";
        protected const string validVisa = "4111111111111111";
        protected const string invalidCard = "1234567812345678";
        protected const string invalidVisa = "4111111111111112";
        protected const string twoDigitYear = "20";
        protected const string sampleDollarAmount = "125";

        private PayeezyGateway GetReference() {
            return new PayeezyGateway(
                apiKey: "y6pWAJNyJyjGv66IsVuWnklkKUPFbb0a",
                apiSecret: "86fbae7030253af3cd15faef2a1f4b67353e41fb6799f576b5093ae52901e6f7",
                token: "fdoa-a480ce8951daa73262734cf102641994c1e55e7cdf4c02b6",
                url: "https://api-cert.payeezy.com/v1/transactions"
            );
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardAuthorize_HappyPath() {
            var response = GetReference().CreditCardAuthorize(validVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", Guid.NewGuid().ToString());
            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Authorize, response.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardPurchase_HappyPath() {
            var response = GetReference().CreditCardPurchase(validVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardRefund_HappyPath() {
            var response = GetReference().CreditCardRefund(validVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardVoid_HappyPath() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", Guid.NewGuid().ToString());
            var voidResponse = GetReference().CreditCardVoid(authResponse.TransactionId, Guid.NewGuid().ToString(), authResponse.TransactionTag, sampleDollarAmount);

            Assert.IsNotNull(voidResponse.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, voidResponse.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Void, voidResponse.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardVoid_DifferentDollarAmount() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", Guid.NewGuid().ToString());
            var voidResponse = GetReference().CreditCardVoid(authResponse.TransactionId, Guid.NewGuid().ToString(), authResponse.TransactionTag, "12345");

            Assert.AreEqual(PayeezyGateway.TransactionStatus.NotProcessed, voidResponse.ParsedTransactionStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(CardNumberNullException))]
        public void PayeezyGateway_CreditCardPurchase_CardNull() {
            GetReference().CreditCardPurchase(string.Empty, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthNull() {
            GetReference().CreditCardPurchase(validVisa, string.Empty, twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyGateway_CreditCardPurchase_ExpYearNull() {
            GetReference().CreditCardPurchase(validVisa, "01", "  ", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardTypeNotSupportedException))]
        public void PayeezyGateway_CreditCardPurchase_CardTypeNotSupported() {
            GetReference().CreditCardPurchase(invalidCard, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardNumberInvalidException))]
        public void PayeezyGateway_CreditCardPurchase_CardNumberInvalid() {
            GetReference().CreditCardPurchase(invalidVisa, "01", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthNotNumeric() {
            GetReference().CreditCardPurchase(validVisa, "abc", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthTooSmall() {
            GetReference().CreditCardPurchase(validVisa, "0", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthTooLarge() {
            GetReference().CreditCardPurchase(validVisa, "13", twoDigitYear, sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyGateway_CreditCardPurchase_ExpYearNotNumeric() {
            GetReference().CreditCardPurchase(validVisa, "01", "abc", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyGateway_CreditCardPurchase_ExpYearNotFourDigit() {
            GetReference().CreditCardPurchase(validVisa, "01", "-50", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardExpiredException))]
        public void PayeezyGateway_CreditCardPurchase_CardExpired() {
            GetReference().CreditCardPurchase(validVisa, "01", "01", sampleDollarAmount, cardHolderName, "123", string.Empty);
        }
    }
}