using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway {
    [TestClass]
    public class PayeezyGatewayTests {
        protected const string cardHolderName = "Bubba Smith";
        protected const string validVisa = "4111111111111111";
        protected const string validDiners = "36438936438936";
        protected const string validAmex = "373953192351004";
        protected const string invalidCard = "1234567812345678";
        protected const string invalidVisa = "4111111111111112";
        protected const string month = "01";
        protected const string twoDigitYear = "20";
        protected const string dollarAmount1 = "1.23";
        protected const string dollarAmount2 = "3.21";
        protected const string cvv = "123";

        private PayeezyGateway GetReference() {
            return new PayeezyGateway(
                apiKey: "y6pWAJNyJyjGv66IsVuWnklkKUPFbb0a",
                apiSecret: "86fbae7030253af3cd15faef2a1f4b67353e41fb6799f576b5093ae52901e6f7",
                token: "fdoa-a480ce8951daa73262734cf102641994c1e55e7cdf4c02b6",
                url: "https://api-cert.payeezy.com/v1/transactions"
            );
        }

        private string GetReferenceNumber() {
            return Guid.NewGuid().ToString();
        }

        #region happy path

        [TestMethod]
        public void PayeezyGateway_CreditCardAuthorize_HappyPath() {
            var response = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());

            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Authorize, response.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardPurchase_HappyPath() {
            var response = GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());

            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Purchase, response.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardRefund_HappyPath() {
            var response = GetReference().CreditCardRefund(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());

            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Refund, response.ParsedTransactionType);
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardVoid_HappyPath() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var voidResponse = GetReference().CreditCardVoid(authResponse.TransactionId, GetReferenceNumber(), authResponse.TransactionTag, dollarAmount1);

            Assert.IsNotNull(voidResponse.TransactionId);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, voidResponse.ParsedTransactionStatus);
            Assert.AreEqual(PayeezyGateway.TransactionType.Void, voidResponse.ParsedTransactionType);
        }

        #endregion

        [TestMethod]
        public void PayeezyGateway_CreditCardVoid_DifferentDollarAmount() {
            var authResponse = GetReference().CreditCardAuthorize(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, GetReferenceNumber());
            var voidResponse = GetReference().CreditCardVoid(authResponse.TransactionId, GetReferenceNumber(), authResponse.TransactionTag, dollarAmount2);

            Assert.AreEqual(PayeezyGateway.TransactionStatus.NotProcessed, voidResponse.ParsedTransactionStatus);
        }

        #region testing ValidateCreditCard

        [TestMethod]
        [ExpectedException(typeof(CardNumberNullException))]
        public void PayeezyGateway_CreditCardPurchase_CardNull() {
            GetReference().CreditCardPurchase(string.Empty, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthNull() {
            GetReference().CreditCardPurchase(validVisa, string.Empty, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationNullException))]
        public void PayeezyGateway_CreditCardPurchase_ExpYearNull() {
            GetReference().CreditCardPurchase(validVisa, month, "  ", dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardTypeNotSupportedException))]
        public void PayeezyGateway_CreditCardPurchase_CardTypeNotSupported() {
            GetReference().CreditCardPurchase(invalidCard, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardTypeNotSupportedException))]
        public void PayeezyGateway_CreditCardPurchase_CardTypeNotSupportedByGateway() {
            GetReference().CreditCardPurchase(validDiners, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardNumberInvalidException))]
        public void PayeezyGateway_CreditCardPurchase_CardNumberInvalid() {
            GetReference().CreditCardPurchase(invalidVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthNotNumeric() {
            GetReference().CreditCardPurchase(validVisa, "abc", twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthTooSmall() {
            GetReference().CreditCardPurchase(validVisa, "0", twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationOutOfRangeException))]
        public void PayeezyGateway_CreditCardPurchase_ExpMonthTooLarge() {
            GetReference().CreditCardPurchase(validVisa, "13", twoDigitYear, dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyGateway_CreditCardPurchase_ExpYearNotNumeric() {
            GetReference().CreditCardPurchase(validVisa, month, "abc", dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ExpirationFormatException))]
        public void PayeezyGateway_CreditCardPurchase_ExpYearNotFourDigit() {
            GetReference().CreditCardPurchase(validVisa, month, "-50", dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CardExpiredException))]
        public void PayeezyGateway_CreditCardPurchase_CardExpired() {
            GetReference().CreditCardPurchase(validVisa, month, "01", dollarAmount1, cardHolderName, cvv, string.Empty);
        }

        #endregion

        #region testing ValidateCardSecurityCode

        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeNullException))]
        public void PayeezyGateway_CreditCardPurchase_NullCVV() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, "", GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeFormatException))]
        public void PayeezyGateway_CreditCardPurchase_AmexWrongLengthCVV() {
            GetReference().CreditCardPurchase(validAmex, month, twoDigitYear, dollarAmount1, cardHolderName, "111", GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeFormatException))]
        public void PayeezyGateway_CreditCardPurchase_VisaWrongLengthCVV() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, "1111", GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeFormatException))]
        public void PayeezyGateway_CreditCardPurchase_NotNumericCVV() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, "abc", GetReferenceNumber());
        }

        #endregion

        #region testing GetUsDollarAmountAsCents

        [TestMethod]
        [ExpectedException(typeof(DollarAmountNullException))]
        public void PayeezyGateway_CreditCardPurchase_NullDollarAmount() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyGateway_CreditCardPurchase_MultipleDecimalPlaces() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "1.2.3", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyGateway_CreditCardPurchase_CentsNotTwoDigit() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "1.2", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyGateway_CreditCardPurchase_FailParseCents() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "1.aa", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        public void PayeezyGateway_CreditCardPurchase_HandlesCurrencySymbols() {
            var response = GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "$1,234.56", cardHolderName, cvv, GetReferenceNumber());
            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual("123456", response.Amount);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyGateway_CreditCardPurchase_FailParseDollars() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "aa.12", cardHolderName, cvv, GetReferenceNumber());
        }

        #endregion
    }
}