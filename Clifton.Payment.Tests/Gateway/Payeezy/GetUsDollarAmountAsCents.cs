using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    public partial class PayeezyGatewayTests {
        [TestMethod]
        [ExpectedException(typeof(DollarAmountNullException))]
        public void PayeezyCC_Purchase_NullDollarAmount() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyCC_Purchase_MultipleDecimalPlaces() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "1.2.3", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyCC_Purchase_CentsNotTwoDigit() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "1.2", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyCC_Purchase_FailParseCents() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "1.aa", cardHolderName, cvv, GetReferenceNumber());
        }

        [TestMethod]
        public void PayeezyCC_Purchase_HandlesCurrencySymbols() {
            var response = GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "$1,234.56", cardHolderName, cvv, GetReferenceNumber());
            Assert.IsNotNull(response.TransactionId);
            Assert.AreEqual("123456", response.Amount);
            Assert.AreEqual(PayeezyGateway.TransactionStatus.Approved, response.ParsedTransactionStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(DollarAmountInvalidException))]
        public void PayeezyCC_Purchase_FailParseDollars() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, "aa.12", cardHolderName, cvv, GetReferenceNumber());
        }
    }
}