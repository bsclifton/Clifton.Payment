using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Clifton.Payment.Gateway;

namespace Clifton.Payment.Tests.Gateway.Payeezy {
    public partial class PayeezyGatewayTests {
        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeNullException))]
        public void PayeezyCC_Purchase_NullCVV() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, "", GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeFormatException))]
        public void PayeezyCC_Purchase_AmexWrongLengthCVV() {
            GetReference().CreditCardPurchase(validAmex, month, twoDigitYear, dollarAmount1, cardHolderName, "111", GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeFormatException))]
        public void PayeezyCC_Purchase_VisaWrongLengthCVV() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, "1111", GetReferenceNumber());
        }

        [TestMethod]
        [ExpectedException(typeof(CardSecurityCodeFormatException))]
        public void PayeezyCC_Purchase_NotNumericCVV() {
            GetReference().CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, "abc", GetReferenceNumber());
        }
    }
}